using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ServerApp.ChatBot;

/// <summary>
/// Master class of telegram bot.
/// </summary>
public class TeleBotService : BackgroundService
{
    private readonly ILogger logger;
    private readonly IHostApplicationLifetime appLifetime;
    private readonly IMessageHandler messageHandler;
    private readonly ITelegramBotClient telegramBotClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeleBotService"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    /// <param name="appLifetime">Application's lifetime events.</param>
    /// <param name="telegramBotClient">Telegram bot client's instance.</param>
    /// <param name="messageHandler">Message handler service.</param>
    public TeleBotService(
        ILogger<TeleBotService> logger,
        IHostApplicationLifetime appLifetime,
        ITelegramBotClient telegramBotClient,
        IMessageHandler messageHandler)
    {
        this.logger = logger;
        this.appLifetime = appLifetime;
        this.messageHandler = messageHandler;
        this.telegramBotClient = telegramBotClient;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogInformation($"Bot is starting.");

            await this.VerifyToken(cancellationToken);
            await this.StartBot(cancellationToken);

            this.logger.LogInformation($"Bot was started.");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception exc)
        {
            this.logger.LogCritical(exc, "A critical error was occur!");
            this.appLifetime.StopApplication();
        }
        finally
        {
            this.logger.LogInformation($"Bot is stopping.");
        }
    }

    private async Task VerifyToken(CancellationToken cancellationToken)
    {
        var isTokenOk = await this.telegramBotClient.TestApiAsync(cancellationToken);

        if (isTokenOk)
        {
            this.logger.LogInformation("A token was verified with success.");
        }
        else
        {
            throw new Exception("A problem was found when verifying the token!");
        }
    }

    private Task StartBot(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message },
            };

            this.telegramBotClient.StartReceiving(
                this.HandleUpdateAsync,
                this.HandleErrorAsync,
                receiverOptions,
                cancellationToken);
        });
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message when update.Message!.Type == MessageType.Text:
            {
                await this.messageHandler.ProcessTextMessage(botClient, update.Message, cancellationToken);
                break;
            }

            default:
            {
                break;
            }
        }
    }

    private Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        this.logger.LogCritical(exception, errorMessage);
        return Task.CompletedTask;
    }
}