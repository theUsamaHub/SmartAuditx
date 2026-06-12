# SmartAuditX

## Overview
SmartAuditX is a comprehensive ASP.NET Core MVC web application designed for auditing, compliance, and organizational management. The platform provides a structured, multi-role environment that caters to various stakeholders including System Administrators, Company Representatives, Managers, and Auditors.

The application manages complex organizational hierarchies (Companies, Branches, Departments, Employees), tracks employee documentation, handles billing processes, and enforces strict access control with secure session management.

## Technologies Used

### Core Framework
- **.NET 10.0** - Latest .NET framework with modern features
- **ASP.NET Core MVC** - Web framework for building dynamic applications
- **ASP.NET Core Identity** - Authentication and authorization system
- **Razor Views & Razor Pages** - Server-side templating engine

### Data & Database
- **Microsoft SQL Server** - Primary database
- **Entity Framework Core 10.0.8** - Primary ORM for database operations
- **Dapper 2.1.79** - Micro-ORM for high-performance queries
- **Microsoft.Data.SqlClient 7.0.1** - SQL Server driver

### Security & Authentication
- **ASP.NET Core Identity** - User management and authentication
- **Role-Based Access Control (RBAC)** - Multi-role authorization
- **HTTP-Only Cookies** - Protection against XSS attacks
- **Secure Cookie Policies** - SameSite, Secure, and HttpOnly configurations
- **Session Management** - Server-side session state with secure policies

### Email & Communication
- **MailKit 4.17.0** - Email sending functionality
- **SMTP Configuration** - Gmail SMTP integration

### Additional Libraries
- **PSC.CSharp.Library.CountryData 8.0.9** - Country and city data management
- **Humanizer** - String and data manipulation utilities

### Development Tools
- **Entity Framework Core Tools** - Database migrations and scaffolding
- **Docker Support** - Containerization for deployment
- **Visual Studio Code Generation** - Scaffolding tools

## Key Features

### Multi-Role Architecture
- **Admin** - System administration and configuration
- **Auditor** - Audit management and compliance checking
- **Company** - Company representative interface
- **Manager** - Department and employee management
- **User** - General user access

### Organization Management
Complete tracking of organizational entities:
- **Companies** - Company registration and management
- **Company Contacts** - Contact person management
- **Branches** - Multi-branch support
- **Branch Departments** - Department assignment to branches
- **Departments** - Organizational department structure
- **Designations** - Employee designation/role management
- **Employees** - Comprehensive employee management with full CRUD operations, filtering, search, and active status management
- **Employee Documents** - Document upload, verification, and management for each employee
- **Employee Document Types** - Customizable document type definitions with required/optional flags

### Document Management
- **Employee Documents** - Document upload, verification, and management for each employee
- **Document Types** - Customizable document type definitions with required/optional flags
- **File Upload Service** - Secure file handling with validation
- **Document Verification** - Toggle verification status for uploaded documents

### Employee Management Features
Comprehensive employee management system with advanced capabilities:
- **Full CRUD Operations** - Create, read, update, and delete employee records
- **Advanced Filtering** - Filter employees by branch, department, designation, and active status
- **Search Functionality** - Search employees by name, email, or other criteria
- **Active Status Management** - Toggle employee active/inactive status
- **Document Management** - Upload and manage documents for each employee
- **Document Verification** - Verify employee documents with toggle functionality
- **Document Type Configuration** - Define custom document types with required/optional flags
- **Company-Scoped Operations** - All employee operations are scoped to the user's company
- **Validation & Error Handling** - Comprehensive validation with detailed error messages

### Billing Module
Comprehensive billing and subscription management:
- **Company Credits** - Credit system for companies
- **Company Subscriptions** - Subscription management
- **Subscription Plans** - Plan configuration and features
- **Subscription Plan Pricing** - Pricing tiers and features
- **Payment Processing** - Payment gateway integration
- **Payment Attempts** - Transaction tracking
- **Invoices** - Invoice generation and management
- **Refunds** - Refund processing
- **Promo Codes** - Discount and promotion system
- **Dunning Schedules** - Payment reminder automation
- **Tax Configuration** - Tax settings and calculations
- **Webhook Logs** - Payment gateway webhook tracking
- **Idempotency Keys** - Duplicate payment prevention

### Security Features
- **Customized Cookie Policies** - HttpOnly, Secure, SameSite configurations
- **Session Management** - 60-minute idle timeout, 7-day max age
- **Password Policies** - 8-character minimum, uppercase, lowercase, digit requirements
- **Lockout Policies** - 5 failed attempts, 15-minute lockout
- **Email Confirmation** - Optional email verification (configurable)
- **Token Providers** - 24-hour token lifespan for password reset and email confirmation

### Automated Features
- **Seed Service** - Automatic system administrator creation on startup
- **Database Migrations** - Automatic schema updates
- **Email Service** - Automated email notifications

