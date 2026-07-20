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
    public class AuditorService : IAuditorService
    {
        private readonly ApplicationDbContext _context;

        public AuditorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditorStatsViewModel> GetStatsAsync(int userId)
        {
            var audits = await _context.Audits
                .Where(a => a.AssignedToUserId == userId && a.Status != AuditStatus.Cancelled)
                .ToListAsync();

            var completedAudits = audits.Where(a => a.Status == AuditStatus.Completed || a.Status == AuditStatus.Approved).ToList();

            return new AuditorStatsViewModel
            {
                TotalAssigned = audits.Count,
                Scheduled = audits.Count(a => a.Status == AuditStatus.Scheduled),
                InProgress = audits.Count(a => a.Status == AuditStatus.InProgress),
                Completed = audits.Count(a => a.Status == AuditStatus.Completed),
                Approved = audits.Count(a => a.Status == AuditStatus.Approved),
                Overdue = audits.Count(a => a.Status == AuditStatus.Scheduled && a.ScheduledStartDate < DateTimeOffset.UtcNow),
                AverageScore = completedAudits.Any() ? Math.Round(completedAudits.Average(a => a.FinalScore ?? 0), 1) : 0
            };
        }

        public async Task<IEnumerable<AuditViewModel>> GetAssignedAuditsAsync(int userId)
        {
            var audits = await _context.Audits
                .Include(a => a.AuditTemplate)
                .Include(a => a.Branch)
                .Include(a => a.AssignedToUser)
                .Where(a => a.AssignedToUserId == userId && a.Status != AuditStatus.Cancelled)
                .OrderBy(a => a.ScheduledStartDate)
                .ToListAsync();

            return audits.Select(a => new AuditViewModel
            {
                Id = a.Id,
                Title = a.Title,
                AuditTemplateId = a.AuditTemplateId,
                TemplateTitle = a.AuditTemplate?.Title,
                BranchName = a.Branch?.BranchName,
                Status = a.Status,
                ScheduledStartDate = a.ScheduledStartDate,
                ScheduledEndDate = a.ScheduledEndDate,
                ActualStartDate = a.ActualStartDate,
                ActualEndDate = a.ActualEndDate,
                FinalScore = a.FinalScore,
                Notes = a.Notes
            });
        }

        public async Task<AuditConductViewModel?> GetAuditForConductAsync(int auditId, int userId)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.Fields)
                            .ThenInclude(f => f.Options)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.AssignedToUserId == userId);

            if (audit == null)
                return null;

            // Load existing responses
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

        public async Task<bool> StartAuditAsync(int auditId, int userId)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.AssignedToUserId == userId);

            if (audit == null || audit.Status != AuditStatus.Scheduled)
                return false;

            audit.Status = AuditStatus.InProgress;
            audit.ActualStartDate = DateTimeOffset.UtcNow;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveResponsesAsync(int auditId, int userId, List<AuditResponseViewModel> responses)
        {
            var audit = await _context.Audits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.AssignedToUserId == userId);

            if (audit == null || audit.Status != AuditStatus.InProgress)
                return false;

            await SaveResponsesToDb(auditId, responses);
            return true;
        }

        public async Task<bool> SubmitAuditAsync(int auditId, int userId, List<AuditResponseViewModel> responses)
        {
            var audit = await _context.Audits
                .Include(a => a.AuditTemplate)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.AssignedToUserId == userId);

            if (audit == null || audit.Status != AuditStatus.InProgress)
                return false;

            // Save all responses
            await SaveResponsesToDb(auditId, responses);

            // Calculate score if scoring enabled
            if (audit.AuditTemplate?.IsScoringEnabled == true)
            {
                var score = await CalculateScoreAsync(auditId);
                audit.FinalScore = score;
            }

            audit.Status = AuditStatus.Completed;
            audit.ActualEndDate = DateTimeOffset.UtcNow;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SaveResponsesToDb(int auditId, List<AuditResponseViewModel> responses)
        {
            // Remove existing responses for this audit
            var existingResponses = await _context.AuditResponses
                .Where(r => r.AuditId == auditId)
                .ToListAsync();
            _context.AuditResponses.RemoveRange(existingResponses);

            // Add new responses
            foreach (var response in responses)
            {
                var field = await _context.AuditTemplateFields
                    .FirstOrDefaultAsync(f => f.Id == response.AuditTemplateFieldId);

                if (field == null) continue;

                var entity = new AuditResponse
                {
                    AuditId = auditId,
                    AuditTemplateFieldId = response.AuditTemplateFieldId,
                    FieldLabelSnapshot = field.QuestionText,
                    FieldTypeSnapshot = field.ItemType,
                    ResponseText = response.ResponseText,
                    ResponseNumber = response.ResponseNumber,
                    ResponseBoolean = response.ResponseBoolean,
                    ResponseDate = response.ResponseDate,
                    SelectedOptionId = response.SelectedOptionId,
                    Notes = response.Notes,
                    IsSkipped = response.IsSkipped,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditResponses.Add(entity);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<decimal> CalculateScoreAsync(int auditId)
        {
            var responses = await _context.AuditResponses
                .Include(r => r.AuditTemplateField)
                .Where(r => r.AuditId == auditId && !r.IsSkipped)
                .ToListAsync();

            if (!responses.Any())
                return 0;

            decimal totalWeight = 0;
            decimal weightedScore = 0;

            foreach (var response in responses)
            {
                if (response.AuditTemplateField == null) continue;

                var weight = response.AuditTemplateField.Weightage;
                totalWeight += weight;

                // Simple scoring: Boolean Yes = full weight, No = 0
                if (response.ResponseBoolean == true)
                {
                    weightedScore += weight;
                }
                else if (response.ResponseNumber.HasValue && response.AuditTemplateField.MaxValue > 0)
                {
                    // For number/rating fields, score proportionally
                    var maxVal = response.AuditTemplateField.MaxValue.Value;
                    var proportion = response.ResponseNumber.Value / maxVal;
                    weightedScore += weight * Math.Min(proportion, 1);
                }
            }

            if (totalWeight == 0) return 0;
            return Math.Round((weightedScore / totalWeight) * 100, 2);
        }
    }
}
