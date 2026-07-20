using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Models.ViewModels.AuditModule;
using SmartAuditX.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAuditX.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditViewModel>> GetAuditsByCompanyAsync(int companyId)
        {
            var audits = await _context.Audits
                .Include(a => a.AuditTemplate)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .Include(a => a.ReviewedByUser)
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return audits.Select(MapToViewModel);
        }

        public async Task<AuditViewModel?> GetAuditByIdAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .Include(a => a.ReviewedByUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            return audit == null ? null : MapToViewModel(audit);
        }

        public async Task<int?> CreateAuditAsync(CreateAuditViewModel model, int companyId, int userId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == model.AuditTemplateId && t.CompanyId == companyId && t.IsPublished);

            if (template == null)
                return null;

            var audit = new Audit
            {
                AuditTemplateId = model.AuditTemplateId,
                CompanyId = companyId,
                BranchId = model.BranchId,
                TemplateVersionSnapshot = template.Version,
                Title = model.Title,
                Status = AuditStatus.Scheduled,
                ScheduledStartDate = model.ScheduledStartDate,
                ScheduledEndDate = model.ScheduledEndDate,
                AssignedToUserId = model.AssignedToUserId,
                Notes = model.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Audits.Add(audit);
            await _context.SaveChangesAsync();

            return audit.Id;
        }

        public async Task<bool> UpdateAuditAsync(CreateAuditViewModel model, int auditId, int companyId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return false;

            audit.Title = model.Title;
            audit.BranchId = model.BranchId;
            audit.ScheduledStartDate = model.ScheduledStartDate;
            audit.ScheduledEndDate = model.ScheduledEndDate;
            audit.AssignedToUserId = model.AssignedToUserId;
            audit.Notes = model.Notes;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAuditAsync(int auditId, int companyId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return false;

            // Use Cancelled status instead of soft delete
            audit.Status = AuditStatus.Cancelled;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAuditStatusAsync(int auditId, AuditStatus status, int companyId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null)
                return false;

            audit.Status = status;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AuditorDto>> GetAuditorsForCompanyAsync(int companyId)
        {
            var auditors = await _context.Users
                .Where(u => u.CompanyId == companyId && !u.IsDeleted && u.IsActive)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == "Auditor")
                .Select(x => new AuditorDto
                {
                    Id = x.u.Id,
                    Name = x.u.UserName,
                    Email = x.u.Email,
                    EmployeeId = x.u.EmployeeId
                })
                .ToListAsync();

            // Enrich with employee names from Employee table
            foreach (var auditor in auditors)
            {
                if (auditor.EmployeeId.HasValue)
                {
                    var employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.EmployeeId == auditor.EmployeeId.Value && !e.IsDeleted);
                    if (employee != null)
                    {
                        auditor.FullName = $"{employee.FirstName} {employee.LastName}".Trim();
                    }
                }
            }

            return auditors;
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

        public async Task<bool> ApproveAuditAsync(int auditId, int companyId, int userId, string? reviewNotes)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null || audit.Status != AuditStatus.Completed)
                return false;

            audit.Status = AuditStatus.Approved;
            audit.ReviewedByUserId = userId;
            audit.ReviewedAt = DateTimeOffset.UtcNow;
            audit.ReviewNotes = reviewNotes;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAuditAsync(int auditId, int companyId, int userId, string? reviewNotes)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.CompanyId == companyId);

            if (audit == null || audit.Status != AuditStatus.Completed)
                return false;

            audit.Status = AuditStatus.Scheduled;
            audit.ReviewedByUserId = userId;
            audit.ReviewedAt = DateTimeOffset.UtcNow;
            audit.ReviewNotes = reviewNotes;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private AuditViewModel MapToViewModel(Audit audit)
        {
            return new AuditViewModel
            {
                Id = audit.Id,
                Title = audit.Title,
                AuditTemplateId = audit.AuditTemplateId,
                TemplateTitle = audit.AuditTemplate?.Title,
                CompanyId = audit.CompanyId,
                BranchId = audit.BranchId,
                BranchName = audit.Branch?.BranchName,
                Status = audit.Status,
                ScheduledStartDate = audit.ScheduledStartDate,
                ScheduledEndDate = audit.ScheduledEndDate,
                ActualStartDate = audit.ActualStartDate,
                ActualEndDate = audit.ActualEndDate,
                FinalScore = audit.FinalScore,
                AssignedToUserId = audit.AssignedToUserId,
                AssignedToUserName = audit.AssignedToUser?.UserName,
                ReviewedByUserId = audit.ReviewedByUserId,
                ReviewedByUserName = audit.ReviewedByUser?.UserName,
                ReviewedAt = audit.ReviewedAt,
                Notes = audit.Notes,
                ReviewNotes = audit.ReviewNotes,
                CreatedByUserId = audit.CreatedByUserId,
                CreatedAt = audit.CreatedAt
            };
        }
    }
}
