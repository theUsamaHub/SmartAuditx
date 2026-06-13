using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class TeamMemberService : ITeamMemberService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TeamMemberService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<TeamMemberVM>> GetAllAsync()
        {
            return await _context.TeamMembers
                .Select(t => new TeamMemberVM
                {
                    TeamMemberId = t.TeamMemberId,
                    FullName = t.FullName,
                    Designation = t.Designation,
                    Bio = t.Bio,
                    ProfileImageUrl = t.ProfileImageUrl,
                    LinkedInUrl = t.LinkedInUrl,
                    DisplayOrder = t.DisplayOrder,
                    IsActive = t.IsActive
                })
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<TeamMemberVM?> GetByIdAsync(int id)
        {
            var t = await _context.TeamMembers.FindAsync(id);
            if (t == null) return null;

            return new TeamMemberVM
            {
                TeamMemberId = t.TeamMemberId,
                FullName = t.FullName,
                Designation = t.Designation,
                Bio = t.Bio,
                ProfileImageUrl = t.ProfileImageUrl,
                LinkedInUrl = t.LinkedInUrl,
                DisplayOrder = t.DisplayOrder,
                IsActive = t.IsActive
            };
        }

        public async Task<bool> CreateAsync(TeamMemberVM model)
        {
            string? imageUrl = null;
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                imageUrl = await SaveFileAsync(model.ProfileImageFile);
            }

            var entity = new TeamMember
            {
                FullName = model.FullName,
                Designation = model.Designation,
                Bio = model.Bio,
                ProfileImageUrl = imageUrl,
                LinkedInUrl = model.LinkedInUrl,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(TeamMemberVM model)
        {
            var entity = await _context.TeamMembers.FindAsync(model.TeamMemberId);
            if (entity == null) return false;

            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                entity.ProfileImageUrl = await SaveFileAsync(model.ProfileImageFile);
            }

            entity.FullName = model.FullName;
            entity.Designation = model.Designation;
            entity.Bio = model.Bio;
            entity.LinkedInUrl = model.LinkedInUrl;
            entity.DisplayOrder = model.DisplayOrder;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.TeamMembers.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.TeamMembers.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.TeamMembers.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.TeamMembers.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.TeamMembers.Update(entity);
            return await _context.SaveChangesAsync() > 0;
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
