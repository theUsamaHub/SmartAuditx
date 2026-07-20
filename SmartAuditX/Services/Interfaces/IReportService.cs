namespace SmartAuditX.Services.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateAuditReportAsync(int auditId, int companyId);
        Task<byte[]> GenerateInventoryReportAsync(int auditId, int companyId);
    }
}
