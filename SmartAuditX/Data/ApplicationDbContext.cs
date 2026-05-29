using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartAuditX.Models;


namespace SmartAuditX.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            int,
            IdentityUserClaim<int>,
            ApplicationUserRole,
            IdentityUserLogin<int>,
            IdentityRoleClaim<int>,
            IdentityUserToken<int>>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {

        }
        public DbSet<Company> Companies { get; set; } //added this DbSet to allow us to query and manage companies through the ApplicationDbContext, which is necessary for the company management features of the platform.
        public DbSet<CompanyContact> CompanyContacts { get; set; } //added this DbSet to allow us to query and manage company contacts through the ApplicationDbContext, which is necessary for the company contact management features of the platform.

        public DbSet<Branch> Branches { get; set; } //  added this DbSet to allow us to query and manage branches through the ApplicationDbContext, which is necessary for the branch management features of the platform.

        public DbSet<Department> Departments { get; set; } //added this DbSet to allow us to query and manage departments through the ApplicationDbContext, which is necessary for the department management features of the platform.

        public DbSet<BranchDepartment> BranchDepartments { get; set; }

        public DbSet<Designation> Designations { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeDocumentType> EmployeeDocumentTypes { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================
            // TABLE NAMES
            // =========================

            builder.Entity<ApplicationUser>()
                .ToTable("Users");

            builder.Entity<ApplicationRole>()
                .ToTable("Roles");

            builder.Entity<ApplicationUserRole>()
                .ToTable("UserRoles");


            builder.Entity<IdentityUserClaim<int>>()
                .ToTable("UserClaims");

            builder.Entity<IdentityUserLogin<int>>()
                .ToTable("UserLogins");

            builder.Entity<IdentityUserToken<int>>()
                .ToTable("UserTokens");

            builder.Entity<IdentityRoleClaim<int>>()
                .ToTable("RoleClaims");

            // =========================
            // USERS TABLE
            // =========================

            builder.Entity<ApplicationUser>(entity =>
            {
                // Primary Key Rename
                entity.Property(x => x.Id)
                    .HasColumnName("UserId");

                // CompanyId (MANDATORY FIELD)
                entity.Property(x => x.CompanyId)
                    .IsRequired();

                // Username
                entity.Property(x => x.UserName)
                    .HasColumnName("Username")
                    .HasMaxLength(50)
                    .IsRequired();

                // Email
                entity.Property(x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                // ── New Field ──────────────────────────────────────
                entity.Property(x => x.PhoneDialCode)
                    .HasMaxLength(5)
                    .IsRequired();

                // Phone Number
                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                // Password Hash
                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(500);

                // Custom Fields
                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Index for performance (multi-tenant filtering)
                entity.HasIndex(x => x.CompanyId);

                // Unique Constraints
                entity.HasIndex(x => x.UserName)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0"); //important for 

                entity.HasIndex(x => x.Email)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0"); //important for 

                entity.HasIndex(x => x.PhoneNumber)
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0"); //important for 

                entity.HasIndex(x => x.IsDeleted);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.CreatedAt);
                // ── Fix: Company relationship now has navigation ───
                // Replace your existing HasOne with this:
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Soft Delete Filter 
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            // =========================
            // ROLES TABLE
            // =========================

            builder.Entity<ApplicationRole>(entity =>
            {
                // Primary Key Rename
                entity.Property(x => x.Id)
                    .HasColumnName("RoleId");

                // Name
                entity.Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                // Description
                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                // Custom Fields
                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Unique Constraint
                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });

            // =========================
            // USER ROLES TABLE
            // =========================

            //builder.Entity<ApplicationUserRole>(entity =>
            //{
            //    //// Primary Key
            //    //entity.HasKey(x => x.UserRoleId);

            //    //entity.Property(x => x.UserRoleId)
            //    //    .ValueGeneratedOnAdd();

            //    // CreatedAt
            //    entity.Property(x => x.CreatedAt)
            //        .HasDefaultValueSql("GETUTCDATE()");

            //    // Composite Unique Constraint
            //    entity.HasIndex(x => new { x.UserId, x.RoleId })
            //        .IsUnique();

            //    // Relationships
            //    //entity.HasOne(x => x.User)
            //    //    .WithMany()
            //    //    .HasForeignKey(x => x.UserId)
            //    //    .OnDelete(DeleteBehavior.Cascade);

            //    //entity.HasOne(x => x.Role)
            //    //    .WithMany()
            //    //    .HasForeignKey(x => x.RoleId)
            //    //    .OnDelete(DeleteBehavior.Cascade);
            //    entity.HasOne(x => x.User)
            //          .WithMany(x => x.UserRoles)
            //          .HasForeignKey(x => x.UserId)
            //           .OnDelete(DeleteBehavior.Restrict);

            //    entity.HasOne(x => x.Role)
            //        .WithMany(x => x.UserRoles)
            //        .HasForeignKey(x => x.RoleId)
            //        .OnDelete(DeleteBehavior.Restrict);
            //});

            builder.Entity<ApplicationUserRole>(entity =>
            {
                // Composite Primary Key
                entity.HasKey(x => new
                {
                    x.UserId,
                    x.RoleId
                });

                // CreatedAt
                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(x => x.User)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            }); 
            // =========================
            // COMPANIES TABLE
            // =========================

            builder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies");

                // Primary Key
                entity.HasKey(x => x.CompanyId);

                entity.Property(x => x.CompanyId)
                    .ValueGeneratedOnAdd();

                // Name
                entity.Property(x => x.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                // Industry - NOW AN ENUM
                entity.Property(x => x.IndustryType)
                    .HasConversion<string>()  // Store as string in DB (e.g., "IT", "Healthcare")
                    .HasMaxLength(50)          // Max enum name length
                    .IsRequired(false);        // Nullable

                // Logo
                entity.Property(x => x.LogoUrl)
                    .HasMaxLength(500);

                // Flags
                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                // Dates
                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                //updated at by default will be null until the record is updated for the first time, so we set the default value to null
                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null);

                // ── New Fields ─────────────────────────────────────

                entity.Property(x => x.Website)
                    .HasMaxLength(255);

                //entity.Property(x => x.RegistrationNumber)
                //    .HasMaxLength(100);

                //entity.Property(x => x.TaxNumber)
                //    .HasMaxLength(100);

                // Employee Count Range - NOW AN ENUM
                entity.Property(x => x.EmployeeCountRange)
                    .HasConversion<string>()  // Store as string in DB (e.g., "Small", "Medium")
                    .HasMaxLength(30)          // Max enum name length
                    .IsRequired(false);        // Nullable

                entity.Property(x => x.CountryCode)
                    .HasMaxLength(2);

                entity.Property(x => x.City)
                    .HasMaxLength(100);

                entity.Property(x => x.ReferralSource)
                    .HasMaxLength(100);

                // ── Enums stored as strings (not integers) ─────────
                // Without HasConversion EF Core stores enums as 0,1,2
                // With it you get "Active", "CompanyInfoSaved" — readable in DB

                // Enums stored as strings (not integers)
                entity.Property(x => x.CompanySize)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(x => x.OnboardingStatus)
        .HasConversion<string>()
        .HasMaxLength(30)
        .IsRequired()
        .HasDefaultValue(OnboardingStatus.CompanyInfoSaved);

                // ── New Indexes ────────────────────────────────────
                entity.HasIndex(x => x.OnboardingStatus); // filter companies by funnel stage
                entity.HasIndex(x => x.CountryCode);      // filter by region

                // Optional: Add indexes for new enum fields if you query by them often
                entity.HasIndex(x => x.IndustryType);
                entity.HasIndex(x => x.EmployeeCountRange);
                // ── Fix relationship — Company now has Users collection
                entity.HasMany(x => x.Users)
                    .WithOne(x => x.Company)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.IsDeleted);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.CreatedAt);
                // Soft Delete Filter
                entity.HasQueryFilter(x => !x.IsDeleted); //we have to add this filter to ensure that when we query the companies, we only get the ones that are not deleted. This is important for the soft delete functionality to work correctly.
            });
            // =========================
            // COMPANY CONTACT TABLES
            // =========================

            builder.Entity<CompanyContact>(entity =>
            {
                entity.ToTable("CompanyContacts");

                entity.HasKey(x => x.CompanyContactId);

                entity.Property(x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ContactName)
                    .HasMaxLength(150);

                entity.Property(x => x.FaxNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.PhysicalAddress)
                    .HasMaxLength(500);

                entity.Property(x => x.IsPrimary)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null); //updated at by default will be null until the record is updated for the first time, so we set the default value to null
                entity.HasOne(x => x.Company)
.WithMany(x => x.CompanyContacts)
.HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.CompanyId);
            });

            // =========================
            // BRANCH TABLES
            // =========================
            builder.Entity<Branch>(entity =>
            {
                entity.ToTable("Branches");

                entity.HasKey(x => x.BranchId);

                entity.Property(x => x.BranchName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.BranchCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(255);

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(50);

                entity.Property(x => x.PhysicalAddress)
                    .HasMaxLength(500);

                entity.Property(x => x.IsHeadOffice)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null);

                entity.HasIndex(x => x.IsDeleted);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.CreatedAt);

                //updated at by default will be null until the record is updated for the first time, so we set the default value to null

                entity.HasIndex(x => new { x.CompanyId, x.BranchCode })
                    .IsUnique()
                  .HasFilter("[IsDeleted] = 0"); //added this line to ensure that the unique constraint only applies to branches that are not deleted, allowing us to reuse branch codes from deleted branches if necessary.

                entity.HasIndex(x => x.CompanyId);

                entity.HasOne(x => x.Company)
         .WithMany(x => x.Branches)
         .HasForeignKey(x => x.CompanyId)
         .OnDelete(DeleteBehavior.Restrict); 

                entity.HasQueryFilter(x => !x.IsDeleted); //we have to add this filter to ensure that when we query the branches, we only get the ones that are not deleted. This is important for the soft delete functionality to work correctly.
            });
            // =========================
            // DEPARTMENT TABLES
            // =========================

            builder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");

                entity.HasKey(x => x.DepartmentId);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt) 
                    .HasDefaultValue(null);

                entity.HasIndex(x => x.IsDeleted);
                entity.HasIndex(x => x.IsActive);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => new { x.CompanyId, x.Code })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0"); //added this line to ensure that the unique constraint only applies to departments that are not deleted, allowing us to reuse department codes from deleted departments if necessary.



                entity.HasOne(x => x.Company)
