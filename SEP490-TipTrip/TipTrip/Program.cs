using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TipTrip.Infrastructure.Identity.Persistance;
using TipTrip.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("TipTrip.Infrastructure.Identity")
    ));

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

var app = builder.Build();

//Regis middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger middleware (env Development và Production)
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
