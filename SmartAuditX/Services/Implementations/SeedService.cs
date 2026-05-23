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
        public async Task SeedSystemAdminAsync()
        {
            // STEP 1: Check if SYSTEM company exists (CompanyId = 1 is reserved for SYSTEM)
            var systemCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == 1);

            // STEP 2: Create SYSTEM company if it doesn't exist
            if (systemCompany == null)
            {
                systemCompany = new Company
                {
                    Name = "SYSTEM",           // Special company name for system-level operations
                    IsActive = true,           // Company is active
                    IsDeleted = false          // Company is not deleted
                };

                _context.Companies.Add(systemCompany);

                // Save to generate CompanyId (will be 1 due to auto-increment starting from 1)
                await _context.SaveChangesAsync();

                // STEP 3: Verify SystemAdmin role exists before creating admin user
                var roleExists = await _roleManager.RoleExistsAsync("SystemAdmin");

                if (!roleExists)
                {
                    throw new Exception(
                        "SystemAdmin role not found. Please run role seeding first.");
                }

                // STEP 4: Check if admin user already exists
                var adminUser =
                  await _userManager.FindByEmailAsync("admin@smartauditx.com");

                // STEP 5: Create admin user if it doesn't exist
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        CompanyId = systemCompany.CompanyId,     // Link to SYSTEM company

                        UserName = "admin",                       // Username for login

                        Email = "admin@smartauditx.com",          // Email address

                        PhoneNumber = "0000000000",               // Default phone number

                        EmailConfirmed = true,                    // Bypass email verification for system admin

                        PhoneNumberConfirmed = true,              // Bypass phone verification

                        IsActive = true,                          // Account is active

                        IsDeleted = false                         // Account is not deleted
                    };

                    // Create the user with default password
                    var result =
                        await _userManager.CreateAsync(
                            adminUser,
                            "Admin@123"                           // Default password (should be changed on first login)
                        );

                    // Check if user creation was successful
                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                            result.Errors.Select(x => x.Description)));
                    }

                    // STEP 6: Assign SystemAdmin role to the newly created admin user
                    if (!await _userManager.IsInRoleAsync(
            adminUser,
            "SystemAdmin"))
                    {
                        await _userManager.AddToRoleAsync(
                            adminUser,
                            "SystemAdmin");
                    }
                }
            }
            // Note: If SYSTEM company already exists, we assume admin user also exists
            // and skip the seeding process to avoid duplicates
        }
    }
}