## Project Structure

```
SmartAuditX/
├── Controllers/
│   ├── HomeController.cs
│   └── mvccontrollers/
│       ├── AdminController.cs
│       ├── AuditorController.cs
│       ├── AuthController.cs
│       ├── BranchController.cs
│       ├── BranchDepartmentController.cs
│       ├── CompanyContactController.cs
│       ├── CompanyController.cs
│       ├── DepartmentController.cs
│       ├── DesignationController.cs
│       ├── EmployeeController.cs
│       ├── EmployeeDocumentController.cs
│       ├── EmployeeDocumentTypeController.cs
│       ├── ManagerController.cs
│       ├── RegistrationController.cs
│       └── UserController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Extensions/
├── Helpers/
├── Migrations/
│   └── [17 migration files]
├── Models/
│   ├── ApplicationRole.cs
│   ├── ApplicationUser.cs
│   ├── ApplicationUserRole.cs
│   ├── AuditableEntity.cs
│   ├── BaseEntity.cs
│   ├── BillingModule/
│   │   ├── CompanyCredit.cs
│   │   ├── CompanySubscription.cs
│   │   ├── DunningSchedule.cs
│   │   ├── Enums/
│   │   ├── IdempotencyKey.cs
│   │   ├── Invoice.cs
│   │   ├── Payment.cs
│   │   ├── PaymentAttempt.cs
│   │   ├── PaymentGateway.cs
│   │   ├── PaymentNotification.cs
│   │   ├── PromoCode.cs
│   │   ├── PromoCodeUsage.cs
│   │   ├── Refund.cs
│   │   ├── SubscriptionPlan.cs
│   │   ├── SubscriptionPlanChange.cs
│   │   ├── SubscriptionPlanFeature.cs
│   │   ├── SubscriptionPlanPricing.cs
│   │   ├── TaxConfiguration.cs
│   │   └── WebhookLog.cs
│   ├── Branch.cs
│   ├── BranchDepartment.cs
│   ├── Company.cs
│   ├── CompanyContact.cs
│   ├── Department.cs
│   ├── Designation.cs
│   ├── EmailSetting.cs
│   ├── Employee.cs
│   ├── EmployeeDocument.cs
│   ├── EmployeeDocumentType.cs
│   ├── ErrorViewModel.cs
│   └── ViewModels/
│       ├── BranchDepartmentListItemViewModel.cs
│       ├── BranchDepartmentOperationResult.cs
│       ├── BranchDepartmentViewModel.cs
│       ├── BranchListItemViewModel.cs
│       ├── BranchOperationResult.cs
│       ├── BranchViewModel.cs
│       ├── CityViewModel.cs
│       ├── CompanyContactListItemViewModel.cs
│       ├── CompanyContactOperationResult.cs
│       ├── CompanyContactViewModel.cs
│       ├── CompleteRegistrationViewModel.cs
│       ├── CountryOptionViewModel.cs
│       ├── DepartmentListItemViewModel.cs
│       ├── DepartmentOperationResult.cs
│       ├── DepartmentViewModel.cs
│       ├── DesignationListItemViewModel.cs
│       ├── DesignationOperationResult.cs
│       ├── DesignationViewModel.cs
│       ├── EmployeeDocumentTypeViewModel.cs
│       ├── EmployeeDocumentViewModel.cs
│       ├── EmployeeListItemViewModel.cs
│       ├── EmployeeOperationResult.cs
│       ├── EmployeeViewModel.cs
│       ├── FileUploadResult.cs
│       ├── RegisterAccountViewModel.cs
│       ├── RegisterCompanyViewModel.cs
│       ├── RegistrationResult.cs
│       └── RegistrationSuccessViewModel.cs
├── Services/
│   ├── Implementations/
│   │   ├── BranchDepartmentService.cs
│   │   ├── BranchService.cs
│   │   ├── CityService.cs
│   │   ├── CompanyContactService.cs
│   │   ├── CountryService.cs
│   │   ├── DepartmentService.cs
│   │   ├── DesignationService.cs
│   │   ├── EmailService.cs
│   │   ├── EmployeeDocumentService.cs
│   │   ├── EmployeeDocumentTypeService.cs
│   │   ├── EmployeeService.cs
│   │   ├── FileService.cs
│   │   ├── RegistrationService.cs
│   │   └── SeedService.cs
│   └── Interfaces/
│       ├── IBranchDepartmentService.cs
│       ├── IBranchService.cs
│       ├── ICityService.cs
│       ├── ICompanyContactService.cs
│       ├── ICountryService.cs
│       ├── IDepartmentService.cs
│       ├── IDesignationService.cs
│       ├── IEmailService.cs
│       ├── IEmployeeDocumentService.cs
│       ├── IEmployeeDocumentTypeService.cs
│       ├── IEmployeeService.cs
│       ├── IFileService.cs
│       ├── IRegistrationService.cs
│       └── ISeedService.cs
├── Views/
│   ├── Admin/
│   ├── Auditor/
│   ├── Auth/
│   ├── Branch/
│   ├── BranchDepartment/
│   ├── Company/
│   ├── CompanyContact/
│   ├── Department/
│   ├── Designation/
│   ├── Employee/
│   ├── EmployeeDocumentType/
│   ├── Home/
│   ├── Manager/
│   ├── Registration/
│   ├── Shared/
│   ├── User/
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── Companylogo/
│   ├── css/
│   │   ├── [37 CSS files]
│   │   ├── Admin/
│   │   ├── Auditor/
│   │   ├── Company/
│   │   ├── Manager/
│   │   └── Public/
│   ├── favicon.ico
│   ├── js/
│   │   ├── [14 JS files]
│   │   ├── Admin/
│   │   ├── Auditor/
│   │   ├── Company/
│   │   ├── Manager/
│   │   └── Public/
│   └── lib/
│       └── [31 library files]
├── Areas/
│   └── Identity/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
├── SmartAuditX.csproj
└── .dockerignore
```

