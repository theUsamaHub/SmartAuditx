using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class SecurityFeatureService : ISecurityFeatureService
    {
        private readonly ApplicationDbContext _context;

        public SecurityFeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SecurityFeatureVM>> GetAllAsync()
        {
            return await _context.SecurityFeatures
                .Select(s => new SecurityFeatureVM
                {
                    SecurityFeatureId = s.SecurityFeatureId,
                    Title = s.Title,
                    Description = s.Description,
                    IconName = s.IconName,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive
                })
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<SecurityFeatureVM?> GetByIdAsync(int id)
        {
            var s = await _context.SecurityFeatures.FindAsync(id);
            if (s == null) return null;

            return new SecurityFeatureVM
            {
                SecurityFeatureId = s.SecurityFeatureId,
                Title = s.Title,
                Description = s.Description,
                IconName = s.IconName,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            };
        }

        public async Task<bool> CreateAsync(SecurityFeatureVM model)
        {
            var entity = new SecurityFeature
            {
                Title = model.Title,
                Description = model.Description,
                IconName = model.IconName,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.SecurityFeatures.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(SecurityFeatureVM model)
        {
            var entity = await _context.SecurityFeatures.FindAsync(model.SecurityFeatureId);
            if (entity == null) return false;

            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.IconName = model.IconName;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.SecurityFeatures.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.SecurityFeatures.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.SecurityFeatures.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.SecurityFeatures.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.SecurityFeatures.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
