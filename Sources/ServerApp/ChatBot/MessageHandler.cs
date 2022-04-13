using System.Text.RegularExpressions;
using ServerApp.Database;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotChat = ServerApp.Entities.Chat;
using MessageType = ServerApp.Entities.MessageType;

namespace ServerApp.ChatBot;

/// <summary>
/// Implementation of message handler.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MessageHandler> logger;
    private readonly IPinnedMessagesManager pinnedMessagesManager;
    private readonly IDataFormatter dataFormatter;
    private readonly IDatabase database;
    private readonly Regex regCommand = new Regex(@"^/\w+$");
    private readonly IScheduledMessageRemover scheduledMessageRemover;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="logger">Logger service.</param>
    /// <param name="pinnedMessagesManager">Pinned messages manager.</param>
    /// <param name="dataFormatter">Data formatter serv7ice.</param>
    /// <param name="database">Database interface.</param>
    /// <param name="scheduledMessageRemover">Scheduler for removing messages.</param>
    public MessageHandler(
        IServiceProvider serviceProvider,
        ILogger<MessageHandler> logger,
        IPinnedMessagesManager pinnedMessagesManager,
        IDataFormatter dataFormatter,
        IDatabase database,
        IScheduledMessageRemover scheduledMessageRemover)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.pinnedMessagesManager = pinnedMessagesManager;
        this.dataFormatter = dataFormatter;
        this.database = database;
        this.scheduledMessageRemover = scheduledMessageRemover;
    }

    /// <inheritdoc />
    public async Task ProcessTextMessage(
        ITelegramBotClient botClient,
        Message receivedMessage,
        CancellationToken cancellationToken)
    {
        var chatId = receivedMessage.Chat.Id;
        var messageText = receivedMessage.Text ?? string.Empty;

        // Process only commands.
        if (this.regCommand.IsMatch(messageText))
        {
            try
            {
                this.logger.LogInformation(
                    $"Received a command '{messageText}' in chat {chatId} from user {receivedMessage.From?.Id}.");
                await this.ExecuteCommandAsync(
                    botClient,
                    receivedMessage,
                    messageText,
                    cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.LogCritical(exc, "Critical error was occur while processing message!");
            }
        }
    }

    private async Task ExecuteCommandAsync(
        ITelegramBotClient botClient,
        Message receivedMessage,
        string commandText,
        CancellationToken cancellationToken)
    {
        var userId = receivedMessage.From?.Id ?? -1L;

        if (userId < 0)
        {
            return;
        }

        var telegramChat = receivedMessage.Chat;
        var isPrivate = telegramChat.Type == ChatType.Private;

        this.logger.LogInformation($"Received command {commandText} from user {userId} in chat {telegramChat.Id}.");

        switch (commandText)
        {
            case "/came":
            case "/left":
            case "/stay":
            {
                var newStatus = commandText switch
                {
                    "/came" => Status.CameToWork,
                    "/left" => Status.LeftWork,
                    "/stay" => Status.StayAtHome,
                    _ => Status.Unknown,
                };

                var chats = await this.database.UpdateUserStatusAsync(
                    receivedMessage.From!.Id,
                    telegramChat.Id,
                    isPrivate,
                    receivedMessage.From?.Username ?? string.Empty,
                    receivedMessage.From?.FirstName ?? string.Empty,
                    receivedMessage.From?.LastName ?? string.Empty,
                    newStatus,
                    cancellationToken);

                var answerMessage = await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    $"Status updated. 👌",
                    ParseMode.Html,
                    false,
                    isPrivate,
                    cancellationToken);

                if (isPrivate)
                {
                    foreach (var chatId in chats)
                    {
                        var item = this.pinnedMessagesManager.GetChatEvent(chatId);
                        item.Set();
                    }
                }
                else
                {
                    var item = this.pinnedMessagesManager.GetChatEvent(telegramChat.Id);
                    item.Set();

                    await this.scheduledMessageRemover.RemoveAfterAsync(
                        telegramChat.Id,
                        receivedMessage.MessageId,
                        TimeSpan.FromMinutes(5),
                        cancellationToken);

                    await this.scheduledMessageRemover.RemoveAfterAsync(
                        telegramChat.Id,
                        answerMessage.MessageId,
                        TimeSpan.FromMinutes(5),
                        cancellationToken);
                }

                break;
            }

            case "/start":
            {
                if (isPrivate)
                {
                    break;
                }

                var affectedEntities = await this.database.UpdateUserStatusAsync(
                    receivedMessage.From!.Id,
                    telegramChat.Id,
                    isPrivate,
                    receivedMessage.From?.Username ?? string.Empty,
                    receivedMessage.From?.FirstName ?? string.Empty,
                    receivedMessage.From?.LastName ?? string.Empty,
                    Status.Unknown,
                    cancellationToken);

                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    $"Hello!\nChat '{telegramChat.Title}' is registered. 👌",
                    ParseMode.Html,
                    false,
                    isPrivate,
                    cancellationToken);

                break;
            }

            case "/end":
            {
                break;
            }

            case "/stats":
            {
                var chats = await this.database.GetStatsAsync(
                    receivedMessage.From!.Id,
                    telegramChat.Id,
                    isPrivate,
                    cancellationToken);
                var msg = await this.dataFormatter.FormatStats(
                    chats,
                    cancellationToken);

                var message = await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    msg,
                    ParseMode.Html,
                    false,
                    isPrivate,
                    cancellationToken);

                if (!isPrivate)
                {
                    await this.pinnedMessagesManager.NewAsync(
                        telegramChat.Id,
                        message.MessageId,
                        MessageType.Status,
                        cancellationToken);

                    await this.scheduledMessageRemover.RemoveAfterAsync(
                        telegramChat.Id,
                        receivedMessage.MessageId,
                        TimeSpan.FromMinutes(5),
                        cancellationToken);
                }

                break;
            }

            case "/web_handlers":
            {
                var hooks = await this.database.GetHooksAsync(
                    receivedMessage.From!.Id,
                    cancellationToken);

                if (hooks.Count == 0)
                {
                    await this.SendMessageAsync(
                        botClient,
                        receivedMessage,
                        "There are no registered chats for this user!",
                        ParseMode.Html,
                        false,
                        true,
                        cancellationToken);
                    return;
                }

                var keyboardMarkup = await this.dataFormatter.FormatHooksKeyboardMarkup(
                    hooks,
                    cancellationToken);

                var sentMessage = await botClient.SendTextMessageAsync(
                    receivedMessage.From?.Id ??
                    throw new NullReferenceException("Received message from unknown sender!"),
                    text: "Unique link for each chat and action are listed below 👇",
                    replyMarkup: keyboardMarkup,
                    cancellationToken: cancellationToken);

                break;
            }

            default:
            {
                this.logger.LogWarning($"Received an unknown command '{commandText}'!");
                break;
            }
        }
    }

    private async Task<Message> SendMessageAsync(
        ITelegramBotClient botClient,
        Message receivedMessage,
        string content,
        ParseMode parseMode,
        bool asReply,
        bool asPrivate,
        CancellationToken cancellationToken)
    {
        var chatId = receivedMessage.Chat.Id;
        var sentMessage =
            await botClient.SendTextMessageAsync(
                asPrivate
                    ? receivedMessage.From?.Id ??
                      throw new NullReferenceException("Received message from unknown sender!")
                    : chatId,
                text: content,
                parseMode: parseMode,
                replyToMessageId: asReply ? receivedMessage.MessageId : default,
                cancellationToken: cancellationToken);
        return sentMessage;
    }
}