## Configuration

### Database Configuration
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SmartAuditX;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Email Configuration
Configure SMTP settings in `appsettings.json`:
```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "SmartAuditX",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
```

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft SQL Server (Express or full version)
- Visual Studio 2022 or VS Code (preferred IDE)
- Docker Desktop (optional, for containerized deployment)
- Git (for cloning the repository)

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd SmartAuditX
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Database**
   - Update the `DefaultConnection` string in `appsettings.json` or `appsettings.Development.json`
   - Ensure SQL Server is running and accessible

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Configure Email Settings** (Optional)
   - Update the `EmailSettings` section in `appsettings.json`
   - Use app-specific passwords for Gmail

6. **Run the Application**
   ```bash
   dotnet run
   ```

   The application will start on `https://localhost:5001` or `http://localhost:5000`

### Initial Setup
On the first run, the `SeedService` automatically creates a System Administrator account with default credentials configured in `Program.cs`. This account has full system access and can manage other users, roles, and system configurations.

## Docker Deployment

### Build Docker Image
```bash
docker build -t smartauditx .
```

### Run Container
```bash
docker run -p 5000:8080 -e ASPNETCORE_ENVIRONMENT=Production smartauditx
```

### Docker Compose (Optional)
Create a `docker-compose.yml` file for easy deployment with SQL Server container.

## Security Considerations

### Cookie Configuration
- **HttpOnly**: Prevents JavaScript access to cookies
- **Secure**: Cookies only sent over HTTPS
- **SameSite**: Lax mode for balanced security and usability
- **Expiration**: 30-day sliding expiration for authentication cookies

### Session Configuration
- **Idle Timeout**: 60 minutes of inactivity
- **Max Age**: 7 days for session cookie
- **HttpOnly**: Enabled for security
- **Essential**: Marked as essential for GDPR compliance

### Password Policy
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- Non-alphanumeric characters optional

### Lockout Policy
- 5 failed access attempts
- 15-minute lockout duration
- Automatic reset after successful login

## Development

### Adding New Migrations
```bash
dotnet ef migrations add MigrationName
```

### Rolling Back Migrations
```bash
dotnet ef database update PreviousMigration
```

### Running in Development Mode
```bash
dotnet run --environment Development
```

## API & Service Layer

The application follows a clean architecture with:
- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and data operations
- **Interfaces**: Service contracts for dependency injection
- **ViewModels**: Data transfer objects for views
- **Models**: Database entities and domain models

## Role-Based Access Control

### Available Roles
- **Admin**: Full system access, user management, configuration
- **Auditor**: Audit management, compliance checking
- **Company**: Company-specific operations, employee management
- **Manager**: Department management, team oversight
- **User**: Basic access, personal information management

### Authorization
Controllers and actions are protected using `[Authorize]` and `[Authorize(Roles="RoleName")]` attributes to ensure proper access control.

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure SQL Server accepts remote connections if needed
- Verify TrustServerCertificate is set to True for development

### Email Sending Issues
- Verify SMTP credentials
- Check if app-specific password is used for Gmail
- Ensure port 587 is not blocked by firewall
- Test SMTP configuration separately

### Migration Errors
- Ensure EF Core tools are installed: `dotnet tool install --global dotnet-ef`
- Check for pending model changes
- Verify database is accessible
- Review migration files for conflicts

## Future Enhancements

- [ ] Two-factor authentication (2FA)
- [ ] API documentation with Swagger
- [ ] Unit and integration tests
- [ ] Performance monitoring and logging
- [ ] Real-time notifications with SignalR
- [ ] Advanced reporting and analytics
- [ ] Mobile application support

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

[Add your license information here]

## Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check documentation for common issues

## Acknowledgments

Built with modern .NET technologies and following best practices for security, scalability, and maintainability.
