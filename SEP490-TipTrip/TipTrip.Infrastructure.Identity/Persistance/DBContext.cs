using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TipTrip.Common.Attributes;
using TipTrip.Infrastructure.Identity.Identity.Model;

namespace TipTrip.Infrastructure.Identity.Persistance
{
    internal class DBContext : IdentityDbContext<User>
    {
        public DBContext() { }
        public DBContext(DbContextOptions<DBContext> options)
        : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }

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
                if (!typeof(BaseAttributes).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.Name).Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                    modelBuilder.Entity(entityType.Name).Property<string>("CreatedBy").HasColumnType("nvarchar(max)");
                    modelBuilder.Entity(entityType.Name).Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                    modelBuilder.Entity(entityType.Name).Property<string>("UpdatedBy").HasColumnType("nvarchar(max)");
                }
            }
        }
    }
}
