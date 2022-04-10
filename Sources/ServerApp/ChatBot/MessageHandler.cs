using System.Text;
using System.Text.RegularExpressions;
using ServerApp.Database;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotChat = ServerApp.Entities.Chat;

namespace ServerApp.ChatBot;

/// <summary>
/// Implementation of message handler.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<MessageHandler> logger;
    private readonly IConfiguration configuration;
    private readonly Regex regCommand = new Regex(@"^/\w+$");

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="logger">Logger service.</param>
    /// <param name="configuration">Configuration data.</param>
    public MessageHandler(
        IServiceProvider serviceProvider,
        ILogger<MessageHandler> logger,
        IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.configuration = configuration;
    }

    private string WebHandlersUriBase => this.configuration.GetValue<string>("WebHook:Uri");

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
                        isPrivate,
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
                        isPrivate,
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
                        isPrivate,
                        cancellationToken);
                }

                break;
            }

            case "/web_handlers":
            {
                using (var db = this.serviceProvider.GetService<IDatabase>())
                {
                    var hooks = await db!.GetHooksAsync(
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

                    var keyboardMarkup = await this.FormatHooksKeyboardMarkup(
                        botClient,
                        hooks,
                        cancellationToken);

                    var sentMessage = await botClient.SendTextMessageAsync(
                        receivedMessage.From?.Id ??
                        throw new NullReferenceException("Received message from unknown sender!"),
                        text: "Unique link for each chat and action are listed below 👇",
                        replyMarkup: keyboardMarkup,
                        cancellationToken: cancellationToken);
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

    private async Task<string> FormatStats(
        ITelegramBotClient botClient,
        Dictionary<long, IEnumerable<ChatStatus>> stats,
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
            foreach (var chat in pair.Value.Where(x => x.Status == Status.CameToWork))
            {
                msg.AppendFormat(
                    "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                    chat.User!.Id,
                    chat.User.NickName,
                    chat.User.FirstName,
                    chat.User.LastName);
            }

            msg.AppendLine("At home 🏠");
            foreach (var chat in pair.Value.Where(x => x.Status != Status.CameToWork))
            {
                msg.AppendFormat(
                    "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                    chat.User!.Id,
                    chat.User.NickName,
                    chat.User.FirstName,
                    chat.User.LastName);
            }
        }

        return msg.ToString();
    }

    private async Task<InlineKeyboardMarkup> FormatHooksKeyboardMarkup(
        ITelegramBotClient botClient,
        Dictionary<long, Guid> hooks,
        CancellationToken cancellationToken)
    {
        List<IEnumerable<InlineKeyboardButton>> buttons = new ();

        foreach (var pair in hooks)
        {
            var chatInfo = await botClient.GetChatAsync(new ChatId(pair.Key), cancellationToken);
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithUrl(
                    "Chat: " + chatInfo!.Title!,
                    chatInfo!.InviteLink!),
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithUrl(
                    "Came to work",
                    $"{this.WebHandlersUriBase}/{pair.Value}/came"),
                InlineKeyboardButton.WithUrl(
                    "Left work",
                    $"{this.WebHandlersUriBase}/{pair.Value}/left"),
                InlineKeyboardButton.WithUrl(
                    "Stay at home",
                    $"{this.WebHandlersUriBase}/{pair.Value}/stay"),
            });
        }

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

        return inlineKeyboard;
    }
}