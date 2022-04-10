using ServerApp.Database;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using MessageType = ServerApp.Entities.MessageType;

namespace ServerApp.ChatBot;

/// <summary>
/// Manager of pinned messages.
/// </summary>
public class PinnedMessagesManager : IPinnedMessagesManager
{
    private readonly ILogger<PinnedMessagesManager> logger;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IDatabase database;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinnedMessagesManager"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="telegramBotClient">Telegram bot client.</param>
    /// <param name="database">Database.</param>
    public PinnedMessagesManager(
        ILogger<PinnedMessagesManager> logger,
        ITelegramBotClient telegramBotClient,
        IDatabase database)
    {
        this.logger = logger;
        this.telegramBotClient = telegramBotClient;
        this.database = database;
    }

    /// <inheritdoc />
    public async Task NewAsync(
        long chatId,
        long messageId,
        MessageType messageType,
        CancellationToken cancellationToken)
    {
        var (success, previousMessageId, _) = await this.database.GetPinnedMessageAsync(
            chatId,
            messageType,
            cancellationToken);

        if (success)
        {
            try
            {
                await this.telegramBotClient.UnpinChatMessageAsync(
                    chatId,
                    (int)previousMessageId,
                    cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.LogWarning(
                    exc,
                    $"Problem occur while unpin message {messageId} in chat {chatId}.");
            }
        }

        try
        {
            await this.telegramBotClient.PinChatMessageAsync(
                chatId,
                (int)messageId,
                disableNotification: true,
                cancellationToken: cancellationToken);

            await this.database.UpdatePinnedMessageAsync(
                chatId,
                (int)messageId,
                messageType,
                DateTime.Now,
                cancellationToken);
        }
        catch (Exception exc)
        {
            this.logger.LogWarning(
                exc,
                $"Problem occur while pin message {messageId} in chat {chatId}.");
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
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
                await this.telegramBotClient.EditMessageTextAsync(
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