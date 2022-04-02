using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServerApp;

/// <summary>
/// Master class of telegram bot.
/// </summary>
public class TeleBotService : IHostedService
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeleBotService"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    /// <param name="appLifetime">Application's lifetime events.</param>
    public TeleBotService(
        ILogger<TeleBotService> logger,
        IHostApplicationLifetime appLifetime)
    {
        this.logger = logger;

        appLifetime.ApplicationStarted.Register(this.OnStarted);
        appLifetime.ApplicationStopping.Register(this.OnStopping);
        appLifetime.ApplicationStopped.Register(this.OnStopped);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("1. StartAsync has been called.");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("4. StopAsync has been called.");

        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        this.logger.LogInformation("2. OnStarted has been called.");
    }

    private void OnStopping()
    {
        this.logger.LogInformation("3. OnStopping has been called.");
    }

    private void OnStopped()
    {
        this.logger.LogInformation("5. OnStopped has been called.");
    }
}