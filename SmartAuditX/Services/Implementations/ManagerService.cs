using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartAuditX.Services.Implementations
{
    public class ManagerService : IManagerService
    {
        private readonly ApplicationDbContext _context;

        public ManagerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ManagerDashboardStats> GetStatsAsync(int companyId, int? branchId)
        {
            var query = _context.Audits.Where(a => a.CompanyId == companyId);
            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId.Value);

            var audits = await query.ToListAsync();

            var completedAudits = audits.Where(a => a.Status == AuditStatus.Completed || a.Status == AuditStatus.Approved).ToList();

            return new ManagerDashboardStats
            {
                TotalAudits = audits.Count,
                ScheduledAudits = audits.Count(a => a.Status == AuditStatus.Scheduled),
                InProgressAudits = audits.Count(a => a.Status == AuditStatus.InProgress),
                CompletedAudits = audits.Count(a => a.Status == AuditStatus.Completed),
                UnderReviewAudits = audits.Count(a => a.Status == AuditStatus.Completed),
                ApprovedAudits = audits.Count(a => a.Status == AuditStatus.Approved),
                AverageScore = completedAudits.Any() ? Math.Round(completedAudits.Average(a => a.FinalScore ?? 0), 1) : 0
            };
        }

        public async Task<List<AuditViewModel>> GetAuditsByCompanyAsync(int companyId, int? branchId)
        {
            var query = _context.Audits
                .Include(a => a.AuditTemplate)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .Include(a => a.ReviewedByUser)
                .Where(a => a.CompanyId == companyId);

            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId.Value);

            var audits = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

            return audits.Select(a => new AuditViewModel
            {
                Id = a.Id,
                Title = a.Title,
                AuditTemplateId = a.AuditTemplateId,
                TemplateTitle = a.AuditTemplate?.Title,
                CompanyId = a.CompanyId,
                BranchId = a.BranchId,
                BranchName = a.Branch?.BranchName,
                Status = a.Status,
                ScheduledStartDate = a.ScheduledStartDate,
                ScheduledEndDate = a.ScheduledEndDate,
                ActualStartDate = a.ActualStartDate,
                ActualEndDate = a.ActualEndDate,
                FinalScore = a.FinalScore,
                AssignedToUserId = a.AssignedToUserId,
                AssignedToUserName = a.AssignedToUser?.UserName,
                ReviewedByUserId = a.ReviewedByUserId,
                ReviewedByUserName = a.ReviewedByUser?.UserName,
                ReviewedAt = a.ReviewedAt,
                Notes = a.Notes,
                ReviewNotes = a.ReviewNotes,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<AuditConductViewModel?> GetAuditForReviewAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.Fields)
                            .ThenInclude(f => f.Options)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return null;

            var responses = await _context.AuditResponses
                .Where(r => r.AuditId == auditId)
                .ToListAsync();

            var sections = audit.AuditTemplate?.Sections?
                .OrderBy(s => s.SortOrder)
                .Select(s => new AuditSectionConductViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    SortOrder = s.SortOrder,
                    Fields = s.Fields?.OrderBy(f => f.SortOrder).Select(f =>
                    {
                        var response = responses.FirstOrDefault(r => r.AuditTemplateFieldId == f.Id);
                        return new AuditFieldConductViewModel
                        {
                            Id = f.Id,
                            QuestionText = f.QuestionText,
                            HelpText = f.HelpText,
                            ItemType = f.ItemType,
                            IsRequired = f.IsRequired,
                            Weightage = f.Weightage,
                            MinValue = f.MinValue,
                            MaxValue = f.MaxValue,
                            AllowNotes = f.AllowNotes,
                            Options = f.Options?.OrderBy(o => o.SortOrder).Select(o => new AuditFieldOptionConductViewModel
                            {
                                Id = o.Id,
                                Text = o.Text
                            }).ToList() ?? new List<AuditFieldOptionConductViewModel>(),
                            ResponseText = response?.ResponseText,
                            ResponseNumber = response?.ResponseNumber,
                            ResponseBoolean = response?.ResponseBoolean,
                            ResponseDate = response?.ResponseDate,
                            SelectedOptionId = response?.SelectedOptionId,
                            Notes = response?.Notes,
                            IsSkipped = response?.IsSkipped ?? false
                        };
                    }).ToList() ?? new List<AuditFieldConductViewModel>()
                }).ToList() ?? new List<AuditSectionConductViewModel>();

            return new AuditConductViewModel
            {
                AuditId = audit.Id,
                AuditTitle = audit.Title,
                TemplateTitle = audit.AuditTemplate?.Title,
                Status = audit.Status,
                ScheduledStartDate = audit.ScheduledStartDate,
                Sections = sections
            };
        }

        public async Task<(bool Success, string Message)> ApproveAuditAsync(int auditId, int companyId, int userId, string? reviewNotes)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return (false, "Audit not found.");

            if (audit.Status != AuditStatus.Completed)
                return (false, $"Cannot approve audit. Current status is '{audit.Status}'. Only audits with 'Completed' status can be approved.");

            audit.Status = AuditStatus.Approved;
            audit.ReviewedByUserId = userId;
            audit.ReviewedAt = DateTimeOffset.UtcNow;
            audit.ReviewNotes = reviewNotes;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Audit approved successfully.");
        }

        public async Task<(bool Success, string Message)> RejectAuditAsync(int auditId, int companyId, int userId, string? reviewNotes)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return (false, "Audit not found.");

            if (audit.Status != AuditStatus.Completed)
                return (false, $"Cannot reject audit. Current status is '{audit.Status}'. Only audits with 'Completed' status can be rejected.");

            audit.Status = AuditStatus.Scheduled;
            audit.ReviewedByUserId = userId;
            audit.ReviewedAt = DateTimeOffset.UtcNow;
            audit.ReviewNotes = reviewNotes;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Audit sent back for correction.");
        }
    }
}
