using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DepartmentListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null)
        {
            var query = _context.Departments
                .Where(department => department.CompanyId == companyId);

            if (isActive.HasValue)
            {
                query = query.Where(department => department.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(department =>
                    department.Name.Contains(term) ||
                    department.Code.Contains(term) ||
                    (department.Description != null && department.Description.Contains(term)));
            }

            var departments = await query
                .OrderBy(department => department.Name)
                .Select(department => new
                {
                    department.DepartmentId,
                    department.Name,
                    department.Code,
                    department.Description,
                    department.IsActive,
                    department.CreatedAt,
                    department.UpdatedAt,
                    BranchLinkCount = department.BranchDepartments.Count,
                    EmployeeCount = _context.Employees.Count(employee =>
                        employee.CompanyId == companyId &&
                        employee.DepartmentId == department.DepartmentId &&
                        !employee.IsDeleted)
                })
                .ToListAsync();

            return departments
                .Select(department => new DepartmentListItemViewModel
                {
                    DepartmentId = department.DepartmentId,
                    Name = department.Name,
                    Code = department.Code,
                    Description = department.Description,
                    IsActive = department.IsActive,
                    BranchLinkCount = department.BranchLinkCount,
                    EmployeeCount = department.EmployeeCount,
                    CreatedAt = department.CreatedAt,
                    UpdatedAt = department.UpdatedAt
                })
                .ToList();
        }

        public async Task<DepartmentViewModel?> GetForEditAsync(int companyId, int departmentId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(item =>
                    item.DepartmentId == departmentId &&
                    item.CompanyId == companyId);

            if (department == null)
            {
                return null;
            }

            return new DepartmentViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                IsActive = department.IsActive
            };
        }

        public async Task<DepartmentOperationResult> CreateAsync(
            int companyId,
            DepartmentViewModel model)
        {
            var normalizedCode = NormalizeCode(model.Code);

            if (await CodeExistsAsync(companyId, normalizedCode))
            {
                return DepartmentOperationResult.Fail(
                    $"Department code '{normalizedCode}' is already in use.");
            }

            var department = new Department
            {
                CompanyId = companyId,
                Name = model.Name.Trim(),
                Code = normalizedCode,
                Description = NormalizeOptional(model.Description),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();

            return DepartmentOperationResult.Ok(
                "Department created successfully.",
                await MapToListItemAsync(companyId, department));
        }

        public async Task<DepartmentOperationResult> UpdateAsync(
            int companyId,
            int departmentId,
            DepartmentViewModel model)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(item =>
                    item.DepartmentId == departmentId &&
                    item.CompanyId == companyId);

            if (department == null)
            {
                return DepartmentOperationResult.Fail("Department not found.");
            }

            var normalizedCode = NormalizeCode(model.Code);

            if (await CodeExistsAsync(companyId, normalizedCode, departmentId))
            {
                return DepartmentOperationResult.Fail(
                    $"Department code '{normalizedCode}' is already in use.");
            }

            department.Name = model.Name.Trim();
            department.Code = normalizedCode;
            department.Description = NormalizeOptional(model.Description);
            department.IsActive = model.IsActive;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return DepartmentOperationResult.Ok(
                "Department updated successfully.",
                await MapToListItemAsync(companyId, department));
        }

        public async Task<DepartmentOperationResult> DeleteAsync(int companyId, int departmentId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(item =>
                    item.DepartmentId == departmentId &&
                    item.CompanyId == companyId);

            if (department == null)
            {
                return DepartmentOperationResult.Fail("Department not found.");
            }

            var employeeCount = await _context.Employees.CountAsync(employee =>
                employee.CompanyId == companyId &&
                employee.DepartmentId == departmentId &&
                !employee.IsDeleted);

            if (employeeCount > 0)
            {
                return DepartmentOperationResult.Fail(
                    "This department cannot be deleted because employees are assigned to it.");
            }

            var branchLinkCount = await _context.BranchDepartments
                .CountAsync(link =>
                    link.DepartmentId == departmentId &&
                    link.Branch.CompanyId == companyId);

            if (branchLinkCount > 0)
            {
                return DepartmentOperationResult.Fail(
                    "This department cannot be deleted because it is linked to one or more branches.");
            }

            department.IsDeleted = true;
            department.IsActive = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return DepartmentOperationResult.Ok("Department deleted successfully.");
        }

        public async Task<DepartmentOperationResult> ToggleActiveAsync(int companyId, int departmentId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(item =>
                    item.DepartmentId == departmentId &&
                    item.CompanyId == companyId);

            if (department == null)
            {
                return DepartmentOperationResult.Fail("Department not found.");
            }

            department.IsActive = !department.IsActive;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var message = department.IsActive
                ? "Department activated successfully."
                : "Department deactivated successfully.";

            return DepartmentOperationResult.Ok(
                message,
                await MapToListItemAsync(companyId, department));
        }

        private async Task<bool> CodeExistsAsync(
            int companyId,
            string code,
            int? excludeDepartmentId = null)
        {
            return await _context.Departments.AnyAsync(department =>
                department.CompanyId == companyId &&
                department.Code == code &&
                (!excludeDepartmentId.HasValue || department.DepartmentId != excludeDepartmentId.Value));
        }

        private async Task<DepartmentListItemViewModel> MapToListItemAsync(
            int companyId,
            Department department)
        {
            var branchLinkCount = await _context.BranchDepartments
                .CountAsync(link =>
                    link.DepartmentId == department.DepartmentId &&
                    link.Branch.CompanyId == companyId);

            var employeeCount = await _context.Employees
                .CountAsync(employee =>
                    employee.CompanyId == companyId &&
                    employee.DepartmentId == department.DepartmentId &&
                    !employee.IsDeleted);

            return new DepartmentListItemViewModel
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                IsActive = department.IsActive,
                BranchLinkCount = branchLinkCount,
                EmployeeCount = employeeCount,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
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
