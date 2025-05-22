using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FOCS.Application.Mappings;
using FOCS.Application.Services;
using FOCS.Common.Helpers;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Common.UnitOfWorks;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Infrastructure.Identity.Persistance;
using FOCS.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<EmailModels>(builder.Configuration.GetSection("EmailSettings")); // Bind EmailSettings from appsettings.json to EmailModels class

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IEmailHelper, EmailHelper>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<IUnitOfWork, UnitOfWork<ApplicationDBContext>>()
                .AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("TipTrip.Infrastructure.Identity")
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
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
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

var app = builder.Build();

//Regis middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
// CORS
app.UseCors("AllowAll");
// Enable Swagger middleware (env Development and Production)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Own API v1");
        c.DocumentTitle = "My API Documentation";
        c.RoutePrefix = "swagger"; // route: /swagger
        c.DefaultModelsExpandDepth(-1); // Hide Models default if dont need
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
