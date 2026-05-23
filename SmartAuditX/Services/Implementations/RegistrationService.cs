using Microsoft.AspNetCore.Identity;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public RegistrationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> RegisterCompanyOwnerAsync(
            RegisterAccountViewModel account,
            RegisterCompanyViewModel company)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // work here
                var newCompany = new Company
                {
                    Name = company.CompanyName,
                    IndustryType = company.IndustryType,
                    LogoUrl = company.LogoUrl,
                    IsActive = true,
                    IsDeleted = false
                };


                await _context.Companies.AddAsync(newCompany);


                await _context.SaveChangesAsync();

                //get the company id after saving the company to the database
                var getComId = newCompany.CompanyId;

                var user = new ApplicationUser
                {
                    CompanyId = getComId,

                    UserName = account.Username,

                    Email = account.Email,

                    PhoneNumber = account.PhoneNumber,

                    IsActive = true,

                    IsDeleted = false,

                    EmailConfirmed = true,

                    PhoneNumberConfirmed = true
                };

                var result =
    await _userManager.CreateAsync(
        user,
        account.Password);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ",
                        result.Errors.Select(x => x.Description)));
                }

                var roleResult = await _userManager.AddToRoleAsync(
     user,
     "CompanyOwner");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ",
                        roleResult.Errors.Select(x => x.Description)));
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();

                return false;
            }
        }
    }
}