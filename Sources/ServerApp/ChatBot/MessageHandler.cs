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
    private readonly Regex regCommand = new Regex(@"^/(?<cmd>\w{1,30})");
    private readonly IScheduledMessageRemover scheduledMessageRemover;
    private readonly Dictionary<string, CommandHandler> commandHandlers = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="logger">Logger service.</param>
    /// <param name="pinnedMessagesManager">Pinned messages manager.</param>
    /// <param name="dataFormatter">Data formatter service.</param>
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

        this.commandHandlers.Add("came", this.CommandSetStatus);
        this.commandHandlers.Add("left", this.CommandSetStatus);
        this.commandHandlers.Add("stay", this.CommandSetStatus);
        this.commandHandlers.Add("start", this.CommandStart);
        this.commandHandlers.Add("stats", this.CommandStats);
        this.commandHandlers.Add("web_handlers", this.CommandWebHandlers);
        this.commandHandlers.Add("set_first_name", this.CommandSetName);
        this.commandHandlers.Add("set_last_name", this.CommandSetName);
        this.commandHandlers.Add("set_user_name", this.CommandSetName);
    }

    private delegate Task CommandHandler(CommandHandlerData data);

    /// <inheritdoc />
    public async Task ProcessTextMessage(
        ITelegramBotClient botClient,
        Message receivedMessage,
        CancellationToken cancellationToken)
    {
        var messageText = receivedMessage.Text ?? string.Empty;

        // Process only commands.
        if (this.regCommand.IsMatch(messageText))
        {
            try
            {
                var match = this.regCommand.Match(messageText);
                var commandText = match.Groups["cmd"].Value;
                var commandArgs = messageText
                    .Remove(0, commandText.Length + 1)
                    .Trim();

                if (receivedMessage.From?.Id is null)
                {
                    return;
                }

                var data = new CommandHandlerData(
                    botClient,
                    receivedMessage,
                    commandText,
                    commandArgs,
                    cancellationToken);
                await this.ExecuteCommandAsync(data);
            }
            catch (Exception exc)
            {
                this.logger.LogCritical(exc, "Critical error was occur while processing message!");
            }
        }
    }

    private async Task ExecuteCommandAsync(CommandHandlerData data)
    {
        if (this.commandHandlers.TryGetValue(data.CommandName, out var handler))
        {
            this.logger.LogInformation(
                $"Received command {data.CommandName} from user {data.User.Id} in chat {data.Chat.Id}.");
            var t = handler.Invoke(data);
            await t;
        }
        else
        {
            this.logger.LogWarning($"An unknown command '{data.CommandName}' was received!");
        }
    }

    private async Task CommandSetStatus(CommandHandlerData data)
    {
        var newStatus = data.CommandName switch
        {
            "/came" => Status.CameToWork,
            "/left" => Status.LeftWork,
            "/stay" => Status.StayAtHome,
            _ => Status.Unknown,
        };

        var chats = await this.database.UpdateUserStatusAsync(
            data.User.Id,
            data.Chat.Id,
            data.IsPrivate,
            data.User.Username ?? string.Empty,
            data.User.FirstName ?? string.Empty,
            data.User.LastName ?? string.Empty,
            newStatus,
            data.CancellationToken);

        var answerMessage = await this.SendMessageAsync(
            data.BotClient,
            data.Chat,
            data.User,
            "Status updated. 👌",
            ParseMode.Html,
            data.IsPrivate,
            data.CancellationToken);

        if (data.IsPrivate)
        {
            foreach (var chatId in chats)
            {
                var item = this.pinnedMessagesManager.GetChatEvent(chatId);
                item.Set();
            }
        }
        else
        {
            var item = this.pinnedMessagesManager.GetChatEvent(data.Chat.Id);
            item.Set();

            await this.scheduledMessageRemover.RemoveAfterAsync(
                data.Chat.Id,
                data.Message.MessageId,
                TimeSpan.FromMinutes(5),
                data.CancellationToken);

            await this.scheduledMessageRemover.RemoveAfterAsync(
                data.Chat.Id,
                answerMessage.MessageId,
                TimeSpan.FromMinutes(5),
                data.CancellationToken);
        }
    }

    private async Task CommandStart(CommandHandlerData data)
    {
        if (data.IsPrivate)
        {
            return;
        }

        var affectedEntities = await this.database.UpdateUserStatusAsync(
            data.User.Id,
            data.Chat.Id,
            data.IsPrivate,
            data.User.Username ?? string.Empty,
            data.User.FirstName ?? string.Empty,
            data.User.LastName ?? string.Empty,
            Status.Unknown,
            data.CancellationToken);

        await this.SendMessageAsync(
            data.BotClient,
            data.Chat,
            data.User,
            $"Hello!\nChat '{data.Chat.Title}' is registered. 👌",
            ParseMode.Html,
            data.IsPrivate,
            data.CancellationToken);
    }

    private async Task CommandStats(CommandHandlerData data)
    {
        var chats = await this.database.GetStatsAsync(
            data.User.Id,
            data.Chat.Id,
            data.IsPrivate,
            data.CancellationToken);
        var msg = await this.dataFormatter.FormatStats(
            chats,
            data.CancellationToken);

        var answerMessage = await this.SendMessageAsync(
            data.BotClient,
            data.Chat,
            data.User,
            msg,
            ParseMode.Html,
            false,
            data.CancellationToken);

        if (!data.IsPrivate)
        {
            await this.pinnedMessagesManager.NewAsync(
                data.Chat.Id,
                answerMessage.MessageId,
                MessageType.Status,
                data.CancellationToken);

            await this.scheduledMessageRemover.RemoveAfterAsync(
                data.Chat.Id,
                data.Message.MessageId,
                TimeSpan.FromMinutes(5),
                data.CancellationToken);
        }
    }

    private async Task CommandWebHandlers(CommandHandlerData data)
    {
        var hooks = await this.database.GetHooksAsync(
            data.User.Id,
            data.CancellationToken);

        if (hooks.Count == 0)
        {
            await this.SendMessageAsync(
                data.BotClient,
                data.Chat,
                data.User,
                "There are no registered chats for this user!",
                ParseMode.Html,
                true,
                data.CancellationToken);
            return;
        }

        var keyboardMarkup = await this.dataFormatter.FormatHooksKeyboardMarkup(
            hooks,
            data.CancellationToken);

        var sentMessage = await data.BotClient.SendTextMessageAsync(
            data.User.Id,
            text: "Unique link for each chat and action are listed below 👇",
            replyMarkup: keyboardMarkup,
            cancellationToken: data.CancellationToken);
    }

    private async Task CommandSetName(CommandHandlerData data)
    {
        await this.database.UpdateUserInfoAsync(
            data.User.Id,
            oldValues =>
            {
                var firstName = oldValues.firstName;
                var lastName = oldValues.lastName;
                var nickName = oldValues.nickName;

                switch (data.CommandName)
                {
                    case "set_first_name":
                    {
                        firstName = data.CommandArgs;
                        break;
                    }

                    case "set_last_name":
                    {
                        lastName = data.CommandArgs;
                        break;
                    }

                    case "set_user_name":
                    {
                        nickName = data.CommandArgs;
                        break;
                    }

                    default:
                    {
                        break;
                    }
                }

                return (firstName, lastName, nickName);
            },
            data.CancellationToken);

        var sentMessage = await data.BotClient.SendTextMessageAsync(
            data.User.Id,
            text: "Information was updated.",
            cancellationToken: data.CancellationToken);
    }

    private async Task<Message> SendMessageAsync(
        ITelegramBotClient botClient,
        Telegram.Bot.Types.Chat chat,
        Telegram.Bot.Types.User user,
        string content,
        ParseMode parseMode,
        bool asPrivate,
        CancellationToken cancellationToken)
    {
        var parts = this.SplitMessage(content);
        Message? first = null;

        foreach (var part in parts)
        {
            var sentMessage =
                await botClient.SendTextMessageAsync(
                    asPrivate
                        ? user.Id
                        : chat.Id,
                    text: part,
                    parseMode: parseMode,
                    cancellationToken: cancellationToken);

            if (first is null)
            {
                first = sentMessage;
            }
        }

        return first!;
    }

    private IEnumerable<string> SplitMessage(string value)
    {
        var items = value.Split("\n");
        var result = new List<string>();
        var temp = string.Empty;

        foreach (var item in items)
        {
            if ((temp + "\n" + item).Length < 4000)
            {
                temp += "\n" + item;
            }
            else
            {
                result.Add(temp);
                temp = item;
            }
        }

        if (!string.IsNullOrEmpty(temp))
        {
            result.Add(temp);
        }

        return result;
    }

    private class CommandHandlerData
    {
        public CommandHandlerData(
            Telegram.Bot.ITelegramBotClient botClient,
            Telegram.Bot.Types.Message message,
            string commandName,
            string commandArgs,
            CancellationToken cancellationToken)
        {
            (this.BotClient, this.Message, this.CommandName, this.CommandArgs, this.CancellationToken) =
                (botClient, message, commandName, commandArgs, cancellationToken);
        }

        public ITelegramBotClient BotClient { get; }

        public Telegram.Bot.Types.Message Message { get; }

        public string CommandName { get; }

        public string CommandArgs { get; }

        public CancellationToken CancellationToken { get; }

        public Telegram.Bot.Types.Chat Chat => this.Message.Chat;

        public Telegram.Bot.Types.User User => this.Message.From!;

        public bool IsPrivate => this.Chat.Type == ChatType.Private;
    }
}