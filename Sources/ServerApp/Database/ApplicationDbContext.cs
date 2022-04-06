using Microsoft.EntityFrameworkCore;
using ServerApp.Entities;

namespace ServerApp.Database
{
    /// <summary>
    /// A DbContext instance what represents a session with the database.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly DbContextOptions<ApplicationDbContext> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">Context options builder.</param>
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            this.options = options;
            this.Users = this.Set<User>();
            ////this.Database.EnsureCreated();
        }

        /// <summary>
        /// Gets or sets a users collection.
        /// </summary>
        public DbSet<User>? Users { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
        }
    }
}