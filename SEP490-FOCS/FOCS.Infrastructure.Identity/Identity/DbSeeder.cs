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
                    Id = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpper(),
                    ConcurrencyStamp = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                }
            );

            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "cf0a1168-7764-4a16-8e7e-f69c834ba3fe",
                    Name = Roles.Customer,
                    NormalizedName = Roles.Customer.ToUpper(),
                    ConcurrencyStamp = "cf0a1168-7764-4a16-8e7e-f69c834ba3fe",
                }
            );
                
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
                    Id = "5961f48a-d700-4807-bc3d-4fa9d229e9fb",
                    Name = Roles.KitchenStaff,
                    NormalizedName = Roles.KitchenStaff.ToUpper(),
                    ConcurrencyStamp = "5961f48a-d700-4807-bc3d-4fa9d229e9fb",
                }
            );
        }
    }
}
