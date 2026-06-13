using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class FaqService : IFaqService
    {
        private readonly ApplicationDbContext _context;

        public FaqService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FaqVM>> GetAllAsync()
        {
            return await _context.Faqs
                .Select(f => new FaqVM
                {
                    FaqId = f.FaqId,
                    Question = f.Question,
                    Answer = f.Answer,
                    DisplayOrder = f.DisplayOrder,
                    IsActive = f.IsActive
                })
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<FaqVM?> GetByIdAsync(int id)
        {
            var f = await _context.Faqs.FindAsync(id);
            if (f == null) return null;

            return new FaqVM
            {
                FaqId = f.FaqId,
                Question = f.Question,
                Answer = f.Answer,
                DisplayOrder = f.DisplayOrder,
                IsActive = f.IsActive
            };
        }

        public async Task<bool> CreateAsync(FaqVM model)
        {
            var entity = new Faq
            {
                Question = model.Question,
                Answer = model.Answer,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Faqs.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(FaqVM model)
        {
            var entity = await _context.Faqs.FindAsync(model.FaqId);
            if (entity == null) return false;

            entity.Question = model.Question;
            entity.Answer = model.Answer;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Faqs.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Faqs.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Faqs.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.Faqs.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Faqs.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
