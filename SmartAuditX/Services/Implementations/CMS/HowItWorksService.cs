using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class HowItWorksService : IHowItWorksService
    {
        private readonly ApplicationDbContext _context;

        public HowItWorksService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HowItWorksStepVM>> GetAllAsync()
        {
            return await _context.HowItWorksSteps
                .Select(s => new HowItWorksStepVM
                {
                    HowItWorksStepId = s.HowItWorksStepId,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive
                })
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<HowItWorksStepVM?> GetByIdAsync(int id)
        {
            var s = await _context.HowItWorksSteps.FindAsync(id);
            if (s == null) return null;

            return new HowItWorksStepVM
            {
                HowItWorksStepId = s.HowItWorksStepId,
                StepNumber = s.StepNumber,
                Title = s.Title,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            };
        }

        public async Task<bool> CreateAsync(HowItWorksStepVM model)
        {
            var entity = new HowItWorksStep
            {
                StepNumber = model.StepNumber,
                Title = model.Title,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.HowItWorksSteps.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(HowItWorksStepVM model)
        {
            var entity = await _context.HowItWorksSteps.FindAsync(model.HowItWorksStepId);
            if (entity == null) return false;

            entity.StepNumber = model.StepNumber;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.HowItWorksSteps.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.HowItWorksSteps.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.HowItWorksSteps.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.HowItWorksSteps.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.HowItWorksSteps.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
