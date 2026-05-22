namespace SmartAuditX.Services.Interfaces
{
    public interface ISeedService
    {
        Task SeedSystemAdminAsync(); //created this to seed the admin for the first time when the system is deployed. After that, the admin can create other users and assign roles.    
    }
}