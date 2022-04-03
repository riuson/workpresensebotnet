using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ServerApp.ChatBot;

/// <summary>
/// Implementation of message handler.
/// </summary>
public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger service.</param>
    public MessageHandler(
        ILogger<MessageHandler> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessTextMessage(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var messageText = message.Text;

        this.logger.LogInformation($"Received a '{messageText}' message in chat {chatId}.");

        // Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);
    }
}