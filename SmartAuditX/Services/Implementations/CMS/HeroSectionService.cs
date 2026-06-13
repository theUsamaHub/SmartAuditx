using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class HeroSectionService : IHeroSectionService
    {
        private readonly ApplicationDbContext _context;

        public HeroSectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HeroSectionVM>> GetAllAsync()
        {
            return await _context.HeroSections
                .Select(h => new HeroSectionVM
                {
                    HeroSectionId = h.HeroSectionId,
                    BadgeText = h.BadgeText,
                    Title = h.Title,
                    Description = h.Description,
                    PrimaryButtonText = h.PrimaryButtonText,
                    PrimaryButtonUrl = h.PrimaryButtonUrl,
                    SecondaryButtonText = h.SecondaryButtonText,
                    SecondaryButtonUrl = h.SecondaryButtonUrl,
                    IsActive = h.IsActive
                })
                .OrderByDescending(h => h.HeroSectionId)
                .ToListAsync();
        }

        public async Task<HeroSectionVM?> GetByIdAsync(int id)
        {
            var h = await _context.HeroSections.FindAsync(id);
            if (h == null) return null;

            return new HeroSectionVM
            {
                HeroSectionId = h.HeroSectionId,
                BadgeText = h.BadgeText,
                Title = h.Title,
                Description = h.Description,
                PrimaryButtonText = h.PrimaryButtonText,
                PrimaryButtonUrl = h.PrimaryButtonUrl,
                SecondaryButtonText = h.SecondaryButtonText,
                SecondaryButtonUrl = h.SecondaryButtonUrl,
                IsActive = h.IsActive
            };
        }

        public async Task<bool> CreateAsync(HeroSectionVM model)
        {
            if (model.IsActive)
            {
                await DeactivateOthersAsync();
            }

            var entity = new HeroSection
            {
                BadgeText = model.BadgeText,
                Title = model.Title,
                Description = model.Description,
                PrimaryButtonText = model.PrimaryButtonText,
                PrimaryButtonUrl = model.PrimaryButtonUrl,
                SecondaryButtonText = model.SecondaryButtonText,
                SecondaryButtonUrl = model.SecondaryButtonUrl,
                IsActive = model.IsActive,
                UpdatedAt = DateTime.UtcNow
            };

            _context.HeroSections.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(HeroSectionVM model)
        {
            var entity = await _context.HeroSections.FindAsync(model.HeroSectionId);
            if (entity == null) return false;

            if (model.IsActive && !entity.IsActive)
            {
                await DeactivateOthersAsync(model.HeroSectionId);
            }

            entity.BadgeText = model.BadgeText;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.PrimaryButtonText = model.PrimaryButtonText;
            entity.PrimaryButtonUrl = model.PrimaryButtonUrl;
            entity.SecondaryButtonText = model.SecondaryButtonText;
            entity.SecondaryButtonUrl = model.SecondaryButtonUrl;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.HeroSections.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.HeroSections.FindAsync(id);
            if (entity == null) return false;

            _context.HeroSections.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.HeroSections.FindAsync(id);
            if (entity == null) return false;

            if (!entity.IsActive)
            {
                await DeactivateOthersAsync(id);
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.HeroSections.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task DeactivateOthersAsync(int? excludeId = null)
        {
            var activeRecords = await _context.HeroSections
                .Where(h => h.IsActive && (excludeId == null || h.HeroSectionId != excludeId))
                .ToListAsync();

            foreach (var record in activeRecords)
            {
                record.IsActive = false;
                record.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
