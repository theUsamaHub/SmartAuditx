using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using SmartAuditX.ViewModels.CMS;

namespace SmartAuditX.Services.Implementations.CMS
{
    public class ContactInformationService : IContactInformationService
    {
        private readonly ApplicationDbContext _context;

        public ContactInformationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ContactInformationVM>> GetAllAsync()
        {
            return await _context.ContactInformation
                .Select(c => new ContactInformationVM
                {
                    ContactInformationId = c.ContactInformationId,
                    CompanyName = c.CompanyName,
                    SupportEmail = c.SupportEmail,
                    SalesEmail = c.SalesEmail,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    GoogleMapEmbedUrl = c.GoogleMapEmbedUrl,
                    FacebookUrl = c.FacebookUrl,
                    LinkedInUrl = c.LinkedInUrl,
                    TwitterUrl = c.TwitterUrl,
                    InstagramUrl = c.InstagramUrl,
                    IsActive = c.IsActive
                })
                .OrderByDescending(c => c.ContactInformationId)
                .ToListAsync();
        }

        public async Task<ContactInformationVM?> GetByIdAsync(int id)
        {
            var c = await _context.ContactInformation.FindAsync(id);
            if (c == null) return null;

            return new ContactInformationVM
            {
                ContactInformationId = c.ContactInformationId,
                CompanyName = c.CompanyName,
                SupportEmail = c.SupportEmail,
                SalesEmail = c.SalesEmail,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                GoogleMapEmbedUrl = c.GoogleMapEmbedUrl,
                FacebookUrl = c.FacebookUrl,
                LinkedInUrl = c.LinkedInUrl,
                TwitterUrl = c.TwitterUrl,
                InstagramUrl = c.InstagramUrl,
                IsActive = c.IsActive
            };
        }

        public async Task<bool> CreateAsync(ContactInformationVM model)
        {
            if (model.IsActive)
            {
                await DeactivateOthersAsync();
            }

            var entity = new ContactInformation
            {
                CompanyName = model.CompanyName,
                SupportEmail = model.SupportEmail,
                SalesEmail = model.SalesEmail,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                GoogleMapEmbedUrl = model.GoogleMapEmbedUrl,
                FacebookUrl = model.FacebookUrl,
                LinkedInUrl = model.LinkedInUrl,
                TwitterUrl = model.TwitterUrl,
                InstagramUrl = model.InstagramUrl,
                IsActive = model.IsActive,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ContactInformation.Add(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(ContactInformationVM model)
        {
            var entity = await _context.ContactInformation.FindAsync(model.ContactInformationId);
            if (entity == null) return false;

            if (model.IsActive && !entity.IsActive)
            {
                await DeactivateOthersAsync(model.ContactInformationId);
            }

            entity.CompanyName = model.CompanyName;
            entity.SupportEmail = model.SupportEmail;
            entity.SalesEmail = model.SalesEmail;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Address = model.Address;
            entity.GoogleMapEmbedUrl = model.GoogleMapEmbedUrl;
            entity.FacebookUrl = model.FacebookUrl;
            entity.LinkedInUrl = model.LinkedInUrl;
            entity.TwitterUrl = model.TwitterUrl;
            entity.InstagramUrl = model.InstagramUrl;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.ContactInformation.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ContactInformation.FindAsync(id);
            if (entity == null) return false;

            _context.ContactInformation.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var entity = await _context.ContactInformation.FindAsync(id);
            if (entity == null) return false;

            if (!entity.IsActive)
            {
                await DeactivateOthersAsync(id);
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.ContactInformation.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task DeactivateOthersAsync(int? excludeId = null)
        {
            var activeRecords = await _context.ContactInformation
                .Where(c => c.IsActive && (excludeId == null || c.ContactInformationId != excludeId))
                .ToListAsync();

            foreach (var record in activeRecords)
            {
                record.IsActive = false;
                record.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
