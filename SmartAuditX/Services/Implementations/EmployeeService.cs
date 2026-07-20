using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public EmployeeService(
            ApplicationDbContext context,
           
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IReadOnlyList<EmployeeListItemViewModel>> GetAllAsync(
            int companyId,
            int? branchId = null,
            int? departmentId = null,
            int? designationId = null,
            bool? isActive = null,
            string? search = null)
        {
            var query = _context.Employees
                .Where(e => e.CompanyId == companyId && !e.IsDeleted);

            // Apply filters
            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (designationId.HasValue)
                query = query.Where(e => e.DesignationId == designationId.Value);

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e =>
                    e.FirstName.Contains(term) ||
                    e.LastName.Contains(term) ||
                    e.EmployeeCode.Contains(term) ||
                    (e.PersonalEmail != null && e.PersonalEmail.Contains(term)) ||
                    (e.CNICOrNationalId != null && e.CNICOrNationalId.Contains(term)));
            }

            var employees = await query
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new EmployeeListItemViewModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FirstName + " " + (e.LastName ?? ""),
                    Email = e.PersonalEmail,
                    Phone = e.PersonalPhone,
                    BranchName = e.Branch != null ? e.Branch.BranchName : null,
                    DepartmentName = e.Department != null ? e.Department.Name : null,
                    DesignationName = e.Designation != null ? e.Designation.Name : null,
                    IsActive = e.IsActive,
                    IsSystemUser = e.IsSystemUser,
                    JoiningDate = e.JoiningDate,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return employees;
        }

        public async Task<EmployeeViewModel?> GetForEditAsync(int companyId, int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return null;

            var viewModel = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                CNICOrNationalId = employee.CNICOrNationalId,
                PersonalEmail = employee.PersonalEmail,
                PersonalPhone = employee.PersonalPhone,
                BranchId = employee.BranchId,
                DepartmentId = employee.DepartmentId,
                DesignationId = employee.DesignationId,
                JoiningDate = employee.JoiningDate,
                IsSystemUser = employee.IsSystemUser,
                IsActive = employee.IsActive,
                EmployeeCode = employee.EmployeeCode
            };

            // If system user, get the associated Identity user email and phone
            if (employee.IsSystemUser)
            {
                var identityUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.CompanyId == companyId && !u.IsDeleted);

                if (identityUser != null)
                {
                    viewModel.SystemEmail = identityUser.Email;
                    viewModel.SystemPhoneDialCode = identityUser.PhoneDialCode;
                    viewModel.SystemPhoneNumber = identityUser.PhoneNumber;
                }
            }

            return viewModel;
        }

        public async Task<EmployeeOperationResult> CreateAsync(int companyId, EmployeeViewModel model)
        {
            // Validate duplicate email
            if (!string.IsNullOrWhiteSpace(model.PersonalEmail))
            {
                var emailExists = await _context.Employees
                    .AnyAsync(e => e.CompanyId == companyId && e.PersonalEmail == model.PersonalEmail && !e.IsDeleted);

                if (emailExists)
                    return EmployeeOperationResult.Fail("An employee with this email already exists.");
            }

            // Validate duplicate CNIC
            if (!string.IsNullOrWhiteSpace(model.CNICOrNationalId))
            {
                var cnicExists = await _context.Employees
                    .AnyAsync(e => e.CompanyId == companyId && e.CNICOrNationalId == model.CNICOrNationalId && !e.IsDeleted);

                if (cnicExists)
                    return EmployeeOperationResult.Fail("An employee with this CNIC/National ID already exists.");
            }

            // Validate duplicate phone
            if (!string.IsNullOrWhiteSpace(model.PersonalPhone))
            {
                var normalizedPhone = model.PersonalPhone.Replace(" ", "").Replace("-", "").Trim();
                var phoneExists = await _context.Employees
                    .AnyAsync(e => e.CompanyId == companyId && e.PersonalPhone != null && e.PersonalPhone.Replace(" ", "").Replace("-", "").Trim() == normalizedPhone && !e.IsDeleted);

                if (phoneExists)
                    return EmployeeOperationResult.Fail("An employee with this phone number already exists.");
            }

            // Generate Employee Code
            var employeeCode = await GenerateEmployeeCodeAsync(companyId);

            // Create Employee (always without system user - system user is created separately)
            var employee = new Employee
            {
                CompanyId = companyId,
                EmployeeCode = employeeCode,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName?.Trim(),
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                CNICOrNationalId = model.CNICOrNationalId?.Trim(),
                PersonalEmail = model.PersonalEmail?.Trim(),
                PersonalPhone = model.PersonalPhone?.Trim(),
                BranchId = model.BranchId,
                DepartmentId = model.DepartmentId,
                DesignationId = model.DesignationId,
                JoiningDate = model.JoiningDate,
                IsSystemUser = false,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return EmployeeOperationResult.Ok(
                "Employee created successfully.",
                await MapToListItemAsync(companyId, employee));
        }

        public async Task<EmployeeOperationResult> UpdateAsync(int companyId, int employeeId, EmployeeViewModel model)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return EmployeeOperationResult.Fail("Employee not found.");

            // Validate duplicate email (exclude current employee)
            if (!string.IsNullOrWhiteSpace(model.PersonalEmail))
            {
                var emailExists = await _context.Employees
                    .AnyAsync(e => e.CompanyId == companyId && e.PersonalEmail == model.PersonalEmail && e.EmployeeId != employeeId && !e.IsDeleted);

                if (emailExists)
                    return EmployeeOperationResult.Fail("An employee with this email already exists.");
            }

            // Validate duplicate CNIC (exclude current employee)
            if (!string.IsNullOrWhiteSpace(model.CNICOrNationalId))
            {
                var cnicExists = await _context.Employees
                    .AnyAsync(e => e.CompanyId == companyId && e.CNICOrNationalId == model.CNICOrNationalId && e.EmployeeId != employeeId && !e.IsDeleted);

                if (cnicExists)
                    return EmployeeOperationResult.Fail("An employee with this CNIC/National ID already exists.");
            }

            // Update Employee fields
            employee.FirstName = model.FirstName.Trim();
            employee.LastName = model.LastName?.Trim();
            employee.Gender = model.Gender;
            employee.DateOfBirth = model.DateOfBirth;
            employee.CNICOrNationalId = model.CNICOrNationalId?.Trim();
            employee.PersonalEmail = model.PersonalEmail?.Trim();
            // Use system phone for PersonalPhone if system user, otherwise use personal phone
            employee.PersonalPhone = model.IsSystemUser && !string.IsNullOrWhiteSpace(model.SystemPhoneNumber)
                ? model.SystemPhoneNumber.Trim()
                : model.PersonalPhone?.Trim();
            employee.BranchId = model.BranchId;
            employee.DepartmentId = model.DepartmentId;
            employee.DesignationId = model.DesignationId;
            employee.JoiningDate = model.JoiningDate;
            employee.IsActive = model.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;

            // Handle IsSystemUser changes
            if (model.IsSystemUser && !employee.IsSystemUser)
            {
                // Converting non-system user to system user
                var validationResult = await ValidateSystemUserInput(model);
                if (!validationResult.Success)
                    return validationResult;

                var identityResult = await CreateIdentityUserAsync(employee, model);
                if (!identityResult.Success)
                    return identityResult;

                employee.IsSystemUser = true;
            }
            else if (model.IsSystemUser && employee.IsSystemUser)
            {
                // Updating existing system user - check if email changed
                var identityUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.CompanyId == companyId && !u.IsDeleted);

                if (identityUser != null && !string.IsNullOrWhiteSpace(model.SystemEmail))
                {
                    if (identityUser.Email != model.SystemEmail)
                    {
                        var emailCheck = await _userManager.FindByEmailAsync(model.SystemEmail);
                        if (emailCheck != null && emailCheck.Id != identityUser.Id)
                            return EmployeeOperationResult.Fail("This email is already in use by another user.");

                        identityUser.Email = model.SystemEmail;
                        identityUser.UserName = model.SystemEmail;
                        identityUser.PhoneNumber = model.SystemPhoneNumber?.Trim();
                        identityUser.PhoneDialCode = model.SystemPhoneDialCode?.Trim();

                        identityUser.NormalizedEmail = model.SystemEmail.ToUpperInvariant();
                        identityUser.NormalizedUserName = model.SystemEmail.ToUpperInvariant();
                        // Keep EmailConfirmed as true since admin is managing the account
                        // If you want to require re-verification, set: identityUser.EmailConfirmed = false;
                        identityUser.UpdatedAt = DateTime.UtcNow;

                        var updateResult = await _userManager.UpdateAsync(identityUser);
                        if (!updateResult.Succeeded)
                            return EmployeeOperationResult.Fail("Failed to update system user account.");
                    }

                    // Update password if provided
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var passwordResult = await _userManager.ResetPasswordAsync(identityUser, passwordToken, model.Password);
                        if (!passwordResult.Succeeded)
                            return EmployeeOperationResult.Fail("Failed to update password.");
                    }

                    // Update role if changed
                    if (!string.IsNullOrWhiteSpace(model.Role))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(identityUser);
                        if (!currentRoles.Contains(model.Role))
                        {
                            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                            await _userManager.AddToRoleAsync(identityUser, model.Role);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return EmployeeOperationResult.Ok(
                "Employee updated successfully.",
                await MapToListItemAsync(companyId, employee));
        }

        public async Task<EmployeeOperationResult> DeleteAsync(int companyId, int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return EmployeeOperationResult.Fail("Employee not found.");

            // Check if employee has documents
            var documentCount = await _context.EmployeeDocuments
                .CountAsync(d => d.EmployeeId == employeeId);

            if (documentCount > 0)
            {
                return EmployeeOperationResult.Fail(
                    "This employee cannot be deleted because they have uploaded documents. Remove documents first.");
            }

            // Soft delete employee
            employee.IsDeleted = true;
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;

            // If system user, deactivate Identity user
            if (employee.IsSystemUser)
            {
                var identityUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.CompanyId == companyId && !u.IsDeleted);

                if (identityUser != null)
                {
                    identityUser.IsActive = false;
                    identityUser.IsDeleted = true;
                    identityUser.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();

            return EmployeeOperationResult.Ok("Employee deleted successfully.");
        }

        public async Task<EmployeeOperationResult> ToggleActiveAsync(int companyId, int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return EmployeeOperationResult.Fail("Employee not found.");

            employee.IsActive = !employee.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;

            // Update Identity user status if system user
            if (employee.IsSystemUser)
            {
                var identityUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.CompanyId == companyId && !u.IsDeleted);

                if (identityUser != null)
                {
                    identityUser.IsActive = employee.IsActive;
                    identityUser.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();

            var message = employee.IsActive
                ? "Employee activated successfully."
                : "Employee deactivated successfully.";

            return EmployeeOperationResult.Ok(
                message,
                await MapToListItemAsync(companyId, employee));
        }

        public async Task<EmployeeOperationResult> CreateSystemUserAsync(int companyId, int employeeId, CreateSystemUserViewModel model)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return EmployeeOperationResult.Fail("Employee not found.");

            if (employee.IsSystemUser)
                return EmployeeOperationResult.Fail("This employee already has a system user account.");

            // Validate email uniqueness
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return EmployeeOperationResult.Fail("This email is already registered as a system user.");

            // Validate phone number uniqueness
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var normalizedPhone = model.PhoneNumber.Replace(" ", "").Replace("-", "").Trim();
                var phoneExists = await _userManager.Users
                    .AnyAsync(u => u.PhoneNumber == normalizedPhone && !u.IsDeleted);
                if (phoneExists)
                    return EmployeeOperationResult.Fail("This phone number is already registered to another user.");
            }

            // Validate role
            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                    return EmployeeOperationResult.Fail($"The role '{model.Role}' does not exist.");

                var excludedRoles = new[] { "SystemAdmin", "CompanyOwner" };
                if (excludedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
                    return EmployeeOperationResult.Fail($"The role '{model.Role}' is not allowed for employees.");
            }

            // Create Identity User
            var user = new ApplicationUser
            {
                CompanyId = companyId,
                EmployeeId = employeeId,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneDialCode = model.PhoneDialCode?.Trim() ?? "+92",
                PhoneNumber = model.PhoneNumber?.Trim() ?? "",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return EmployeeOperationResult.Fail($"Failed to create system user: {errors}");
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return EmployeeOperationResult.Fail("A user with this phone number or email already exists.");
            }

            // Assign role
            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Employee");
            }

            // Update employee record
            employee.IsSystemUser = true;
            employee.PersonalPhone = $"{model.PhoneDialCode}{model.PhoneNumber}".Trim();
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return EmployeeOperationResult.Ok(
                "System user account created successfully.",
                await MapToListItemAsync(companyId, employee));
        }

        // Private helper methods

        private async Task<string> GenerateEmployeeCodeAsync(int companyId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            var companyShortCode = company?.Name?.Length >= 3
                ? company.Name[..3].ToUpperInvariant()
                : companyId.ToString();

            var existingCodes = await _context.Employees
                .Where(e => e.CompanyId == companyId)
                .Select(e => e.EmployeeCode)
                .ToListAsync();

            int maxNumber = 0;

            foreach (var code in existingCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                var parts = code.Split('-');

                if (parts.Length == 3 &&
                    int.TryParse(parts[2], out int number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }

            return $"Emp-{companyShortCode}-{(maxNumber + 1):D4}";
        }

        private async Task<EmployeeOperationResult> ValidateSystemUserInput(EmployeeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SystemEmail))
                return EmployeeOperationResult.Fail("System email is required for system users.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return EmployeeOperationResult.Fail("Password is required for system users.");

            if (model.Password != model.ConfirmPassword)
                return EmployeeOperationResult.Fail("Password and confirmation password do not match.");

            // Check if email already exists in Identity
            var existingUser = await _userManager.FindByEmailAsync(model.SystemEmail);
            if (existingUser != null)
                return EmployeeOperationResult.Fail("This email is already registered as a system user.");

            // Validate role
            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                    return EmployeeOperationResult.Fail($"The role '{model.Role}' does not exist.");

                // Exclude SystemAdmin and CompanyOwner roles
                var excludedRoles = new[] { "SystemAdmin", "CompanyOwner" };
                if (excludedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
                    return EmployeeOperationResult.Fail($"The role '{model.Role}' is not allowed for employees.");
            }

            return EmployeeOperationResult.Ok("Validation passed.");
        }

        private async Task<EmployeeOperationResult> CreateIdentityUserAsync(Employee employee, EmployeeViewModel model)
        {
            var user = new ApplicationUser
            {
                CompanyId = employee.CompanyId,
                EmployeeId = employee.EmployeeId,
                UserName = model.SystemEmail,
                Email = model.SystemEmail,
                EmailConfirmed = true, // Auto-confirm since admin is creating the account
                PhoneDialCode = model.SystemPhoneDialCode?.Trim(),
                PhoneNumber = model.SystemPhoneNumber?.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return EmployeeOperationResult.Fail($"Failed to create system user: {errors}");
            }

            // Assign role if provided
            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            else
            {
                // Default to "Employee" role
                await _userManager.AddToRoleAsync(user, "Employee");
            }

            return EmployeeOperationResult.Ok("Identity user created successfully.");
        }

        public async Task<EmployeeOperationResult> RemoveSystemUserAsync(int companyId, int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId && !e.IsDeleted);

            if (employee == null)
                return EmployeeOperationResult.Fail("Employee not found.");

            if (!employee.IsSystemUser)
                return EmployeeOperationResult.Fail("This employee does not have a system user account.");

            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.CompanyId == companyId && !u.IsDeleted);

            if (identityUser != null)
            {
                // Remove all roles first
                var currentRoles = await _userManager.GetRolesAsync(identityUser);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                }

                // Deactivate the user instead of deleting
                identityUser.IsActive = false;
                identityUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            employee.IsSystemUser = false;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return EmployeeOperationResult.Ok("System user account removed successfully.");
        }

        private async Task<EmployeeListItemViewModel> MapToListItemAsync(int companyId, Employee employee)
        {
            var branchName = employee.BranchId.HasValue
                ? await _context.Branches
                    .Where(b => b.BranchId == employee.BranchId && b.CompanyId == companyId)
                    .Select(b => b.BranchName)
                    .FirstOrDefaultAsync()
                : null;

            var departmentName = employee.DepartmentId.HasValue
                ? await _context.Departments
                    .Where(d => d.DepartmentId == employee.DepartmentId && d.CompanyId == companyId)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync()
                : null;

            var designationName = employee.DesignationId.HasValue
                ? await _context.Designations
                    .Where(d => d.DesignationId == employee.DesignationId && d.CompanyId == companyId)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync()
                : null;

            return new EmployeeListItemViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FirstName + " " + (employee.LastName ?? ""),
                Email = employee.PersonalEmail,
                Phone = employee.PersonalPhone,
                BranchName = branchName,
                DepartmentName = departmentName,
                DesignationName = designationName,
                IsActive = employee.IsActive,
                IsSystemUser = employee.IsSystemUser,
                JoiningDate = employee.JoiningDate,
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
