using FOCS.Application.Mappings;
using FOCS.Application.Services;
using FOCS.Application.Services.ApplyStrategy;
using FOCS.Application.Services.BackgroundServices;
using FOCS.Application.Services.Interface;
using FOCS.Common.Helpers;
using FOCS.Common.Interfaces;
using FOCS.Common.Interfaces.Focs.Application.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Common.UnitOfWorks;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Infrastructure.Identity.Persistance;
using FOCS.Middlewares;
using FOCS.NotificationService.Consumers;
using FOCS.NotificationService.Services;
using FOCS.Order.Infrastucture.Context;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using FOCS.Order.Infrastucture.Migrations;
using FOCS.Realtime.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

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
                .AddScoped<ICustomerService, CustomerService>()
                .AddScoped<IOrderService, OrderService>()
                .AddScoped<IKitchenService, KitchenService>()
                .AddScoped<IMenuService, MenuService>()
                .AddScoped<IAdminMenuItemService, AdminMenuItemService>()
                .AddScoped<IAdminBrandService, AdminBrandService>()
                .AddScoped<IMenuItemManagementService, MenuItemManagementService>()
                .AddScoped<IRealtimeService, RealtimeService>()
                .AddScoped<IMenuItemsVariantGroupItemService, MenuItemsVariantGroupItemService>()
                .AddScoped<IMenuItemsVariantGroupService, MenuItemsVariantGroupService>()
                .AddScoped<IAdminStoreService, AdminStoreService>()
                .AddScoped<IAdminCouponService, AdminCouponService>()
                .AddScoped<IStoreSettingService, StoreSettingService>()
                .AddScoped<IPromotionService, PromotionService>()
                .AddScoped<IVariantGroupService, VariantGroupService>()
                .AddScoped<IRepository<VariantGroup>, Repository<VariantGroup, OrderDbContext>>()
                .AddScoped<IRepository<OrderWrap>, Repository<OrderWrap, OrderDbContext>>()
                .AddScoped<ITableService, TableService>()
                .AddScoped<IOrderWrapService, OrderWrapService>()
                .AddScoped<IUnitOfWork, UnitOfWork<ApplicationDBContext>>()
                .AddScoped<IMenuItemVariantService, MenuItemVariantService>()
                .AddScoped<IRepository<OrderEntity>, Repository<OrderEntity, OrderDbContext>>()
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
                .AddScoped<IRepository<Feedback>, Repository<Feedback, OrderDbContext>>()
                .AddScoped<IFeedbackService, FeedbackService>()
                .AddScoped<ICategoryService, CategoryService>()
                .AddScoped<ICouponService, CouponService>()
                .AddScoped<IPricingService, PricingService>()
                .AddScoped<IPayOSServiceFactory, PayOSServiceFactory>()
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
                .AddSingleton<FirebaseService>()
                .AddScoped<IMenuInsightService, MenuInsightService>()
                .AddScoped<IRepository<MobileTokenDevice>,  Repository<MobileTokenDevice, OrderDbContext>>()
                .AddScoped<IMobileTokenSevice, MobileTokenSevice>()
                .AddScoped<ICashierService, CashierService>()
                .AddScoped<IRepository<CartItem>, Repository<CartItem, OrderDbContext>>()
                .AddScoped<IRepository<CouponUsage>, Repository<CouponUsage, OrderDbContext>>()
                .AddScoped<INotifyService, NotifyService>()
                .AddScoped<IRepository<Coupon>, Repository<Coupon, OrderDbContext>>()
                .AddScoped<IRepository<PaymentAccount>, Repository<PaymentAccount, OrderDbContext>>()
                .AddScoped<IRepository<Promotion>, Repository<Promotion, OrderDbContext>>()
                .AddScoped<IRepository<UserStore>, Repository<UserStore, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemCategories>, Repository<MenuItemCategories, OrderDbContext>>()
                .AddScoped<IRepository<Table>, Repository<Table, OrderDbContext>>()
                .AddScoped<IAdminMenuItemService, AdminMenuItemService>()
                .AddScoped<IWorkshiftScheduleService, WorkshiftScheduleService>()
                .AddScoped<IRepository<MenuItemVariantGroupItem>, Repository<MenuItemVariantGroupItem, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemVariantGroup>, Repository<MenuItemVariantGroup, OrderDbContext>>()
                .AddScoped<IMenuItemCategoryService, MenuItemCategoryService>()
                .AddScoped<IRepository<MenuItemVariantGroup>, Repository<MenuItemVariantGroup, OrderDbContext>>()
                .AddScoped<IRepository<MenuItemImage>, Repository<MenuItemImage, OrderDbContext>>()
                .AddScoped<IRepository<Workshift>, Repository<Workshift, OrderDbContext>>()
                .AddScoped<IRepository<WorkshiftSchedule>, Repository<WorkshiftSchedule, OrderDbContext>>()
                .AddScoped<IRepository<StaffWorkshiftRegistration>, Repository<StaffWorkshiftRegistration, OrderDbContext>>()
                .AddScoped<IRepository<SystemConfiguration>, Repository<SystemConfiguration, OrderDbContext>>()
                .AddScoped<ICouponUsageService, CouponUsageService>()
                .AddScoped<IDashboardService, DashboardService>()
                .AddSingleton<ICloudinaryService, CloudinaryService>()
                .AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddSingleton<OtpService>();
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddSingleton<SmsService>();

