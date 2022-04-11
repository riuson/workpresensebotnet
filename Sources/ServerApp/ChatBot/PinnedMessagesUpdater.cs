using ServerApp.Database;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using MessageType = ServerApp.Entities.MessageType;

namespace ServerApp.ChatBot;

/// <summary>
/// Updater of pinned messages.
/// </summary>
public class PinnedMessagesUpdater : BackgroundService
{
    private readonly ILogger<PinnedMessagesUpdater> logger;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IDatabase database;
    private readonly IDataFormatter dataFormatter;
    private readonly IPinnedMessagesManager pinnedMessagesManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinnedMessagesUpdater"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="telegramBotClient">Telegram bot client.</param>
    /// <param name="database">Database.</param>
    /// <param name="dataFormatter">Data formatter serv7ice.</param>
    /// <param name="pinnedMessagesManager">Manager of pinned messages.</param>
    public PinnedMessagesUpdater(
        ILogger<PinnedMessagesUpdater> logger,
        ITelegramBotClient telegramBotClient,
        IDatabase database,
        IDataFormatter dataFormatter,
        IPinnedMessagesManager pinnedMessagesManager)
    {
        this.logger = logger;
        this.telegramBotClient = telegramBotClient;
        this.database = database;
        this.dataFormatter = dataFormatter;
        this.pinnedMessagesManager = pinnedMessagesManager;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogInformation($"Pinned messages service is starting.");

            // Until service shutdown.
            while (!cancellationToken.IsCancellationRequested)
            {
                // Get sync event for chat.
                var items = this.pinnedMessagesManager.GetChatEvents();

                foreach (var item in items)
                {
                    // If item is marked as updated.
                    if (item.mre.IsSet)
                    {
                        var (isSuccessfull, messageId, time) = await this.database.GetPinnedMessageAsync(
                            item.chatId,
                            MessageType.Status,
                            cancellationToken);
                        if (isSuccessfull)
                        {
                            try
                            {
                                var stats = await this.database.GetStatsAsync(
                                    userId: 0,
                                    chatId: item.chatId,
                                    isPrivateChat: false,
                                    cancellationToken);
                                var msg = await this.dataFormatter.FormatStats(
                                    stats,
                                    cancellationToken);
                                await this.UpdateAsync(
                                    item.chatId,
                                    msg,
                                    MessageType.Status,
                                    cancellationToken);
                            }
                            catch (Exception exc)
                            {
                                this.logger.LogError(exc, "Problem occur while updating pinned message.");
                            }
                            finally
                            {
                                item.mre.Reset();
                            }
                        }
                    }
                }

                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception exc)
        {
            this.logger.LogCritical(exc, "A critical error was occur!");
        }
        finally
        {
            this.logger.LogInformation($"Pinned messages service is stopping.");
        }
    }

    private async Task UpdateAsync(
        long chatId,
        string text,
        MessageType messageType,
        CancellationToken cancellationToken)
    {
        var (success, previousMessageId, time) = await this.database.GetPinnedMessageAsync(
            chatId,
            messageType,
            cancellationToken);

        if (success)
        {
            try
            {
                var message = await this.telegramBotClient.EditMessageTextAsync(
                    chatId,
                    (int)previousMessageId,
                    text,
                    ParseMode.Html,
                    cancellationToken: cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.LogWarning(
                    exc,
                    $"Problem occur while updating message {previousMessageId} in chat {chatId}.");
            }
        }
    }
}