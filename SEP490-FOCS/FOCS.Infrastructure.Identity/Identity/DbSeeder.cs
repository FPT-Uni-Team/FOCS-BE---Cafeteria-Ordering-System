using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FOCS.Infrastructure.Identity.Identity
{
    public static class DbSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "123dc5a4-2ce6-4497-9c3a-f614752c28fa",
                    Name = Roles.User,
                    NormalizedName = Roles.User.ToUpper(),
                    ConcurrencyStamp = "123dc5a4-2ce6-4497-9c3a-f614752c28fa",
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "bc722046-068e-47ca-b280-c9088e3ebc1f",
                    Name = Roles.Staff,
                    NormalizedName = Roles.Staff.ToUpper(),
                    ConcurrencyStamp = "bc722046-068e-47ca-b280-c9088e3ebc1f",
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "5961f48a-d700-4807-bc3d-4fa9d229e9fb",
                    Name = Roles.KitchenStaff,
                    NormalizedName = Roles.KitchenStaff.ToUpper(),
                    ConcurrencyStamp = "5961f48a-d700-4807-bc3d-4fa9d229e9fb",
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "35e0de77-c3a0-4a6e-ae26-d7fa0721ed62",
                    Name = Roles.Manager,
                    NormalizedName = Roles.Manager.ToUpper(),
                    ConcurrencyStamp = "35e0de77-c3a0-4a6e-ae26-d7fa0721ed62",
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpper(),
                    ConcurrencyStamp = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                }
            );
        }
    }
}
