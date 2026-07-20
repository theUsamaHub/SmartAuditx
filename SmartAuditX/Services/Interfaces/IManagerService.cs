using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Models.ViewModels.AuditModule;

namespace SmartAuditX.Services.Interfaces
{
    public class ManagerDashboardStats
    {
        public int TotalAudits { get; set; }
        public int ScheduledAudits { get; set; }
        public int InProgressAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int UnderReviewAudits { get; set; }
        public int ApprovedAudits { get; set; }
        public decimal AverageScore { get; set; }
    }

    public interface IManagerService
    {
        Task<ManagerDashboardStats> GetStatsAsync(int companyId, int? branchId);
        Task<List<AuditViewModel>> GetAuditsByCompanyAsync(int companyId, int? branchId);
        Task<AuditConductViewModel?> GetAuditForReviewAsync(int auditId, int companyId);
        Task<(bool Success, string Message)> ApproveAuditAsync(int auditId, int companyId, int userId, string? reviewNotes);
        Task<(bool Success, string Message)> RejectAuditAsync(int auditId, int companyId, int userId, string? reviewNotes);
    }
}
