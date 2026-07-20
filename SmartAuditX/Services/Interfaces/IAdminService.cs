namespace SmartAuditX.Services.Interfaces
{
    public class AdminDashboardStats
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int PendingAudits { get; set; }
        public int TotalTemplates { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class AdminUserListItem
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCompanyListItem
    {
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? CountryCode { get; set; }
        public int EmployeeCount { get; set; }
        public int UserCount { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IAdminService
    {
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<List<AdminUserListItem>> GetAllUsersAsync(string? search, bool? isActive);
        Task<List<AdminCompanyListItem>> GetAllCompaniesAsync(string? search, bool? isActive);
        Task<bool> ToggleUserActiveAsync(int userId);
        Task<bool> ToggleCompanyActiveAsync(int companyId);
    }
}
