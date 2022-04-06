using Microsoft.Extensions.DependencyInjection;

namespace ServerApp.Database;

/// <summary>
/// DB Context helper.
/// </summary>
public class Database : IDatabase
{
    private readonly IServiceScope scope;
    private readonly ApplicationDbContext context;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Database"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public Database(IServiceProvider serviceProvider)
    {
        this.scope = serviceProvider?.CreateScope() ?? throw new ArgumentNullException(nameof(serviceProvider));
        var context = this.scope.ServiceProvider.GetService<ApplicationDbContext>();
        this.context = context ?? throw new NullReferenceException("Resolved null DBContext!");
    }

    /// <inheritdoc />
    public ApplicationDbContext Context => this.context;

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);

        // Use SupressFinalize in case a subclass
        // of this type implements a finalizer.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Internal dispose method.
    /// </summary>
    /// <param name="disposing">Flag of completed disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                // Clear all property values that maybe have been set
                // when the class was instantiated
                this.context.Dispose();
                this.scope.Dispose();
            }

            // Indicate that the instance has been disposed.
            this.disposed = true;
        }
    }
}