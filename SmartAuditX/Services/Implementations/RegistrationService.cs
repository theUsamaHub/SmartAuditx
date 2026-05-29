//using Microsoft.AspNetCore.Identity;
//using SmartAuditX.Data;
//using SmartAuditX.Models;
//using SmartAuditX.Models.ViewModels;
//using SmartAuditX.Services.Interfaces;

//namespace SmartAuditX.Services.Implementations
//{
//    public class RegistrationService : IRegistrationService
//    {
//        private readonly IFileService _fileService;
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;


//        public RegistrationService(
//            IFileService fileService,
//            ApplicationDbContext context,
//            UserManager<ApplicationUser> userManager)
//        {
//            _fileService = fileService;
//            _context = context;
//            _userManager = userManager;
//        }

//        public async Task<bool> RegisterCompanyOwnerAsync(
//            RegisterAccountViewModel account,
//            RegisterCompanyViewModel company)
//        {
//            using var transaction =
//                await _context.Database.BeginTransactionAsync();

//            try
//            {
//                // work here
//                var newCompany = new Company
//                {
//                    Name = company.CompanyName,
//                    IndustryType = company.IndustryType,
//                    LogoUrl = company.LogoUrl,
//                    IsActive = true,
//                    IsDeleted = false
//                };


//                await _context.Companies.AddAsync(newCompany);


//                await _context.SaveChangesAsync();

//                //get the company id after saving the company to the database
//                var getComId = newCompany.CompanyId;

//                var user = new ApplicationUser
//                {
//                    CompanyId = getComId,

//                    UserName = account.Username,

//                    Email = account.Email,

//                    PhoneNumber = account.PhoneNumber,

//                    IsActive = true,

//                    IsDeleted = false,

//                    EmailConfirmed = true,

//                    PhoneNumberConfirmed = true
//                };

//                var result =
//    await _userManager.CreateAsync(
//        user,
//        account.Password);

//                if (!result.Succeeded)
//                {
//                    throw new Exception(
//                        string.Join(", ",
//                        result.Errors.Select(x => x.Description)));
//                }

//                var roleResult = await _userManager.AddToRoleAsync(
//     user,
//     "CompanyOwner");

//                if (!roleResult.Succeeded)
//                {
//                    throw new Exception(
//                        string.Join(", ",
//                        roleResult.Errors.Select(x => x.Description)));
//                }

//                await _context.SaveChangesAsync();

//                await transaction.CommitAsync();

//                return true;
//            }
//            catch
//            {
//                await transaction.RollbackAsync();

