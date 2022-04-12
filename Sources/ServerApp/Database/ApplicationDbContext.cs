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
            this.Phones = this.Set<PhoneNumber>();
            this.Chats = this.Set<Chat>();
            this.Statuses = this.Set<ChatStatus>();
            this.PinnedMessages = this.Set<PinnedMessage>();
            this.Database.EnsureCreated();
        }

        /// <summary>
        /// Gets or sets a users collection.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets a phone numbers collection.
        /// </summary>
        public DbSet<PhoneNumber> Phones { get; set; }

        /// <summary>
        /// Gets or sets a chats collection.
        /// </summary>
        public DbSet<Chat> Chats { get; set; }

        /// <summary>
        /// Gets or sets a statuses collection.
        /// </summary>
        public DbSet<ChatStatus> Statuses { get; set; }

        /// <summary>
        /// Gets or sets a pinned status messages collection.
        /// </summary>
        public DbSet<PinnedMessage> PinnedMessages { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
            modelBuilder.ApplyConfiguration(new ChatConfiguration());
            modelBuilder.ApplyConfiguration(new ChatStatusConfiguration());
            modelBuilder.ApplyConfiguration(new PinnedMessageConfiguration());
        }
    }
}