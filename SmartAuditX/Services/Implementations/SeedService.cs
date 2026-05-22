using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class SeedService : ISeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SeedService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {  
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedSystemAdminAsync()
        {
            var systemCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId==1);

            if (systemCompany == null)
            {
                systemCompany = new Company
                {
                    Name = "SYSTEM",
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Companies.Add(systemCompany);

                await _context.SaveChangesAsync();

                var roleExists = await _roleManager.RoleExistsAsync("SystemAdmin");

                if (!roleExists)
                {
                    throw new Exception(
                        "SystemAdmin role not found.");
                }
                var adminUser =
                  await _userManager.FindByEmailAsync("admin@smartauditx.com");

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        CompanyId = systemCompany.CompanyId,

                        UserName = "admin",

                        Email = "admin@smartauditx.com",

                        PhoneNumber = "0000000000",

                        EmailConfirmed = true,

                        PhoneNumberConfirmed = true,

                        IsActive = true,

                        IsDeleted = false
                    };

                    var result =
                        await _userManager.CreateAsync(
                            adminUser,
                            "Admin@123"
                        );

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                            result.Errors.Select(x => x.Description)));
                    }

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


        }
    }
}