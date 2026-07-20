namespace SmartAuditX.Services.Interfaces
{
    public class CompanyDashboardStats
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalBranches { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalAudits { get; set; }
        public int ScheduledAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int TotalTemplates { get; set; }
        public int PublishedTemplates { get; set; }
        public int TotalDocuments { get; set; }
    }

    public interface ICompanyDashboardService
    {
        Task<CompanyDashboardStats> GetStatsAsync(int companyId);
    }
}
