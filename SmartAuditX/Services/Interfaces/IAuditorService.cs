using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Models.ViewModels.AuditModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAuditX.Services.Interfaces
{
    public class AuditorStatsViewModel
    {
        public int TotalAssigned { get; set; }
        public int Scheduled { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Approved { get; set; }
        public int Overdue { get; set; }
        public decimal AverageScore { get; set; }
    }

    public interface IAuditorService
    {
        Task<AuditorStatsViewModel> GetStatsAsync(int userId);
        Task<IEnumerable<AuditViewModel>> GetAssignedAuditsAsync(int userId);
        Task<AuditConductViewModel?> GetAuditForConductAsync(int auditId, int userId);
        Task<bool> StartAuditAsync(int auditId, int userId);
        Task<bool> SaveResponsesAsync(int auditId, int userId, List<AuditResponseViewModel> responses);
        Task<bool> SubmitAuditAsync(int auditId, int userId, List<AuditResponseViewModel> responses);
    }
}
