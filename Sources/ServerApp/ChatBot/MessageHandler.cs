using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerApp.Database;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotChat = ServerApp.Entities.Chat;
using BotUser = ServerApp.Entities.User;
using Chat = Telegram.Bot.Types.Chat;
using User = Telegram.Bot.Types.User;

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

                var affectedEntities = await this.UpdateUserStatus(
                    receivedMessage.From!,
                    telegramChat,
                    isPrivate,
                    newStatus,
                    cancellationToken);

                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    $"Updated entities: {affectedEntities} 👌",
                    ParseMode.Html,
                    false,
                    cancellationToken);
                break;
            }

            case "/start":
            {
                if (isPrivate)
                {
                    break;
                }

                var affectedEntities = await this.UpdateUserStatus(
                    receivedMessage.From!,
                    telegramChat,
                    isPrivate,
                    Status.Unknown,
                    cancellationToken);

                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    $"Hello!\nChat '{telegramChat.Title}' is registered. \nUpdated entities: {affectedEntities} 👌",
                    ParseMode.Html,
                    false,
                    cancellationToken);
                break;
            }

            case "/end":
            {
                break;
            }

            case "/stats":
            {
                var msg = await this.GetStats(
                    botClient,
                    receivedMessage.From!,
                    telegramChat,
                    isPrivate,
                    cancellationToken);

                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    msg,
                    ParseMode.Html,
                    false,
                    cancellationToken);
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

    private async Task<int> UpdateUserStatus(
        Telegram.Bot.Types.User telegramUser,
        Chat telegramChat,
        bool isPrivate,
        Status newStatus,
        CancellationToken cancellationToken)
    {
        using (var db = this.serviceProvider.GetService<IDatabase>())
        {
            if (db is null)
            {
                throw new NullReferenceException("IDatabase resolved as null!");
            }

            var user = await db.Context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == telegramUser.Id,
                    cancellationToken: cancellationToken);

            if (user is null)
            {
                user = new BotUser()
                {
                    Id = telegramUser.Id,
                    FirstName = telegramUser.FirstName ?? string.Empty,
                    LastName = telegramUser.LastName ?? string.Empty,
                    NickName = telegramUser.Username ?? string.Empty,
                };
                db.Context.Users.Add(user!);
            }
            else
            {
                await db.Context.Entry(user).Collection(x => x.Chats).LoadAsync();
            }

            if (isPrivate)
            {
                // Update all registered chats for user.
                foreach (var chat in user.Chats)
                {
                    await db.Context.Entry(chat).Reference(x => x.Status).LoadAsync();
                    chat.Status.Status = newStatus;
                    chat.Status.Time = DateTime.Now;
                }
            }
            else
            {
                // Update current chat.
                var chat = user.Chats.FirstOrDefault(x => x.ChatId == telegramChat.Id);

                if (chat is null)
                {
                    chat = new BotChat()
                    {
                        User = user,
                        ChatId = telegramChat.Id,
                    };
                    chat.Status.Chat = chat;
                    chat.Status.HookId = Guid.NewGuid();
                    chat.Status.Status = newStatus;
                    chat.Status.Time = DateTime.Now;
                    db.Context.Chats.Add(chat);
                }
                else
                {
                    await db.Context.Entry(chat).Reference(x => x.Status).LoadAsync();
                    chat.Status.Status = newStatus;
                    chat.Status.Time = DateTime.Now;
                }
            }

            var affectedEntities = await db!.Context.SaveChangesAsync(cancellationToken);
            return affectedEntities;
        }
    }

    private async Task<string> GetStats(
        ITelegramBotClient botClient,
        User telegramUser,
        Chat telegramChat,
        bool isPrivate,
        CancellationToken cancellationToken)
    {
        using (var db = this.serviceProvider.GetService<IDatabase>())
        {
            if (db is null)
            {
                throw new NullReferenceException("IDatabase resolved as null!");
            }

            var user = await db.Context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == telegramUser.Id,
                    cancellationToken: cancellationToken);

            if (user is null)
            {
                return "There are no registered chats for this user!";
            }

            await db.Context.Entry(user).Collection(x => x.Chats).LoadAsync();
            var chats = isPrivate
                ? user.Chats.ToArray()
                : user.Chats.Where(x => x.ChatId == telegramChat.Id).ToArray();

            if (chats.Length == 0)
            {
                return "There are no registered chats for this user!";
            }

            var msg = new StringBuilder();

            foreach (var chat in chats)
            {
                var chatInfoTask = botClient.GetChatAsync(new ChatId(chat.ChatId), cancellationToken);

                var sameChats = await db.Context.Chats
                    .Where(x => x.ChatId == chat.ChatId)
                    .Include(x => x.User)
                    .Include(x => x.Status)
                    .ToListAsync(cancellationToken);

                var chatInfo = await chatInfoTask;

                msg.AppendFormat("Chat: <b>{0}</b>\n", chatInfo.Title);
                msg.AppendLine("At work 🏢");
                foreach (var sameChat in sameChats.Where(x => x.Status.Status == Status.CameToWork))
                {
                    msg.AppendFormat(
                        "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                        sameChat.User.Id,
                        sameChat.User.NickName,
                        sameChat.User.FirstName,
                        sameChat.User.LastName);
                }

                msg.AppendLine("At home 🏠");
                foreach (var sameChat in sameChats.Where(x => x.Status.Status != Status.CameToWork))
                {
                    msg.AppendFormat(
                        "• <a href=\"tg://user?id={0}\">@{1} {2} {3}</a>\n",
                        sameChat.User.Id,
                        sameChat.User.NickName,
                        sameChat.User.FirstName,
                        sameChat.User.LastName);
                }
            }

            return msg.ToString();
        }
    }
}