using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerApp.Entities;

namespace ServerApp.DB2
{
    /// <summary>
    /// A DbContext instance what represents a session with the database.
    /// </summary>
    public class ApplicationContext : DbContext
    {
        private readonly IConfiguration config;
        private readonly ILogger<ApplicationContext> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationContext"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="logger">Logger service.</param>
        public ApplicationContext(
            IConfiguration config,
            ILogger<ApplicationContext> logger)
        {
            this.config = config;
            this.logger = logger;
            ////this.Database.EnsureCreated();
        }

        /// <summary>
        /// Gets or sets a users collection.
        /// </summary>
        public DbSet<User>? Users { get; set; }

        private string DatabasePath => this.config.GetValue<string>("Database:Path2");

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={this.DatabasePath}");
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<PhoneNumber>().ToTable("PhoneNumbers");
        }
    }
}