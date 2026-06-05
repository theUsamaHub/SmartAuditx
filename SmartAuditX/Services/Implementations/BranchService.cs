using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BranchListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null)
        {
            var query = _context.Branches
                .Where(branch => branch.CompanyId == companyId);

            if (isActive.HasValue)
            {
                query = query.Where(branch => branch.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(branch =>
                    branch.BranchName.Contains(term) ||
                    branch.BranchCode.Contains(term) ||
                    (branch.PhysicalAddress != null && branch.PhysicalAddress.Contains(term)));
            }

            var branches = await query
                .OrderBy(branch => branch.BranchName)
                .Select(branch => new
                {
                    branch.BranchId,
                    branch.BranchName,
                    branch.BranchCode,
                    branch.Email,
                    branch.PhoneNumber,
                    branch.PhysicalAddress,
                    branch.IsHeadOffice,
                    branch.IsActive,
                    branch.CreatedAt,
                    branch.UpdatedAt,
                    DepartmentCount = branch.BranchDepartments.Count,
                    EmployeeCount = _context.Employees.Count(employee =>
                        employee.CompanyId == companyId &&
                        employee.BranchId == branch.BranchId &&
                        !employee.IsDeleted)
                })
                .ToListAsync();

            return branches
                .Select(branch => new BranchListItemViewModel
                {
                    BranchId = branch.BranchId,
                    BranchName = branch.BranchName,
                    BranchCode = branch.BranchCode,
                    Email = branch.Email,
                    PhoneNumber = branch.PhoneNumber,
                    PhysicalAddress = branch.PhysicalAddress,
                    IsHeadOffice = branch.IsHeadOffice,
                    IsActive = branch.IsActive,
                    DepartmentCount = branch.DepartmentCount,
                    EmployeeCount = branch.EmployeeCount,
                    CreatedAt = branch.CreatedAt,
                    UpdatedAt = branch.UpdatedAt
                })
                .ToList();
        }

        public async Task<BranchViewModel?> GetForEditAsync(int companyId, int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(item =>
                    item.BranchId == branchId &&
                    item.CompanyId == companyId);

            if (branch == null)
            {
                return null;
            }

            return new BranchViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                Email = branch.Email,
                PhoneNumber = branch.PhoneNumber,
                PhysicalAddress = branch.PhysicalAddress,
                IsHeadOffice = branch.IsHeadOffice,
                IsActive = branch.IsActive
            };
        }

        public async Task<BranchOperationResult> CreateAsync(
            int companyId,
            BranchViewModel model)
        {
            var normalizedCode = NormalizeCode(model.BranchCode);

            if (await CodeExistsAsync(companyId, normalizedCode))
            {
                return BranchOperationResult.Fail(
                    $"Branch code '{normalizedCode}' is already in use.");
            }

            if (model.IsHeadOffice && await HeadOfficeExistsAsync(companyId))
            {
                return BranchOperationResult.Fail(
                    "A head office already exists for this company. Only one head office is allowed.");
            }

            var branch = new Branch
            {
                CompanyId = companyId,
                BranchName = model.BranchName.Trim(),
                BranchCode = normalizedCode,
                Email = NormalizeOptional(model.Email),
                PhoneNumber = NormalizeOptional(model.PhoneNumber),
                PhysicalAddress = NormalizeOptional(model.PhysicalAddress),
                IsHeadOffice = model.IsHeadOffice,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Branches.AddAsync(branch);
            await _context.SaveChangesAsync();

            return BranchOperationResult.Ok(
                "Branch created successfully.",
                await MapToListItemAsync(companyId, branch));
        }

        public async Task<BranchOperationResult> UpdateAsync(
            int companyId,
            int branchId,
            BranchViewModel model)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(item =>
                    item.BranchId == branchId &&
                    item.CompanyId == companyId);

            if (branch == null)
            {
                return BranchOperationResult.Fail("Branch not found.");
            }

            var normalizedCode = NormalizeCode(model.BranchCode);

            if (await CodeExistsAsync(companyId, normalizedCode, branchId))
            {
                return BranchOperationResult.Fail(
                    $"Branch code '{normalizedCode}' is already in use.");
            }

            if (model.IsHeadOffice && !branch.IsHeadOffice && await HeadOfficeExistsAsync(companyId, branchId))
            {
                return BranchOperationResult.Fail(
                    "A head office already exists for this company. Only one head office is allowed.");
            }

            branch.BranchName = model.BranchName.Trim();
            branch.BranchCode = normalizedCode;
            branch.Email = NormalizeOptional(model.Email);
            branch.PhoneNumber = NormalizeOptional(model.PhoneNumber);
            branch.PhysicalAddress = NormalizeOptional(model.PhysicalAddress);
            branch.IsHeadOffice = model.IsHeadOffice;
            branch.IsActive = model.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BranchOperationResult.Ok(
                "Branch updated successfully.",
                await MapToListItemAsync(companyId, branch));
        }

        public async Task<BranchOperationResult> DeleteAsync(int companyId, int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(item =>
                    item.BranchId == branchId &&
                    item.CompanyId == companyId);

            if (branch == null)
            {
                return BranchOperationResult.Fail("Branch not found.");
            }

            var employeeCount = await _context.Employees.CountAsync(employee =>
                employee.CompanyId == companyId &&
                employee.BranchId == branchId &&
                !employee.IsDeleted);

            if (employeeCount > 0)
            {
                return BranchOperationResult.Fail(
                    "This branch cannot be deleted because employees are assigned to it.");
            }

            var departmentLinkCount = await _context.BranchDepartments
                .CountAsync(link => link.BranchId == branchId);

            if (departmentLinkCount > 0)
            {
                return BranchOperationResult.Fail(
                    "This branch cannot be deleted because departments are linked to it. Remove department links first.");
            }

            branch.IsDeleted = true;
            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BranchOperationResult.Ok("Branch deleted successfully.");
        }

        public async Task<BranchOperationResult> ToggleActiveAsync(int companyId, int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(item =>
                    item.BranchId == branchId &&
                    item.CompanyId == companyId);

            if (branch == null)
            {
                return BranchOperationResult.Fail("Branch not found.");
            }

            branch.IsActive = !branch.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var message = branch.IsActive
                ? "Branch activated successfully."
                : "Branch deactivated successfully.";

            return BranchOperationResult.Ok(
                message,
                await MapToListItemAsync(companyId, branch));
        }

        private async Task<bool> CodeExistsAsync(
            int companyId,
            string code,
            int? excludeBranchId = null)
        {
            return await _context.Branches.AnyAsync(branch =>
                branch.CompanyId == companyId &&
                branch.BranchCode == code &&
                (!excludeBranchId.HasValue || branch.BranchId != excludeBranchId.Value));
        }

        private async Task<bool> HeadOfficeExistsAsync(
            int companyId,
            int? excludeBranchId = null)
        {
            return await _context.Branches.AnyAsync(branch =>
                branch.CompanyId == companyId &&
                branch.IsHeadOffice &&
                (!excludeBranchId.HasValue || branch.BranchId != excludeBranchId.Value));
        }

        private async Task<BranchListItemViewModel> MapToListItemAsync(
            int companyId,
            Branch branch)
        {
            var departmentCount = await _context.BranchDepartments
                .CountAsync(link => link.BranchId == branch.BranchId);

            var employeeCount = await _context.Employees
                .CountAsync(employee =>
                    employee.CompanyId == companyId &&
                    employee.BranchId == branch.BranchId &&
                    !employee.IsDeleted);

            return new BranchListItemViewModel
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                Email = branch.Email,
                PhoneNumber = branch.PhoneNumber,
                PhysicalAddress = branch.PhysicalAddress,
                IsHeadOffice = branch.IsHeadOffice,
                IsActive = branch.IsActive,
                DepartmentCount = departmentCount,
                EmployeeCount = employeeCount,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt
            };
        }

        private static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();

        private static string? NormalizeOptional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
