using System.Text;
using System.Text.RegularExpressions;
using ServerApp.Database;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotChat = ServerApp.Entities.Chat;

namespace ServerApp.ChatBot;

/// <summary>
/// Implementation of message handler.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MessageHandler> logger;
    private readonly Regex regCommand = new Regex(@"^/\w+$");

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="logger">Logger service.</param>
    public MessageHandler(
        IServiceProvider serviceProvider,
        ILogger<MessageHandler> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
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
                this.logger.LogCritical("Critical error was occur while processing message!", exc);
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

                using (var db = this.serviceProvider.GetService<IDatabase>())
                {
                    var affectedEntities = await db!.UpdateUserStatusAsync(
                        receivedMessage.From!.Id,
                        telegramChat.Id,
                        isPrivate,
                        receivedMessage.From?.Username ?? string.Empty,
                        receivedMessage.From?.FirstName ?? string.Empty,
                        receivedMessage.From?.LastName ?? string.Empty,
                        newStatus,
                        cancellationToken);

                    await this.SendMessageAsync(
                        botClient,
                        receivedMessage,
                        $"Updated entities: {affectedEntities} 👌",
                        ParseMode.Html,
                        false,
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

                using (var db = this.serviceProvider.GetService<IDatabase>())
                {
                    var affectedEntities = await db!.UpdateUserStatusAsync(
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
                        $"Hello!\nChat '{telegramChat.Title}' is registered. \nUpdated entities: {affectedEntities} 👌",
                        ParseMode.Html,
                        false,
                        cancellationToken);
                }

                break;
            }

            case "/end":
            {
                break;
            }

            case "/stats":
            {
                using (var db = this.serviceProvider.GetService<IDatabase>())
                {
                    var stats = await db!.GetStatsAsync(
                        receivedMessage.From!.Id,
                        telegramChat.Id,
                        isPrivate,
                        cancellationToken);
                    var msg = await this.FormatStats(
                        botClient,
                        stats,
                        cancellationToken);

                    await this.SendMessageAsync(
                        botClient,
                        receivedMessage,
                        msg,
                        ParseMode.Html,
                        false,
                        cancellationToken);
                }

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
        CancellationToken cancellationToken)
    {
        var chatId = receivedMessage.Chat.Id;
        var sentMessage =
            await botClient.SendTextMessageAsync(
                receivedMessage.From?.Id ?? throw new NullReferenceException("Received message from unknown sender!"),
                text: content,
                parseMode: parseMode,
                replyToMessageId: asReply ? receivedMessage.MessageId : default,
                cancellationToken: cancellationToken);
        return sentMessage;
    }

    private async Task<string> FormatStats(
        ITelegramBotClient botClient,
        Dictionary<long, IEnumerable<BotChat>> stats,
        CancellationToken cancellationToken)
    {
        if (stats.Count == 0)
        {
            return "There are no registered chats for this user!";
        }

        var msg = new StringBuilder();

        foreach (var pair in stats)
        {
            var chatInfo = await botClient.GetChatAsync(new ChatId(pair.Key), cancellationToken);

            msg.AppendFormat("Chat: <b>{0}</b>\n", chatInfo.Title);
            msg.AppendLine("At work 🏢");
            foreach (var chat in pair.Value.Where(x => x.Status.Status == Status.CameToWork))
            {
                msg.AppendFormat(
                    "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                    chat.User.Id,
                    chat.User.NickName,
                    chat.User.FirstName,
                    chat.User.LastName);
            }

            msg.AppendLine("At home 🏠");
            foreach (var chat in pair.Value.Where(x => x.Status.Status != Status.CameToWork))
            {
                msg.AppendFormat(
                    "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                    chat.User.Id,
                    chat.User.NickName,
                    chat.User.FirstName,
                    chat.User.LastName);
            }
        }

        return msg.ToString();
    }
}