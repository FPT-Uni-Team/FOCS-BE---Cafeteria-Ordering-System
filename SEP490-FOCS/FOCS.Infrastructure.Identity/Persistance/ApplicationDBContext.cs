using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity;
using FOCS.Infrastructure.Identity.Identity.Model;

namespace FOCS.Infrastructure.Identity.Persistance
{
    public class ApplicationDBContext : IdentityDbContext<User>
    {
        public ApplicationDBContext() { }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
        {
        }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType != typeof(IdentityRole) &&
                    !typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.Name).Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                    modelBuilder.Entity(entityType.Name).Property<string>("CreatedBy").HasColumnType("nvarchar(max)");
                    modelBuilder.Entity(entityType.Name).Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                    modelBuilder.Entity(entityType.Name).Property<string>("UpdatedBy").HasColumnType("nvarchar(max)");
                }
            }

            modelBuilder.Seed();
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType() != typeof(IdentityRole) &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                var user = "System"; // Replace with actual user ID, e.g., from HttpContext

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                    entry.Property("CreatedBy").CurrentValue = user;
                }

                entry.Property("UpdatedAt").CurrentValue = now;
                entry.Property("UpdatedBy").CurrentValue = user;
            }
        }
    }
}
