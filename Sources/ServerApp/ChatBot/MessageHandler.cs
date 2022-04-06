using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerApp.Database;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = ServerApp.Entities.User;

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

        switch (commandText)
        {
            case "/came":
            case "/left":
            case "/stay":
            {
                var status = commandText switch
                {
                    "/came" => Status.CameToWork,
                    "/left" => Status.LeftWork,
                    "/stay" => Status.StayAtHome,
                    _ => Status.Unknown,
                };

                using (var db = this.serviceProvider.GetService<IDatabase>())
                {
                    var user = await db!.Context!.Users!.FirstOrDefaultAsync(
                        x => x.UserId == userId,
                        cancellationToken);

                    if (user is null)
                    {
                        user = new User()
                        {
                            UserId = userId,
                            FirstName = receivedMessage.From?.FirstName ?? string.Empty,
                            LastName = receivedMessage.From?.LastName ?? string.Empty,
                            NickName = receivedMessage.From?.Username ?? string.Empty,
                            WebHookId = Guid.NewGuid(),
                        };
                        db!.Context!.Users!.Add(user!);
                    }

                    user.Status = status;
                    user.StatusTime = DateTime.Now;

                    await db!.Context.SaveChangesAsync(cancellationToken);
                }

                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    "Status changed 👌",
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
                chatId: chatId,
                text: content,
                parseMode: parseMode,
                replyToMessageId: asReply ? receivedMessage.MessageId : default,
                cancellationToken: cancellationToken);
        return sentMessage;
    }
}