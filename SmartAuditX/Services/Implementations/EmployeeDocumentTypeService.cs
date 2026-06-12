using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using System.Linq;

namespace SmartAuditX.Services.Implementations
{
    public class EmployeeDocumentTypeService : IEmployeeDocumentTypeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDocumentTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDocumentType>> GetAllAsync(int companyId)
        {
            return await _context.EmployeeDocumentTypes
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<EmployeeDocumentType?> GetByIdAsync(int id, int companyId)
        {
            return await _context.EmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.EmployeeDocumentTypeId == id && x.CompanyId == companyId && !x.IsDeleted);
        }

        public async Task<EmployeeDocumentType> CreateAsync(EmployeeDocumentType model)
        {
            _context.EmployeeDocumentTypes.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<EmployeeDocumentType?> UpdateAsync(int id, EmployeeDocumentType model, int companyId)
        {
            var existing = await _context.EmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.EmployeeDocumentTypeId == id && x.CompanyId == companyId && !x.IsDeleted);
            if (existing == null)
                return null;

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.IsRequired = model.IsRequired;
            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, int companyId)
        {
            var existing = await _context.EmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.EmployeeDocumentTypeId == id && x.CompanyId == companyId);
            if (existing == null)
                return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int id, int companyId)
        {
            var existing = await _context.EmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.EmployeeDocumentTypeId == id && x.CompanyId == companyId);
            if (existing == null)
                return false;

            existing.IsActive = !existing.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsNameExistsAsync(string name, int companyId, int? excludeId = null)
        {
            var query = _context.EmployeeDocumentTypes
                .Where(x => x.CompanyId == companyId && x.Name.Trim().ToLower() == name.Trim().ToLower() && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.EmployeeDocumentTypeId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<EmployeeDocumentType> items, int totalCount)> GetPagedListAsync(
            int companyId,
            int pageNumber,
            int pageSize,
            string? searchTerm,
            bool? isActive,
            string? sortColumn,
            string? sortOrder)
        {
            var query = _context.EmployeeDocumentTypes
                .Where(x => x.CompanyId == companyId && !x.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            // Get total count before paging
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                // We'll use a switch to avoid SQL injection and to ensure we only sort by allowed columns.
                // For simplicity, we'll allow sorting by Name, Description, IsRequired, IsActive, CreatedAt, UpdatedAt.
                // We'll use EF Core's OrderBy with a static expression.
                query = sortColumn.ToLower() switch
                {
                    "name" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "description" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                    "isrequired" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.IsRequired) : query.OrderBy(x => x.IsRequired),
                    "isactive" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                    "createdat" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    "updatedat" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
                    _ => query.OrderBy(x => x.Name) // default
                };
            }
            else
            {
                // Default sort by name
                query = query.OrderBy(x => x.Name);
            }

            // Apply paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}