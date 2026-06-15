
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models;
using SmartAuditX.Models.SmartAuditX.Models.Settings;
using SmartAuditX.Services.Implementations;
using SmartAuditX.Services.Interfaces;
using SmartAuditX.Services.Implementations.CMS;
using SmartAuditX.Services.Interfaces.CMS;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // User settings
        options.User.RequireUniqueEmail = true;
        // Lockout settings
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        // Sign In settings
        options.SignIn.RequireConfirmedEmail = true; //we will uncomment it later WHEN GO TO THE PHASE A-II
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    //Purpose: Prevents client-side JavaScript from accessing the cookie
    //Why important: Protects against Cross - Site Scripting(XSS) attacks
    //Effect: Cookie can only be sent to the server via HTTP requests

    options.Cookie.SameSite = SameSiteMode.Lax;
    //Purpose: Controls whether cookies are sent with cross - site requests
    //Values: Lax(default), Strict, None
    //Lax behavior: Cookie sent when user navigates to site via link(GET requests) but not for POST requests from other sites
    //Why Lax ?: Good balance of security and usability for login cookies

    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //Purpose: Ensures cookie is only sent over HTTPS connections
    //Values: Always, SameAsRequest, None
    //Why Always: Prevents cookie theft over insecure HTTP connections
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    //Purpose: Sets how long the authentication cookie remains valid
    //Current: 30 days
    //Default without configuration: 14 days
    //How Remember Me works: When user checks "Remember Me", this 30 - day timer applies. When unchecked, cookie expires when browser closes
    //Your LoginModel passes this: Input.RememberMe → PasswordSignInAsync → sets cookie duration based on this value
    options.SlidingExpiration = true;
    //Purpose: Resets the expiration timer on each request when user is active
    //How it works:
    //With true: Cookie expires 30 days from the user's LAST request
    //With false: Cookie expires 30 days from initial login, regardless of activity
    //Example: User logs in day 0, sets cookie for 30 days.On day 25 they make a request → cookie expiration extends another 30 days from day 25
    //Best for: Users who stay actively logged in for months / years
    options.LoginPath = "/Identity/Account/Login";
    //Purpose: Redirects unauthorized users to this login page
    //When triggered: When[Authorize] attribute is on a controller / page and user isn't authenticated
    options.LogoutPath = "/Identity/Account/Logout";
    //Purpose: Default URL for sign -out requests
    //What happens: Clears the authentication cookie
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    //Purpose: Redirects users who are authenticated but lack required roles / policies
    //When triggered: User has valid login but[Authorize(Roles = "Admin")] fails
    options.Cookie.MaxAge = TimeSpan.FromDays(30);
    //Purpose: Explicitly sets the cookie's maximum age in the browser
    //Relationship to ExpireTimeSpan: They typically match.ExpireTimeSpan is the server - side validation, MaxAge is the browser - side instruction
    //Why set both: Ensures browser respects the expiration even if server validation fails
});
builder.Services.AddSession(options =>
{
   options.IdleTimeout = TimeSpan.FromMinutes(60);
//Purpose: How long session can be idle before it expires on the server
//Current: 60 minutes
//Default: 20 minutes
//Behavior: Timer resets with each request
//Difference from cookie: Cookie lives on client, session data lives on server

options.Cookie.HttpOnly = true;
//    Same as application cookie - prevents JavaScript access
//Session - specific: Controls the session ID cookie, not the auth cookie
    options.Cookie.IsEssential = true;
//Purpose: Allows session cookie to work without user consent under GDPR
//Why important: Without true, some browsers might block session cookies until user accepts cookies
//Essential session: Storing shopping cart, login state -user expects this functionality

    options.Cookie.MaxAge = TimeSpan.FromDays(7);
//Purpose: Maximum age for the session ID cookie in the browser
//Current: 7 days
//Relationship to IdleTimeout: Session dies when EITHER:
//No activity for 60 minutes(IdleTimeout)
//Cookie reaches 7 days old(MaxAge)
//Whichever comes first wins

});

builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan =
            TimeSpan.FromHours(24);
    });

builder.Services.AddScoped<ISeedService, SeedService>(); //added seed service to the DI container
builder.Services.AddScoped<  IRegistrationService, RegistrationService>(); //added company registeration service to the DI container
builder.Services.AddScoped<IFileService, FileService>(); //added file service to the DI container
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICompanyContactService, CompanyContactService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IBranchDepartmentService, BranchDepartmentService>();
builder.Services.AddScoped<IEmployeeDocumentTypeService, EmployeeDocumentTypeService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
builder.Services.AddScoped<IEmailService, EmailService>();


// CMS Services
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IAboutUsService, AboutUsService>();
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();
builder.Services.AddScoped<IContactInformationService, ContactInformationService>();
builder.Services.AddScoped<IHeroSectionService, HeroSectionService>();
builder.Services.AddScoped<IHowItWorksService, HowItWorksService>();
builder.Services.AddScoped<IPlatformModuleService, PlatformModuleService>();
builder.Services.AddScoped<ISecurityFeatureService, SecurityFeatureService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();  //added this for the internal ui of identity
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var seedService =
        scope.ServiceProvider
             .GetRequiredService<ISeedService>();

    await seedService.SeedSystemAdminAsync();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession(); //added for the session
app.UseAuthentication(); //added this
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();