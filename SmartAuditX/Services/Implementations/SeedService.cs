using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.CMS;
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
            // System company + admin user + roles (existing logic)
            await SeedSystemCompanyAndAdminAsync();

            // CMS data for public website
            await SeedCmsDataAsync();
        }

        private async Task SeedSystemCompanyAndAdminAsync()
        {
            var systemCompany = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == 1);

            if (systemCompany == null)
            {
                systemCompany = new Company
                {
                    Name = "SYSTEM",
                    IndustryType = IndustryType.Administrative,
                    Website = "https://smartauditx.com",
                    LogoUrl = null,
                    CompanySize = CompanySize.Enterprise,
                    EmployeeCountRange = EmployeeCountRange.Large,
                    CountryCode = "PK",
                    City = "Karachi",
                    ReferralSource = ReferralSources.Youtube,
                    OnboardingStatus = OnboardingStatus.Active,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Companies.Add(systemCompany);
                await _context.SaveChangesAsync();
            }

            string[] roles = { "SystemAdmin", "CompanyOwner", "Manager", "Auditor", "Employee", "User" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                        Description = $"{role} role",
                        IsActive = true
                    });
                }
            }

            var adminUser = await _userManager.FindByEmailAsync("admin@smartauditx.com");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    CompanyId = systemCompany.CompanyId,
                    EmployeeId = null,
                    UserName = "admin",
                    Email = "admin@smartauditx.com",
                    PhoneDialCode = "+92",
                    PhoneNumber = "3000000000",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = true,
                    TwoFactorEnabled = false,
                    IsActive = true,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
                }

                await _userManager.AddToRoleAsync(adminUser, "SystemAdmin");
            }
        }

        private async Task SeedCmsDataAsync()
        {
            // Seed HeroSection
            if (!await _context.HeroSections.AnyAsync())
            {
                _context.HeroSections.Add(new HeroSection
                {
                    BadgeText = "SmartAuditX Platform",
                    Title = "Smart Audits for Modern Businesses",
                    Description = "Streamline your audit processes with AI-powered templates, real-time tracking, and comprehensive reporting. Built for companies of all sizes across every industry.",
                    PrimaryButtonText = "Get Started",
                    PrimaryButtonUrl = "/Registration/AccountInfo",
                    SecondaryButtonText = "View Demo",
                    SecondaryButtonUrl = "/Public",
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }

            // Seed Features
            if (!await _context.Features.AnyAsync())
            {
                var features = new List<Feature>
                {
                    new Feature { Title = "Custom Audit Templates", ShortDescription = "Create fully customizable audit templates with multiple field types including Yes/No, text, number, rating, photo, barcode scanning, and signature capture.", IconName = "bi-clipboard-check", DisplayOrder = 1, IsActive = true },
                    new Feature { Title = "Real-Time Audit Tracking", ShortDescription = "Track audit progress in real-time with status updates, notifications, and live dashboards for managers and auditors.", IconName = "bi-graph-up-arrow", DisplayOrder = 2, IsActive = true },
                    new Feature { Title = "Inventory Barcode Scanning", ShortDescription = "Scan barcodes during physical audits, compare expected vs actual quantities, and generate discrepancy reports automatically.", IconName = "bi-upc-scan", DisplayOrder = 3, IsActive = true },
                    new Feature { Title = "Multi-Branch Support", ShortDescription = "Manage audits across multiple branches, departments, and locations with centralized oversight and branch-specific tracking.", IconName = "bi-building", DisplayOrder = 4, IsActive = true },
                    new Feature { Title = "Employee Document Management", ShortDescription = "Upload, verify, and manage employee documents with customizable document types and verification workflows.", IconName = "bi-file-earmark-text", DisplayOrder = 5, IsActive = true },
                    new Feature { Title = "Comprehensive Reporting", ShortDescription = "Generate detailed PDF and Excel reports with audit scores, findings, inventory comparisons, and historical data.", IconName = "bi-bar-chart-line", DisplayOrder = 6, IsActive = true }
                };
                _context.Features.AddRange(features);
                await _context.SaveChangesAsync();
            }

            // Seed FAQs
            if (!await _context.Faqs.AnyAsync())
            {
                var faqs = new List<Faq>
                {
                    new Faq { Question = "What is SmartAuditX?", Answer = "SmartAuditX is a comprehensive audit management platform designed for businesses of all sizes. It provides custom audit templates, real-time tracking, inventory management, and detailed reporting to streamline your compliance and audit processes.", DisplayOrder = 1, IsActive = true },
                    new Faq { Question = "How many industries does SmartAuditX support?", Answer = "SmartAuditX is designed to serve 80% of businesses across all industries including retail, hospitality, healthcare, manufacturing, education, logistics, corporate, and any other industry with fully company-defined templates.", DisplayOrder = 2, IsActive = true },
                    new Faq { Question = "Can I create custom audit templates?", Answer = "Yes! SmartAuditX allows you to create fully customizable audit templates with multiple field types including Yes/No, text, number, rating, dropdown, checkbox, date, photo, barcode scanning, and signature capture.", DisplayOrder = 3, IsActive = true },
                    new Faq { Question = "How does the inventory barcode scanning work?", Answer = "Upload your inventory list via Excel, then auditors scan barcodes during physical audits. The system automatically compares expected vs actual quantities and generates discrepancy reports showing matched, surplus, shortage, and missing items.", DisplayOrder = 4, IsActive = true },
                    new Faq { Question = "Is SmartAuditX suitable for small businesses?", Answer = "Absolutely! SmartAuditX is designed for companies of all sizes, from small retail shops doing daily inventory checks to large enterprises running ISO compliance audits across multiple branches.", DisplayOrder = 5, IsActive = true },
                    new Faq { Question = "How does the audit assignment workflow work?", Answer = "Company administrators create audit templates, schedule audits, and assign them to auditors. Auditors receive notifications, conduct the audit, and submit responses. Managers review completed audits and approve or send them back for correction.", DisplayOrder = 6, IsActive = true }
                };
                _context.Faqs.AddRange(faqs);
                await _context.SaveChangesAsync();
            }

            // Seed HowItWorks
            if (!await _context.HowItWorksSteps.AnyAsync())
            {
                var steps = new List<HowItWorksStep>
                {
                    new HowItWorksStep { StepNumber = 1, Title = "Create Your Audit Template", Description = "Design custom audit templates with the field types you need. Add sections, questions, scoring weights, and inventory lists. Templates are fully reusable across audits.", DisplayOrder = 1, IsActive = true },
                    new HowItWorksStep { StepNumber = 2, Title = "Assign & Conduct Audits", Description = "Schedule audits, assign them to auditors, and track progress in real-time. Auditors fill in responses, scan barcodes, capture photos, and submit their findings.", DisplayOrder = 2, IsActive = true },
                    new HowItWorksStep { StepNumber = 3, Title = "Review & Generate Reports", Description = "Managers review completed audits, approve or send back for corrections. Generate comprehensive PDF and Excel reports with scores, findings, and inventory comparisons.", DisplayOrder = 3, IsActive = true }
                };
                _context.HowItWorksSteps.AddRange(steps);
                await _context.SaveChangesAsync();
            }

            // Seed PlatformModules
            if (!await _context.PlatformModules.AnyAsync())
            {
                var modules = new List<PlatformModule>
                {
                    new PlatformModule { Title = "Audit Template Builder", Description = "Create and manage reusable audit templates with multiple field types, scoring, and inventory lists.", IconName = "bi-clipboard-check", DisplayOrder = 1, IsActive = true },
                    new PlatformModule { Title = "Audit Management", Description = "Schedule, assign, track, and manage audits across your organization with real-time status updates.", IconName = "bi-calendar-check", DisplayOrder = 2, IsActive = true },
                    new PlatformModule { Title = "Employee Management", Description = "Manage employees across branches and departments with document tracking and system user creation.", IconName = "bi-people", DisplayOrder = 3, IsActive = true },
                    new PlatformModule { Title = "Inventory & Barcode", Description = "Upload inventory lists, scan barcodes during audits, and compare expected vs actual quantities.", IconName = "bi-upc-scan", DisplayOrder = 4, IsActive = true },
                    new PlatformModule { Title = "Reporting & Analytics", Description = "Generate detailed reports with audit scores, findings, inventory comparisons, and historical data.", IconName = "bi-bar-chart-line", DisplayOrder = 5, IsActive = true },
                    new PlatformModule { Title = "Organization Setup", Description = "Set up branches, departments, designations, and company contacts for structured organizational management.", IconName = "bi-diagram-3", DisplayOrder = 6, IsActive = true }
                };
                _context.PlatformModules.AddRange(modules);
                await _context.SaveChangesAsync();
            }

            // Seed SecurityFeatures
            if (!await _context.SecurityFeatures.AnyAsync())
            {
                var securityFeatures = new List<SecurityFeature>
                {
                    new SecurityFeature { Title = "Role-Based Access Control", Description = "Multi-role authorization system with SystemAdmin, CompanyOwner, Manager, Auditor, and Employee roles for granular access control.", IconName = "bi-shield-lock", DisplayOrder = 1, IsActive = true },
                    new SecurityFeature { Title = "Secure Session Management", Description = "HTTP-only cookies with 60-minute idle timeout, 7-day max age, and secure cookie policies for maximum session security.", IconName = "bi-clock-history", DisplayOrder = 2, IsActive = true },
                    new SecurityFeature { Title = "Password Policies", Description = "Enforced password complexity with minimum 8 characters, uppercase, lowercase, digits, and account lockout after 5 failed attempts.", IconName = "bi-key", DisplayOrder = 3, IsActive = true },
                    new SecurityFeature { Title = "Multi-Tenant Isolation", Description = "Complete data isolation between companies with company-scoped operations ensuring no cross-tenant data leakage.", IconName = "bi-building", DisplayOrder = 4, IsActive = true },
                    new SecurityFeature { Title = "Email Verification", Description = "Mandatory email verification for all new accounts with secure token-based confirmation links.", IconName = "bi-envelope-check", DisplayOrder = 5, IsActive = true },
                    new SecurityFeature { Title = "Audit Trail Logging", Description = "Complete audit trail for all critical operations with timestamps, user tracking, and activity logging.", IconName = "bi-journal-text", DisplayOrder = 6, IsActive = true }
                };
                _context.SecurityFeatures.AddRange(securityFeatures);
                await _context.SaveChangesAsync();
            }

            // Seed AboutUs
            if (!await _context.AboutUs.AnyAsync())
            {
                _context.AboutUs.Add(new AboutUs
                {
                    Title = "About SmartAuditX",
                    ShortDescription = "SmartAuditX is a comprehensive audit management platform designed to serve 80% of businesses across all industries.",
                    FullDescription = "SmartAuditX was built to solve the complex challenge of audit management for businesses of all sizes. From small retail shops doing daily inventory checks to large enterprises running ISO compliance audits across 50 branches, our platform provides the tools you need. We believe that every business deserves access to professional-grade audit tools without the complexity and cost of enterprise solutions.",
                    Mission = "To simplify audit management for businesses worldwide with intuitive tools, real-time tracking, and comprehensive reporting.",
                    Vision = "To become the global standard for audit management, serving every industry from retail to healthcare, manufacturing to education.",
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }

            // Seed TeamMembers
            if (!await _context.TeamMembers.AnyAsync())
            {
                var team = new List<TeamMember>
                {
                    new TeamMember { FullName = "SmartAuditX Team", Designation = "Development Team", Bio = "The dedicated team behind SmartAuditX, committed to building the best audit management platform.", DisplayOrder = 1, IsActive = true }
                };
                _context.TeamMembers.AddRange(team);
                await _context.SaveChangesAsync();
            }

            // Seed ContactInformation
            if (!await _context.ContactInformation.AnyAsync())
            {
                _context.ContactInformation.Add(new ContactInformation
                {
                    CompanyName = "SmartAuditX",
                    SupportEmail = "support@smartauditx.com",
                    SalesEmail = "sales@smartauditx.com",
                    PhoneNumber = "+92 300 0000000",
                    Address = "SmartAuditX HQ, Karachi, Pakistan",
                    FacebookUrl = "https://facebook.com/smartauditx",
                    LinkedInUrl = "https://linkedin.com/company/smartauditx",
                    TwitterUrl = "https://twitter.com/smartauditx",
                    InstagramUrl = "https://instagram.com/smartauditx",
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}
