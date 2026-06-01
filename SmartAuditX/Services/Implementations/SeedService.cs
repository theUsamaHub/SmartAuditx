using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{   
    /// <summary>
    /// Service responsible for seeding initial data into the application database
    /// including the system admin user and required company records
    /// </summary>
    public class SeedService : ISeedService 
    {   
        // Database context for direct database operations
        private readonly ApplicationDbContext _context;

        // Manages user account operations (create, update, find, etc.)
        private readonly UserManager<ApplicationUser> _userManager;

        // Manages role operations (create, delete, check role existence, etc.)
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the SeedService with required dependencies
        /// </summary>
        /// <param name="context">Database context for data access</param>
        /// <param name="userManager">Identity user manager for user operations</param>
        /// <param name="roleManager">Identity role manager for role operations</param>
        public SeedService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Seeds the System Admin user into the database
        /// Creates a SYSTEM company if it doesn't exist
        /// Creates admin@smartauditx.com user with SystemAdmin role
        /// </summary>
        /// <exception cref="Exception">Thrown when SystemAdmin role doesn't exist or user creation fails</exception>
        /// 
        public async Task SeedSystemAdminAsync()
        {
            // STEP 1: Check if we need to migrate old data first

            // STEP 1:
            // Check whether the SYSTEM company already exists.
            // CompanyId = 1 is reserved for platform-level operations.
            var systemCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == 1);

            // STEP 2:
            // Create SYSTEM company if it does not exist.
            if (systemCompany == null)
            {
                systemCompany = new Company
                {
                    // ─────────────────────────────────────────────
                    // CORE COMPANY INFORMATION
                    // ─────────────────────────────────────────────

                    Name = "SYSTEM",

                    IndustryType = IndustryType.Administrative, //Using Administrative as a generic industry type for the system company

                    Website = "https://smartauditx.com",

                    LogoUrl = null,

                    // ─────────────────────────────────────────────
                    // COMPANY SEGMENTATION
                    // ─────────────────────────────────────────────

                    CompanySize = CompanySize.Enterprise,

                    EmployeeCountRange = EmployeeCountRange.Large, //now uses the enum value instead of hardcoded number

                    // ─────────────────────────────────────────────
                    // LOCATION
                    // ─────────────────────────────────────────────

                    CountryCode = "PK",

                    City = "Karachi",

                    // ─────────────────────────────────────────────
                    // MARKETING / ONBOARDING
                    // ─────────────────────────────────────────────

                    ReferralSource = ReferralSources.Youtube,

                    OnboardingStatus = OnboardingStatus.Active,

                    // ─────────────────────────────────────────────
                    // SYSTEM FLAGS
                    // ─────────────────────────────────────────────

                    IsActive = true,

                    IsDeleted = false
                };

                _context.Companies.Add(systemCompany);

                // Save immediately to generate CompanyId
                await _context.SaveChangesAsync();
            }

            // STEP 3:
            // Ensure SystemAdmin role exists before creating user.
            var roleExists =
                await _roleManager.RoleExistsAsync("SystemAdmin");

            if (!roleExists)
            {
                throw new Exception(
                    "SystemAdmin role not found. Run role seeding first.");
            }

            // STEP 4:
            // Check whether admin user already exists.
            var adminUser =
                await _userManager.FindByEmailAsync(
                    "admin@smartauditx.com");

            // STEP 5:
            // Create system admin user if not found.
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    // ─────────────────────────────────────────────
                    // TENANT RELATIONSHIP
                    // ─────────────────────────────────────────────

                    CompanyId = systemCompany.CompanyId,

                    EmployeeId = null,

                    // ─────────────────────────────────────────────
                    // LOGIN INFORMATION
                    // ─────────────────────────────────────────────

                    UserName = "admin",

                    Email = "admin@smartauditx.com",

                    // ─────────────────────────────────────────────
                    // PHONE INFORMATION
                    // ─────────────────────────────────────────────

                    PhoneDialCode = "+92",

                    PhoneNumber = "3000000000",

                    // ─────────────────────────────────────────────
                    // CONFIRMATIONS
                    // ─────────────────────────────────────────────

                    EmailConfirmed = true,

                    PhoneNumberConfirmed = true,

                    // ─────────────────────────────────────────────
                    // SECURITY
                    // ─────────────────────────────────────────────

                    LockoutEnabled = true,

                    TwoFactorEnabled = false,

                    // ─────────────────────────────────────────────
                    // SYSTEM FLAGS
                    // ─────────────────────────────────────────────

                    IsActive = true,

                    IsDeleted = false
                };

                // Create admin user
                var result =
                    await _userManager.CreateAsync(
                        adminUser,
                        "Admin@123");

                // Validate user creation
                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ",
                        result.Errors.Select(x => x.Description)));
                }

                // STEP 6:
                // Assign SystemAdmin role.
                await _userManager.AddToRoleAsync(
                    adminUser,
                    "SystemAdmin");
            }
        }
    }
}