//                return false;
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Helpers;
using SmartAuditX.Models;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    /// <summary>
    /// Handles complete company onboarding registration.
    /// 
    /// FLOW:
    /// 1. Validate incoming data
    /// 2. Validate uniqueness
    /// 3. Upload company logo
    /// 4. Create company
    /// 5. Create company owner user
    /// 6. Assign CompanyOwner role
    /// 7. Commit transaction
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        // ─────────────────────────────────────────────
        // DEPENDENCIES
        // ─────────────────────────────────────────────

        private readonly IFileService _fileService;

        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        // ─────────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────────

        public RegistrationService(
            IFileService fileService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _fileService = fileService;

            _context = context;

            _userManager = userManager;
        }

        // ─────────────────────────────────────────────
        // MAIN REGISTRATION METHOD
        // ─────────────────────────────────────────────

        public async Task<bool> RegisterCompanyOwnerAsync(
            RegisterAccountViewModel account,
            RegisterCompanyViewModel company)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ─────────────────────────────────────────────
                // STEP 1: VALIDATE COUNTRY CODE
                // ─────────────────────────────────────────────

                account.PhoneNumber =
    account.PhoneNumber.Replace(" ", "")
                       .Replace("-", "")
                       .Trim();

                if (!CountryValidator.IsValidCountryCode(
                    company.CountryCode))
                {
                    throw new Exception(
                        "Invalid country code.");
                }

                // ─────────────────────────────────────────────
                // STEP 2: CHECK DUPLICATE EMAIL
                // ─────────────────────────────────────────────

                var emailExists =
                    await _userManager.FindByEmailAsync(
                        account.Email);

                if (emailExists != null)
                {
                    throw new Exception(
                        "Email already exists.");
                }

                // ─────────────────────────────────────────────
                // STEP 3: CHECK DUPLICATE USERNAME
                // ─────────────────────────────────────────────

                var usernameExists =
                    await _userManager.FindByNameAsync(
                        account.Username);

                if (usernameExists != null)
                {
                    throw new Exception(
                        "Username already exists.");
                }

                // ─────────────────────────────────────────────
                // STEP 4: CHECK DUPLICATE PHONE NUMBER
                // ─────────────────────────────────────────────

                var phoneExists =
                    await _userManager.Users
                        .AnyAsync(x =>
                            x.PhoneNumber == account.PhoneNumber &&
                            x.PhoneDialCode == account.PhoneDialCode);

                if (phoneExists)
                {
                    throw new Exception("Phone number already exists.");
                }
                // ─────────────────────────────────────────────
                // STEP 5: UPLOAD COMPANY LOGO
                // ─────────────────────────────────────────────

                var logoPath =
                    await _fileService
                        .UploadCompanyLogoAsync(
                            company.CompanyLogo);

                // ─────────────────────────────────────────────
                // STEP 6: CREATE COMPANY
                // ─────────────────────────────────────────────

                var newCompany = new Company
                {
                    Name = company.CompanyName,

                    IndustryType = company.IndustryType,

                    Website = company.Website,

                    CompanySize = company.CompanySize,

                    EmployeeCountRange = company.EmployeeCountRange,  // Now uses enum

                    CountryCode =
                        company.CountryCode.ToUpper(),

                    City = company.City,

                    ReferralSource =
                        company.ReferralSource,

                    LogoUrl = logoPath,

                    OnboardingStatus =
                        OnboardingStatus.CompanyInfoSaved,

                    IsActive = true,

                    IsDeleted = false
                };

                await _context.Companies.AddAsync(
                    newCompany);

                // Save immediately to generate CompanyId
                await _context.SaveChangesAsync();

                // ─────────────────────────────────────────────
                // STEP 7: CREATE COMPANY OWNER USER
                // ─────────────────────────────────────────────

                var user = new ApplicationUser
                {
                    // Tenant isolation
                    CompanyId = newCompany.CompanyId,

                    // Identity
                    UserName = account.Username,

                    Email = account.Email,

                    PhoneNumber = account.PhoneNumber,

                    PhoneDialCode = account.PhoneDialCode,

                    // Verification
                    EmailConfirmed = true,

                    PhoneNumberConfirmed = true,

                    // Account state
                    IsActive = true,

                    IsDeleted = false,

                    // Security
                    LockoutEnabled = true
                };

                // ─────────────────────────────────────────────
                // STEP 8: CREATE USER IN IDENTITY
                // ─────────────────────────────────────────────

                var createUserResult =
                    await _userManager.CreateAsync(
                        user,
                        account.Password);

                if (!createUserResult.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            ", ",
                            createUserResult.Errors
                                .Select(x => x.Description)));
                }

                // ─────────────────────────────────────────────
                // STEP 9: ASSIGN COMPANY OWNER ROLE
                // ─────────────────────────────────────────────

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "CompanyOwner");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            ", ",
                            roleResult.Errors
                                .Select(x => x.Description)));
                }

                // ─────────────────────────────────────────────
                // STEP 10: SAVE ALL CHANGES
                // ─────────────────────────────────────────────

                await _context.SaveChangesAsync();

                // ─────────────────────────────────────────────
                // STEP 11: COMMIT TRANSACTION
                // ─────────────────────────────────────────────

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                // ─────────────────────────────────────────────
                // ROLLBACK EVERYTHING
                // ─────────────────────────────────────────────

                await transaction.RollbackAsync();

                return false;
            }
        }
    }
}