using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Infrastructure.Identity.Persistance;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext() { }

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
        {
        }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<Category> MenuCategories { get; set; }
        public DbSet<MenuItemCategories> MenuItemCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemImage> MenuItemImages { get; set; }
        public DbSet<MenuItemVariant> MenuItemVariants { get; set; }
        public DbSet<VariantGroup> VariantGroups { get; set; }
        public DbSet<MenuItemVariantGroup> MenuItemVariantGroups { get; set; }
        public DbSet<MenuItemVariantGroupItem> MenuItemVariantGroupItems { get; set; }

        public DbSet<Table> Tables { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<FOCS.Order.Infrastucture.Entities.Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderWrap> OrderWraps { get; set; }

        public DbSet<UserStore> UserStores { get; set; }

        public DbSet<WorkshiftSchedule> Workshifts { get; set; }
        public DbSet<Workshift> WorkshiftSchedules { get; set; }
        public DbSet<StaffWorkshiftRegistration> StaffWorkshiftRegistrations { get; set; }


        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionItemCondition> PromotionItemConditions { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreSetting> StoreSettings { get; set; }

        public DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        public DbSet<MobileTokenDevice> MobileTokenDevices { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>()
                 .HasOne(mi => mi.Store)
                 .WithMany(s => s.MenuItems)
                 .HasForeignKey(mi => mi.StoreId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PromotionItemCondition>()
                .HasOne(pic => pic.BuyItem)
                .WithMany()
                .HasForeignKey(pic => pic.BuyItemId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<PromotionItemCondition>()
                .HasOne(pic => pic.GetItem)
                .WithMany()
                .HasForeignKey(pic => pic.GetItemId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<PromotionItemCondition>()
                .HasOne(pic => pic.Promotion)
                .WithMany(p => p.PromotionItemConditions)
                .HasForeignKey(pic => pic.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                  .HasOne(c => c.MenuItem)
                  .WithMany()
                  .HasForeignKey(c => c.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Variant)
                .WithMany()
                .HasForeignKey(c => c.VariantId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Table)
                .WithMany()
                .HasForeignKey(c => c.TableId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
