using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class PlatformModuleService : IPlatformModuleService
    {
        private readonly ApplicationDbContext _context;

        public PlatformModuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlatformModuleVM>> GetAllAsync()
        {
            return await _context.PlatformModules
                .Select(p => new PlatformModuleVM
                {
                    PlatformModuleId = p.PlatformModuleId,
                    Title = p.Title,
                    Description = p.Description,
                    IconName = p.IconName,
                    DisplayOrder = p.DisplayOrder,
                    IsActive = p.IsActive
                })
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PlatformModuleVM?> GetByIdAsync(int id)
        {
            var p = await _context.PlatformModules.FindAsync(id);
            if (p == null) return null;

            return new PlatformModuleVM
            {
                PlatformModuleId = p.PlatformModuleId,
                Title = p.Title,
                Description = p.Description,
                IconName = p.IconName,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive
            };
        }

        public async Task<bool> CreateAsync(PlatformModuleVM model)
        {
            var entity = new PlatformModule
            {
                Title = model.Title,
                Description = model.Description,
                IconName = model.IconName,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.PlatformModules.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(PlatformModuleVM model)
        {
            var entity = await _context.PlatformModules.FindAsync(model.PlatformModuleId);
            if (entity == null) return false;

            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.IconName = model.IconName;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.PlatformModules.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PlatformModules.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.PlatformModules.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.PlatformModules.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.PlatformModules.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
