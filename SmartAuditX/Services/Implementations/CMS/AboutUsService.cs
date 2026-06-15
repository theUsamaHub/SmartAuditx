using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.Models.CMS.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class AboutUsService : IAboutUsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AboutUsService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<AboutUsVM>> GetAllAsync()
        {
            return await _context.AboutUs
                .Select(a => new AboutUsVM
                {
                    AboutUsId = a.AboutUsId,
                    Title = a.Title,
                    ShortDescription = a.ShortDescription,
                    FullDescription = a.FullDescription,
                    Mission = a.Mission,
                    Vision = a.Vision,
                    ImageUrl = a.ImageUrl,
                    IsActive = a.IsActive
                })
                .OrderByDescending(a => a.AboutUsId)
                .ToListAsync();
        }

        public async Task<AboutUsVM?> GetByIdAsync(int id)
        {
            var a = await _context.AboutUs.FindAsync(id);
            if (a == null) return null;

            return new AboutUsVM
            {
                AboutUsId = a.AboutUsId,
                Title = a.Title,
                ShortDescription = a.ShortDescription,
                FullDescription = a.FullDescription,
                Mission = a.Mission,
                Vision = a.Vision,
                ImageUrl = a.ImageUrl,
                IsActive = a.IsActive
            };
        }

        public async Task<bool> CreateAsync(AboutUsVM model)
        {
            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                imageUrl = await SaveFileAsync(model.ImageFile);
            }

            if (model.IsActive)
            {
                await DeactivateOthersAsync();
            }

            var entity = new AboutUs
            {
                Title = model.Title,
                ShortDescription = model.ShortDescription,
                FullDescription = model.FullDescription,
                Mission = model.Mission,
                Vision = model.Vision,
                ImageUrl = imageUrl,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.AboutUs.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(AboutUsVM model)
        {
            var entity = await _context.AboutUs.FindAsync(model.AboutUsId);
            if (entity == null) return false;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                entity.ImageUrl = await SaveFileAsync(model.ImageFile);
            }

            if (model.IsActive && !entity.IsActive)
            {
                await DeactivateOthersAsync(model.AboutUsId);
            }

            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.FullDescription = model.FullDescription;
            entity.Mission = model.Mission;
            entity.Vision = model.Vision;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.AboutUs.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.AboutUs.FindAsync(id);
            if (entity == null) return false;

            _context.AboutUs.Remove(entity); // No IsDeleted field requested, hard delete or just ignore? The prompt mentioned soft delete for features. But AboutUs doesn't have IsDeleted in prompt. Will hard delete.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.AboutUs.FindAsync(id);
            if (entity == null) return false;

            if (!entity.IsActive)
            {
                await DeactivateOthersAsync(id);
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.AboutUs.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task DeactivateOthersAsync(int? excludeId = null)
        {
            var activeRecords = await _context.AboutUs
                .Where(a => a.IsActive && (excludeId == null || a.AboutUsId != excludeId))
                .ToListAsync();

            foreach (var record in activeRecords)
            {
                record.IsActive = false;
                record.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task<string> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "CMSUpload");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/CMSUpload/" + uniqueFileName;
        }
    }
}
