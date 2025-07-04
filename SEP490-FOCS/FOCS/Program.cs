﻿using FOCS.Application.Mappings;
using FOCS.Application.Services;
using FOCS.Application.Services.ApplyStrategy;
using FOCS.Application.Services.Interface;
using FOCS.Common.Helpers;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Common.UnitOfWorks;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Infrastructure.Identity.Persistance;
using FOCS.Middlewares;
using FOCS.Order.Infrastucture.Context;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Config Serilog for logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<EmailModels>(builder.Configuration.GetSection("EmailSettings")); // Bind EmailSettings from appsettings.json to EmailModels class
builder.Services.Configure<OrderBatchingOptions>(builder.Configuration.GetSection("OrderBatchingOptions"));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IEmailHelper, EmailHelper>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IUserProfileService, UserProfileService>()
                .AddScoped<IRepository<UserRefreshToken>, Repository<UserRefreshToken, ApplicationDBContext>>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<IStaffService, StaffService>()
                .AddScoped<IOrderService, OrderService>()
                .AddScoped<IKitchenService, KitchenService>()
                .AddScoped<IMenuService, MenuService>()
                .AddScoped<IAdminMenuItemService, AdminMenuItemService>()
                .AddScoped<IAdminBrandService, AdminBrandService>()
                .AddScoped<IMenuItemManagementService, MenuItemManagementService>()
                .AddScoped<IMenuItemsVariantGroupItemService, MenuItemsVariantGroupItemService>()
                .AddScoped<IMenuItemsVariantGroupService, MenuItemsVariantGroupService>()
                .AddScoped<IAdminStoreService, AdminStoreService>()
                .AddScoped<IAdminCouponService, AdminCouponService>()
                .AddScoped<IStoreSettingService, StoreSettingService>()
                .AddScoped<IPromotionService, PromotionService>()
                .AddScoped<IVariantGroupService, VariantGroupService>()
                .AddScoped<IRepository<VariantGroup>, Repository<VariantGroup, OrderDbContext>>()
                .AddScoped<ITableService, TableService>()
                .AddScoped<IUnitOfWork, UnitOfWork<ApplicationDBContext>>()
                .AddScoped<IMenuItemVariantService, MenuItemVariantService>()
                .AddScoped<IRepository<Order>, Repository<Order, OrderDbContext>>()
                .AddScoped<IRepository<MenuItem>, Repository<MenuItem, OrderDbContext>>()
                .AddScoped<IRepository<Brand>, Repository<Brand, OrderDbContext>>()
                .AddScoped<IRepository<Store>, Repository<Store, OrderDbContext>>()
                .AddScoped<IRepository<Coupon>, Repository<Coupon, OrderDbContext>>()
                .AddScoped<IRepository<StoreSetting>, Repository<StoreSetting, OrderDbContext>>()
                .AddScoped<IRepository<Table>, Repository<Table, OrderDbContext>>()
                .AddScoped<IRepository<Promotion>, Repository<Promotion, OrderDbContext>>()
                .AddScoped<IRepository<PromotionItemCondition>, Repository<PromotionItemCondition, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemVariant>, Repository<MenuItemVariant, OrderDbContext>>()
                .AddScoped<IRepository<Category>, Repository<Category, OrderDbContext>>()
                .AddScoped<ICategoryService, CategoryService>()
                .AddScoped<ICouponService, CouponService>()
                .AddScoped<IPricingService, PricingService>()
                .AddScoped<IRepository<PromotionItemCondition>, Repository<PromotionItemCondition, OrderDbContext>>()
                .AddScoped<IRepository<OrderDetail>, Repository<OrderDetail, OrderDbContext>>()
                .AddScoped<DiscountContext>()
                .AddScoped<CouponOnlyStrategy>()
                .AddScoped<PromotionOnlyStrategy>()
                .AddScoped<CouponThenPromotionStrategy>()
                .AddScoped<MaxDiscountOnlyStrategy>()
                .AddScoped<IDiscountStrategyService, CouponOnlyStrategy>()
                .AddScoped<IDiscountStrategyService, PromotionOnlyStrategy>()
                .AddScoped<ICartService, CartService>()
                .AddScoped<IRepository<CartItem>, Repository<CartItem, OrderDbContext>>()
                .AddScoped<IRepository<CouponUsage>, Repository<CouponUsage, OrderDbContext>>()
                .AddScoped<IRepository<Coupon>, Repository<Coupon, OrderDbContext>>()
                .AddScoped<IRepository<Promotion>, Repository<Promotion, OrderDbContext>>()
                .AddScoped<IRepository<UserStore>, Repository<UserStore, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemCategories>, Repository<MenuItemCategories, OrderDbContext>>()
                .AddScoped<IRepository<Table>, Repository<Table, OrderDbContext>>()
                .AddScoped<IAdminMenuItemService, AdminMenuItemService>()
                .AddScoped<IRepository<MenuItemVariantGroupItem>, Repository<MenuItemVariantGroupItem, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemVariantGroup>, Repository<MenuItemVariantGroup, OrderDbContext>>()
                .AddScoped<IMenuItemCategoryService, MenuItemCategoryService>()
                .AddScoped<IRepository<MenuItemVariantGroup>, Repository<MenuItemVariantGroup, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemImage>, Repository<MenuItemImage, OrderDbContext>>()
                .AddSingleton<ICloudinaryService, CloudinaryService>()
                .AddSingleton<IRedisCacheService>(sp => new RedisCacheService("localhost:6379"));
;
//builder.Services.AddHostedService<OrderBatchingService>();
//builder.Services.AddHostedService<CartFlushBackgroundService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));


builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("FOCS.Infrastructure.Identity")
    ));
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("FOCS.Order.Infrastucture")
    ));

//auto mapper
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);


builder.Services.AddSwaggerGen(options =>
{
    // config API
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Own API",
        Version = "v1",
        Description = "API for managing authentication, users, and data operations.",
        Contact = new OpenApiContact
        {
            Name = "SEP490: Capstone project",
            Email = "hson512475@gmail.com",
            Url = new Uri("https://yourwebsite.com")
        }
    });

    // JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token format: **Bearer {token}**\n\nExample: `Bearer eyJhbGciOiJIUzI1...`"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.MaxFailedAccessAttempts = 4;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication();

var app = builder.Build();

// Add Serilog request logging middleware
app.UseSerilogRequestLogging();
//Regis middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
// CORS
app.UseCors("AllowFrontend");

//SignalR
app.MapHub<OrderHub>("/orderHubs");
app.MapHub<CartHub>("/cartHubs");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger(); // Swagger on Production
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
