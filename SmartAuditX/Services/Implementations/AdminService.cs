using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartAuditX.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            return new AdminDashboardStats
            {
                TotalCompanies = await _context.Companies.CountAsync(c => !c.IsDeleted),
                ActiveCompanies = await _context.Companies.CountAsync(c => !c.IsDeleted && c.IsActive),
                TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
                ActiveUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive),
                TotalAudits = await _context.Audits.CountAsync(),
                CompletedAudits = await _context.Audits.CountAsync(a => a.Status == AuditStatus.Completed || a.Status == AuditStatus.Approved),
                PendingAudits = await _context.Audits.CountAsync(a => a.Status == AuditStatus.Scheduled || a.Status == AuditStatus.InProgress),
                TotalTemplates = await _context.AuditTemplates.CountAsync(t => !t.IsDeleted),
                TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted)
            };
        }

        public async Task<List<AdminUserListItem>> GetAllUsersAsync(string? search, bool? isActive)
        {
            var query = _context.Users
                .Where(u => !u.IsDeleted)
                .Include(u => u.Company)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            // Get roles for each user
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .ToListAsync();

            return users.Select(u => new AdminUserListItem
            {
                UserId = u.Id,
                Username = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                CompanyId = u.CompanyId,
                CompanyName = u.Company?.Name,
                Role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.Role?.Name,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminCompanyListItem>> GetAllCompaniesAsync(string? search, bool? isActive)
        {
            var query = _context.Companies
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.City != null && c.City.ToLower().Contains(term)));
            }

            var companies = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            // Get counts
            var companyIds = companies.Select(c => c.CompanyId).ToList();
            var employeeCounts = await _context.Employees
                .Where(e => companyIds.Contains(e.CompanyId) && !e.IsDeleted)
                .GroupBy(e => e.CompanyId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CompanyId, x => x.Count);

            var userCounts = await _context.Users
                .Where(u => companyIds.Contains(u.CompanyId) && !u.IsDeleted)
                .GroupBy(u => u.CompanyId)
                .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CompanyId, x => x.Count);

            return companies.Select(c => new AdminCompanyListItem
            {
                CompanyId = c.CompanyId,
                Name = c.Name,
                Industry = c.IndustryType?.ToString(),
                City = c.City,
                CountryCode = c.CountryCode,
                EmployeeCount = employeeCounts.GetValueOrDefault(c.CompanyId, 0),
                UserCount = userCounts.GetValueOrDefault(c.CompanyId, 0),
                OnboardingStatus = c.OnboardingStatus.ToString(),
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<bool> ToggleUserActiveAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleCompanyActiveAsync(int companyId)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId && !c.IsDeleted);
            if (company == null) return false;

            company.IsActive = !company.IsActive;
            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