.WithMany(x => x.Departments)
.HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasIndex(x => x.CompanyId);
            });

            // =========================
            // BRANCH DEPARTMENT TABLES
            // =========================

            builder.Entity<BranchDepartment>(entity =>
            {
                entity.ToTable("BranchDepartments");

                entity.HasKey(x => x.BranchDepartmentId);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null);

                entity.HasIndex(x => new { x.BranchId, x.DepartmentId })
                    .IsUnique();

                entity.HasOne(x => x.Branch)
                    .WithMany(x => x.BranchDepartments)
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany(x => x.BranchDepartments)
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // DESIGNATION  TABLES
            // =========================

            builder.Entity<Designation>(entity =>
            {
                entity.ToTable("Designations");

                entity.HasKey(x => x.DesignationId);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null); // updated at by default will be null until the record is updated for the first time, so we set the default value to null

                // unique per company
                entity.HasIndex(x => new { x.CompanyId, x.Code })
                    .IsUnique()
                 .HasFilter("[IsDeleted] = 0"); //added this line 

                entity.HasIndex(x => x.CompanyId);

                entity.HasOne(x => x.Company)
.WithMany(x => x.Designations)
.HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict); //updated this also

                entity.HasQueryFilter(x => !x.IsDeleted); // we have to add this filter to ensure that when we query the designations, we only get the ones that are not deleted. This is important for the soft delete functionality to work correctly.
            });

            // =========================
            // EMPLOYEE  TABLES
            // =========================
            builder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");

                entity.HasKey(x => x.EmployeeId);

                entity.Property(x => x.EmployeeCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(100);

                entity.Property(x => x.Gender)
                    .IsRequired();

                entity.Property(x => x.PersonalEmail)
                    .HasMaxLength(255);

                entity.Property(x => x.PersonalPhone)
                    .HasMaxLength(20);

                entity.Property(x => x.CNICOrNationalId)
                    .HasMaxLength(30);

                entity.Property(x => x.ProfileImageUrl)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                   .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null);

                entity.HasIndex(x => x.CompanyId);
                entity.HasIndex(x => new { x.CompanyId, x.EmployeeCode }).IsUnique();

                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Designation)
                    .WithMany()
                    .HasForeignKey(x => x.DesignationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // EMPLOYEE DOCUMENT TYPE  TABLES
            // =========================

            builder.Entity<EmployeeDocumentType>(entity =>
            {
                entity.ToTable("EmployeeDocumentTypes");

                entity.HasKey(x => x.EmployeeDocumentTypeId);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);


                entity.Property(x => x.IsActive)
                  .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt)
                    .HasDefaultValue(null);
                entity.HasIndex(x => x.CompanyId);

                entity.HasIndex(x => new { x.CompanyId, x.Name })
                    .IsUnique();

                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // EMPLOYEE DOCUMENT  TABLES
            // =========================
            builder.Entity<EmployeeDocument>(entity =>
            {
                entity.ToTable("EmployeeDocuments");

                entity.HasKey(x => x.EmployeeDocumentId);

                entity.Property(x => x.FileUrl)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.FileName)
                    .HasMaxLength(255);

                entity.Property(x => x.FileType)
                    .HasMaxLength(50);

                entity.Property(x => x.DocumentTypeNameSnapshot)
                    .HasMaxLength(150);
               
              
                entity.HasIndex(x => x.EmployeeId);
                entity.HasIndex(x => x.EmployeeDocumentTypeId);

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.EmployeeDocumentType)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeDocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // =========================
            // OTHER IDENTITY TABLES
            // =========================

            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.Property(x => x.Id)
                    .HasColumnName("UserClaimId");
            });

            builder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.Property(x => x.Id)
                    .HasColumnName("RoleClaimId");
            });

            // =========================
            // SEED DEFAULT ROLES
            // =========================
            //Every migration becomes "data changed". we fixed that by providing the static guid
            builder.Entity<ApplicationRole>().HasData(

                new ApplicationRole
                {
                    Id = 1,
                    Name = "SystemAdmin",
                    NormalizedName = "SYSTEMADMIN",
                    Description = "Full platform administration access",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                    ConcurrencyStamp = "8f4c6a90-1111-2222-3333-444444444444"
                },

                new ApplicationRole
                {
                    Id = 2,
                    Name = "CompanyOwner",
                    NormalizedName = "COMPANYOWNER",
                    Description = "Company owner with company-level access",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                    ConcurrencyStamp = "8f4c6a21-1111-2222-3333-444444444444"
                },

                new ApplicationRole
                {
                    Id = 3,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "Department and employee management access",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                    ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
                },

                new ApplicationRole
                {
                    Id = 4,
                    Name = "Auditor",
                    NormalizedName = "AUDITOR",
                    Description = "Audit and inspection related access",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                    ConcurrencyStamp = "8f4c6a90-2123-2222-3333-444444444444"
                },

                new ApplicationRole
                {
                    Id = 5,
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Basic system user access",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1),
                    ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
                }
            );
        }



    }
}