using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ServerApp.DB;
using ServerApp.Defs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ServerApp.ChatBot;

/// <summary>
/// Implementation of message handler.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> logger;
    private readonly IDatabase database;
    private readonly Regex regCommand = new Regex(@"^/\w+$");

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    /// <param name="database">Database.</param>
    public MessageHandler(
        ILogger<MessageHandler> logger,
        IDatabase database)
    {
        this.logger = logger;
        this.database = database;
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
                var state = commandText switch
                {
                    "/came" => UserState.CameToWork,
                    "/left" => UserState.LeftWork,
                    "/stay" => UserState.StayAtHome,
                    _ => UserState.Unknown,
                };
                await this.database.UpdateUserState(
                    userId,
                    state,
                    cancellationToken);
                await this.SendMessageAsync(
                    botClient,
                    receivedMessage,
                    "State changed 👌",
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