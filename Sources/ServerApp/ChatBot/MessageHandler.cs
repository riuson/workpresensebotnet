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
            this.logger.LogInformation(
                $"Received a command '{messageText}' in chat {chatId} from user {receivedMessage.From?.Id}.");
            await this.ExecuteCommandAsync(
                botClient,
                receivedMessage,
                messageText,
                cancellationToken);
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
                    ParseMode.MarkdownV2,
                    true,
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
}