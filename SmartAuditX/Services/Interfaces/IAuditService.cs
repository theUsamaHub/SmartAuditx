using SmartAuditX.Models.AuditModule.AuditEnums;
using SmartAuditX.Models.ViewModels.AuditModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAuditX.Services.Interfaces
{
    public class AuditorDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? EmployeeId { get; set; }
        public string? FullName { get; set; }
    }

    public interface IAuditService
    {
        Task<IEnumerable<AuditViewModel>> GetAuditsByCompanyAsync(int companyId);
        Task<AuditViewModel?> GetAuditByIdAsync(int auditId, int companyId);
        Task<int?> CreateAuditAsync(CreateAuditViewModel model, int companyId, int userId);
        Task<bool> UpdateAuditAsync(CreateAuditViewModel model, int auditId, int companyId);
        Task<bool> DeleteAuditAsync(int auditId, int companyId);
        Task<bool> UpdateAuditStatusAsync(int auditId, AuditStatus status, int companyId);
        Task<IEnumerable<AuditorDto>> GetAuditorsForCompanyAsync(int companyId);
        Task<AuditConductViewModel?> GetAuditForReviewAsync(int auditId, int companyId);
        Task<bool> ApproveAuditAsync(int auditId, int companyId, int userId, string? reviewNotes);
        Task<bool> RejectAuditAsync(int auditId, int companyId, int userId, string? reviewNotes);
    }
}
