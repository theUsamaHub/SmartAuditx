using Humanizer;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class CompanyContactService : ICompanyContactService
    {
        private readonly ApplicationDbContext _context;

        public CompanyContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CompanyContactListItemViewModel>> GetAllAsync(
            int companyId,
            ContactType? contactType = null,
            string? search = null)
        {
            var query = _context.CompanyContacts
                .Where(contact => contact.CompanyId == companyId);

            if (contactType.HasValue)
            {
                query = query.Where(contact => contact.ContactType == contactType.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(contact =>
                    (contact.ContactName != null && contact.ContactName.Contains(term)) ||
                    contact.Email.Contains(term) ||
                    contact.PhoneNumber.Contains(term) ||
                    contact.PhoneDialCode.Contains(term));
            }

            var contacts = await query
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenBy(contact => contact.ContactType)
                .ThenBy(contact => contact.ContactName)
                .ToListAsync();

            return contacts.Select(MapToListItem).ToList();
        }

        public async Task<CompanyContactViewModel?> GetForEditAsync(int companyId, int contactId)
        {
            var contact = await _context.CompanyContacts
                .FirstOrDefaultAsync(item =>
                    item.CompanyContactId == contactId &&
                    item.CompanyId == companyId);

            if (contact == null)
            {
                return null;
            }

            return new CompanyContactViewModel
            {
                CompanyContactId = contact.CompanyContactId,
                ContactType = contact.ContactType,
                ContactName = contact.ContactName,
                Email = contact.Email,
                PhoneDialCode = contact.PhoneDialCode,
                PhoneNumber = contact.PhoneNumber,
                FaxNumber = contact.FaxNumber,
                PhysicalAddress = contact.PhysicalAddress,
                IsPrimary = contact.IsPrimary
            };
        }

        public async Task<CompanyContactOperationResult> CreateAsync(
            int companyId,
            CompanyContactViewModel model)
        {
            var contact = new CompanyContact
            {
                CompanyId = companyId,
                ContactType = model.ContactType,
                ContactName = NormalizeOptional(model.ContactName),
                Email = model.Email.Trim(),
                PhoneDialCode = model.PhoneDialCode.Trim(),
                PhoneNumber = NormalizePhoneNumber(model.PhoneNumber),
                FaxNumber = NormalizeOptional(model.FaxNumber),
                PhysicalAddress = NormalizeOptional(model.PhysicalAddress),
                IsPrimary = model.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            if (model.IsPrimary)
            {
                await ClearPrimaryAsync(companyId);
            }

            await _context.CompanyContacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            return CompanyContactOperationResult.Ok(
                "Contact created successfully.",
                MapToListItem(contact));
        }

        public async Task<CompanyContactOperationResult> UpdateAsync(
            int companyId,
            int contactId,
            CompanyContactViewModel model)
        {
            var contact = await _context.CompanyContacts
                .FirstOrDefaultAsync(item =>
                    item.CompanyContactId == contactId &&
                    item.CompanyId == companyId);

            if (contact == null)
            {
                return CompanyContactOperationResult.Fail("Contact not found.");
            }

            contact.ContactType = model.ContactType;
            contact.ContactName = NormalizeOptional(model.ContactName);
            contact.Email = model.Email.Trim();
            contact.PhoneDialCode = model.PhoneDialCode.Trim();
            contact.PhoneNumber = NormalizePhoneNumber(model.PhoneNumber);
            contact.FaxNumber = NormalizeOptional(model.FaxNumber);
            contact.PhysicalAddress = NormalizeOptional(model.PhysicalAddress);
            contact.UpdatedAt = DateTime.UtcNow;

            if (model.IsPrimary && !contact.IsPrimary)
            {
                await ClearPrimaryAsync(companyId);
                contact.IsPrimary = true;
            }
            else if (!model.IsPrimary && contact.IsPrimary)
            {
                contact.IsPrimary = false;
            }

            await _context.SaveChangesAsync();

            return CompanyContactOperationResult.Ok(
                "Contact updated successfully.",
                MapToListItem(contact));
        }

        public async Task<CompanyContactOperationResult> DeleteAsync(int companyId, int contactId)
        {
            var contact = await _context.CompanyContacts
                .FirstOrDefaultAsync(item =>
                    item.CompanyContactId == contactId &&
                    item.CompanyId == companyId);

            if (contact == null)
            {
                return CompanyContactOperationResult.Fail("Contact not found.");
            }

            contact.IsDeleted = true;
            contact.IsPrimary = false;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return CompanyContactOperationResult.Ok("Contact deleted successfully.");
        }

        public async Task<CompanyContactOperationResult> SetPrimaryAsync(int companyId, int contactId)
        {
            var contact = await _context.CompanyContacts
                .FirstOrDefaultAsync(item =>
                    item.CompanyContactId == contactId &&
                    item.CompanyId == companyId);

            if (contact == null)
            {
                return CompanyContactOperationResult.Fail("Contact not found.");
            }

            await ClearPrimaryAsync(companyId);

            contact.IsPrimary = true;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return CompanyContactOperationResult.Ok(
                "Primary contact updated successfully.",
                MapToListItem(contact));
        }

        private async Task ClearPrimaryAsync(int companyId)
        {
            var primaryContacts = await _context.CompanyContacts
                .Where(contact => contact.CompanyId == companyId && contact.IsPrimary)
                .ToListAsync();

            foreach (var primaryContact in primaryContacts)
            {
                primaryContact.IsPrimary = false;
                primaryContact.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static string NormalizePhoneNumber(string phoneNumber) =>
            phoneNumber
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Trim();

        private static string? NormalizeOptional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static CompanyContactListItemViewModel MapToListItem(CompanyContact contact) =>
            new()
            {
                CompanyContactId = contact.CompanyContactId,
                ContactType = contact.ContactType,
                ContactTypeDisplay = contact.ContactType.ToString().Humanize(),
                ContactName = contact.ContactName,
                Email = contact.Email,
                PhoneDialCode = contact.PhoneDialCode,
                PhoneNumber = contact.PhoneNumber,
                FaxNumber = contact.FaxNumber,
                PhysicalAddress = contact.PhysicalAddress,
                IsPrimary = contact.IsPrimary,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };
    }
}
