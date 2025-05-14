using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TipTrip.Infrastructure.Identity.Persistance;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
{
    public ApplicationDBContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TipTrip"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();

        var connectionString = config.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString, x =>
        {
            x.MigrationsAssembly(typeof(ApplicationDBContext).Assembly.FullName);
        });

        return new ApplicationDBContext(optionsBuilder.Options);
    }
}
