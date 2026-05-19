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

                // Username
                entity.Property(x => x.UserName)
                    .HasColumnName("Username")
                    .HasMaxLength(50)
                    .IsRequired();

                // Email
                entity.Property(x => x.Email)
                    .HasMaxLength(255)
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

                // Unique Constraints
                entity.HasIndex(x => x.UserName)
                    .IsUnique();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.HasIndex(x => x.PhoneNumber)
                    .IsUnique();

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

            builder.Entity<ApplicationUserRole>(entity =>
            {
                // Primary Key
                entity.HasKey(x => x.UserRoleId);

                entity.Property(x => x.UserRoleId)
                    .ValueGeneratedOnAdd();

                // CreatedAt
                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Composite Unique Constraint
                entity.HasIndex(x => new { x.UserId, x.RoleId })
                    .IsUnique();

                // Relationships
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
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

            builder.Entity<ApplicationRole>().HasData(

                new ApplicationRole
                {
                    Id = 1,
                    Name = "SystemAdmin",
                    NormalizedName = "SYSTEMADMIN",
                    Description = "Full platform administration access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },

                new ApplicationRole
                {
                    Id = 2,
                    Name = "CompanyOwner",
                    NormalizedName = "COMPANYOWNER",
                    Description = "Company owner with company-level access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },

                new ApplicationRole
                {
                    Id = 3,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "Department and employee management access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },

                new ApplicationRole
                {
                    Id = 4,
                    Name = "Auditor",
                    NormalizedName = "AUDITOR",
                    Description = "Audit and inspection related access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },

                new ApplicationRole
                {
                    Id = 5,
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Basic system user access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );
        }



    }
}