using FOCS.Application.Mappings;
using FOCS.Application.Services;
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
                .AddScoped<IRepository<UserRefreshToken>, Repository<UserRefreshToken, ApplicationDBContext>>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<IOrderService, OrderService>()
                .AddScoped<IKitchenService, KitchenService>()
                .AddScoped<IMenuService, MenuService>()
                .AddScoped<IAdminService, AdminService>()
                .AddScoped<IUnitOfWork, UnitOfWork<ApplicationDBContext>>()
                .AddScoped<IRepository<Order>, Repository<Order, OrderDbContext>>()
                .AddScoped<IRepository<MenuItem>, Repository<MenuItem, OrderDbContext>>();
builder.Services.AddHostedService<OrderBatchingService>();

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

//SignalR
builder.Services.AddSignalR();

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
        Type = SecuritySchemeType.ApiKey,
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


// CORS
builder.Services.AddCors(options =>
{
    // Allow specific origins
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    // Allow all origins
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication();

var app = builder.Build();

// Add Serilog request logging middleware
app.UseSerilogRequestLogging();
//Regis middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
// CORS
app.UseCors("AllowAll");

//SignalR
app.MapHub<OrderHub>("/orders");

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

    app.UseExceptionHandler("/Home/Error");
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
