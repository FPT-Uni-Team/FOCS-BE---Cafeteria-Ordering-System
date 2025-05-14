using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TipTrip.Common.Constants;

namespace TipTrip.Infrastructure.Identity.Identity
{
    public static class DbSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // Get current datetime for the audit fields
            var now = DateTime.UtcNow;
            var systemUser = "System";

            // Admin role
            var adminRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = adminRoleId,
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    // Audit fields
                    CreatedAt = now,
                    CreatedBy = systemUser,
                    UpdatedAt = now,
                    UpdatedBy = systemUser
                }
            );

            // Customer role
            var customerRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = customerRoleId,
                    Name = Roles.Customer,
                    NormalizedName = Roles.Customer.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    // Audit fields
                    CreatedAt = now,
                    CreatedBy = systemUser,
                    UpdatedAt = now,
                    UpdatedBy = systemUser
                }
            );
        }
    }
}
