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
            // Admin role
            var adminRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpper(),
                    ConcurrencyStamp = "6a25d16f-9e41-431a-86e3-5e7e1620746b",
                }
            );

            // Customer role
            var customerRoleId = Guid.NewGuid().ToString();
            modelBuilder.Entity<IdentityRole>().HasData(
                new
                {
                    Id = "cf0a1168-7764-4a16-8e7e-f69c834ba3fe",
                    Name = Roles.Customer,
                    NormalizedName = Roles.Customer.ToUpper(),
                    ConcurrencyStamp = "cf0a1168-7764-4a16-8e7e-f69c834ba3fe",
                }
            );
        }
    }
}
