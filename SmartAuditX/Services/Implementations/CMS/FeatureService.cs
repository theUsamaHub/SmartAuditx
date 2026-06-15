using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class FeatureService : IFeatureService
    {
        private readonly ApplicationDbContext _context;

        public FeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeatureVM>> GetAllAsync()
        {
            return await _context.Features
                .Select(f => new FeatureVM
                {
                    FeatureId = f.FeatureId,
                    Title = f.Title,
                    ShortDescription = f.ShortDescription,
                    IconName = f.IconName,
                    DisplayOrder = f.DisplayOrder,
                    IsActive = f.IsActive
                })
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FeatureVM?> GetByIdAsync(int id)
        {
            var f = await _context.Features.FindAsync(id);
            if (f == null) return null;
            
            return new FeatureVM
            {
                FeatureId = f.FeatureId,
                Title = f.Title,
                ShortDescription = f.ShortDescription,
                IconName = f.IconName,
                DisplayOrder = f.DisplayOrder,
                IsActive = f.IsActive
            };
        }

        public async Task<bool> CreateAsync(FeatureVM model)
        {
            var entity = new Feature
            {
                Title = model.Title,
                ShortDescription = model.ShortDescription,
                IconName = model.IconName,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Features.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(FeatureVM model)
        {
            var entity = await _context.Features.FindAsync(model.FeatureId);
            if (entity == null) return false;

            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.IconName = model.IconName;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Features.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Features.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Features.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.Features.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Features.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
