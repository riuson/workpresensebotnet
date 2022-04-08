using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ServerApp.Database
{
    /// <summary>
    /// DB Context factory for migrations.
    /// </summary>
    public class ApplicationDBDesignTimeContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <inheritdoc />
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            builder.AddJsonFile("appsettings.Development.json");
            var config = builder.Build();

            var connectionString = config.GetConnectionString("DataFile2");
            Debug.WriteLine($"ConnStr: {connectionString}");
            optionsBuilder.UseSqlite(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}