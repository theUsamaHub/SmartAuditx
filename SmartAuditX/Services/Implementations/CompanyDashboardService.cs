using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartAuditX.Services.Implementations
{
    public class CompanyDashboardService : ICompanyDashboardService
    {
        private readonly ApplicationDbContext _context;

        public CompanyDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyDashboardStats> GetStatsAsync(int companyId)
        {
            return new CompanyDashboardStats
            {
                TotalEmployees = await _context.Employees.CountAsync(e => e.CompanyId == companyId && !e.IsDeleted),
                ActiveEmployees = await _context.Employees.CountAsync(e => e.CompanyId == companyId && !e.IsDeleted && e.IsActive),
                TotalBranches = await _context.Branches.CountAsync(b => b.CompanyId == companyId && !b.IsDeleted),
                TotalDepartments = await _context.Departments.CountAsync(d => d.CompanyId == companyId && !d.IsDeleted),
                TotalAudits = await _context.Audits.CountAsync(a => a.CompanyId == companyId),
                ScheduledAudits = await _context.Audits.CountAsync(a => a.CompanyId == companyId && a.Status == AuditStatus.Scheduled),
                CompletedAudits = await _context.Audits.CountAsync(a => a.CompanyId == companyId && (a.Status == AuditStatus.Completed || a.Status == AuditStatus.Approved)),
                TotalTemplates = await _context.AuditTemplates.CountAsync(t => t.CompanyId == companyId && !t.IsDeleted),
                PublishedTemplates = await _context.AuditTemplates.CountAsync(t => t.CompanyId == companyId && !t.IsDeleted && t.IsPublished),
                TotalDocuments = await _context.EmployeeDocuments.CountAsync(d => d.Employee.CompanyId == companyId)
            };
        }
    }
}