builder.Services.AddHostedService<OrderBatchingService>();
builder.Services.AddHostedService<ScanToDeleteCartItem>();
//builder.Services.AddHostedService<CartFlushBackgroundService>();
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));


builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
            sqlOptions.MigrationsAssembly("FOCS.Infrastructure.Identity");
        }
    ));

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
            sqlOptions.MigrationsAssembly("FOCS.Order.Infrastucture");
        }
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

var options = new ConfigurationOptions
{
    EndPoints = { "103.185.184.27:6379" },
    Password = "Hxs03122003",
    AbortOnConnectFail = false,
    ConnectRetry = 5,
    ConnectTimeout = 15000,   
    SyncTimeout = 15000
};

var redis = ConnectionMultiplexer.Connect(options);

builder.Services.AddSignalR()
    .AddStackExchangeRedis("103.185.184.27:6379,password=Hxs03122003,allowAdmin=true,abortConnect=false,connectTimeout=15000");

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("SEP490FOCS")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(3650));


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
        policy.WithOrigins("http://127.0.0.1:5500",
                           "http://127.0.0.1:3000",
                           "https://adminfocssite.vercel.app",
                           "https://focs-site.vercel.app",
                           "http://localhost:3000",
                           "https://localhost:3000",
                           "https://focs.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotifyConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var configuration = ctx.GetRequiredService<IConfiguration>();
        var rabbitMqSection = configuration.GetSection("RabbitMq");

        cfg.Host(rabbitMqSection["Host"], ushort.Parse(rabbitMqSection["Port"]), "/", h =>
        {
            h.Username(rabbitMqSection["Username"]);
            h.Password(rabbitMqSection["Password"]);
            h.Heartbeat(TimeSpan.FromSeconds(60));
        });

        cfg.ReceiveEndpoint("notify-consumer", e =>
        {
            e.ConfigureConsumer<NotifyConsumer>(ctx);
        });
    });
});

var multiplexer = ConnectionMultiplexer.Connect(options);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddAuthentication();

var app = builder.Build();

// Add Serilog request logging middleware
app.UseSerilogRequestLogging();
//Regis middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
// CORS
app.UseCors("AllowFrontend");

//SignalR
app.MapHub<OrderHub>("/hubs/order");
app.MapHub<CartHub>("/hubs/cart");
app.MapHub<NotifyHub>("/hubs/notify");

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
