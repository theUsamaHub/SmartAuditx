using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class DesignationService : IDesignationService
    {
        private readonly ApplicationDbContext _context;

        public DesignationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DesignationListItemViewModel>> GetAllAsync(
            int companyId,
            bool? isActive = null,
            string? search = null)
        {
            var query = _context.Designations
                .Where(designation => designation.CompanyId == companyId);

            if (isActive.HasValue)
            {
                query = query.Where(designation => designation.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(designation =>
                    designation.Name.Contains(term) ||
                    designation.Code.Contains(term) ||
                    (designation.Description != null && designation.Description.Contains(term)));
            }

            var designations = await query
                .OrderBy(designation => designation.Name)
                .Select(designation => new
                {
                    designation.DesignationId,
                    designation.Name,
                    designation.Code,
                    designation.Description,
                    designation.IsActive,
                    designation.CreatedAt,
                    designation.UpdatedAt,
                    EmployeeCount = _context.Employees.Count(employee =>
                        employee.CompanyId == companyId &&
                        employee.DesignationId == designation.DesignationId &&
                        !employee.IsDeleted)
                })
                .ToListAsync();

            return designations
                .Select(designation => new DesignationListItemViewModel
                {
                    DesignationId = designation.DesignationId,
                    Name = designation.Name,
                    Code = designation.Code,
                    Description = designation.Description,
                    IsActive = designation.IsActive,
                    EmployeeCount = designation.EmployeeCount,
                    CreatedAt = designation.CreatedAt,
                    UpdatedAt = designation.UpdatedAt
                })
                .ToList();
        }

        public async Task<DesignationViewModel?> GetForEditAsync(int companyId, int designationId)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(item =>
                    item.DesignationId == designationId &&
                    item.CompanyId == companyId);

            if (designation == null)
            {
                return null;
            }

            return new DesignationViewModel
            {
                DesignationId = designation.DesignationId,
                Name = designation.Name,
                Code = designation.Code,
                Description = designation.Description,
                IsActive = designation.IsActive
            };
        }

        public async Task<DesignationOperationResult> CreateAsync(
            int companyId,
            DesignationViewModel model)
        {
            var normalizedCode = NormalizeCode(model.Code);

            if (await CodeExistsAsync(companyId, normalizedCode))
            {
                return DesignationOperationResult.Fail(
                    $"Designation code '{normalizedCode}' is already in use.");
            }

            var designation = new Designation
            {
                CompanyId = companyId,
                Name = model.Name.Trim(),
                Code = normalizedCode,
                Description = NormalizeOptional(model.Description),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Designations.AddAsync(designation);
            await _context.SaveChangesAsync();

            return DesignationOperationResult.Ok(
                "Designation created successfully.",
                await MapToListItemAsync(companyId, designation));
        }

        public async Task<DesignationOperationResult> UpdateAsync(
            int companyId,
            int designationId,
            DesignationViewModel model)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(item =>
                    item.DesignationId == designationId &&
                    item.CompanyId == companyId);

            if (designation == null)
            {
                return DesignationOperationResult.Fail("Designation not found.");
            }

            var normalizedCode = NormalizeCode(model.Code);

            if (await CodeExistsAsync(companyId, normalizedCode, designationId))
            {
                return DesignationOperationResult.Fail(
                    $"Designation code '{normalizedCode}' is already in use.");
            }

            designation.Name = model.Name.Trim();
            designation.Code = normalizedCode;
            designation.Description = NormalizeOptional(model.Description);
            designation.IsActive = model.IsActive;
            designation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return DesignationOperationResult.Ok(
                "Designation updated successfully.",
                await MapToListItemAsync(companyId, designation));
        }

        public async Task<DesignationOperationResult> DeleteAsync(int companyId, int designationId)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(item =>
                    item.DesignationId == designationId &&
                    item.CompanyId == companyId);

            if (designation == null)
            {
                return DesignationOperationResult.Fail("Designation not found.");
            }

            var employeeCount = await _context.Employees.CountAsync(employee =>
                employee.CompanyId == companyId &&
                employee.DesignationId == designationId &&
                !employee.IsDeleted);

            if (employeeCount > 0)
            {
                return DesignationOperationResult.Fail(
                    "This designation cannot be deleted because employees are assigned to it.");
            }

            designation.IsDeleted = true;
            designation.IsActive = false;
            designation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return DesignationOperationResult.Ok("Designation deleted successfully.");
        }

        public async Task<DesignationOperationResult> ToggleActiveAsync(int companyId, int designationId)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(item =>
                    item.DesignationId == designationId &&
                    item.CompanyId == companyId);

            if (designation == null)
            {
                return DesignationOperationResult.Fail("Designation not found.");
            }

            designation.IsActive = !designation.IsActive;
            designation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var message = designation.IsActive
                ? "Designation activated successfully."
                : "Designation deactivated successfully.";

            return DesignationOperationResult.Ok(
                message,
                await MapToListItemAsync(companyId, designation));
        }

        private async Task<bool> CodeExistsAsync(
            int companyId,
            string code,
            int? excludeDesignationId = null)
        {
            return await _context.Designations.AnyAsync(designation =>
                designation.CompanyId == companyId &&
                designation.Code == code &&
                (!excludeDesignationId.HasValue || designation.DesignationId != excludeDesignationId.Value));
        }

        private async Task<DesignationListItemViewModel> MapToListItemAsync(
            int companyId,
            Designation designation)
        {
            var employeeCount = await _context.Employees
                .CountAsync(employee =>
                    employee.CompanyId == companyId &&
                    employee.DesignationId == designation.DesignationId &&
                    !employee.IsDeleted);

            return new DesignationListItemViewModel
            {
                DesignationId = designation.DesignationId,
                Name = designation.Name,
                Code = designation.Code,
                Description = designation.Description,
                IsActive = designation.IsActive,
                EmployeeCount = employeeCount,
                CreatedAt = designation.CreatedAt,
                UpdatedAt = designation.UpdatedAt
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
