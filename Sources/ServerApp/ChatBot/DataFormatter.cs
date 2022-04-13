using System.Text;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotChat = ServerApp.Entities.Chat;

namespace ServerApp.ChatBot;

/// <summary>
/// Formatter of data before sending to users.
/// </summary>
public class DataFormatter : IDataFormatter
{
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFormatter"/> class.
    /// </summary>
    /// <param name="telegramBotClient">Telegram bot client.</param>
    /// <param name="configuration">Configuration data.</param>
    public DataFormatter(
        ITelegramBotClient telegramBotClient,
        IConfiguration configuration)
    {
        this.telegramBotClient = telegramBotClient;
        this.configuration = configuration;
    }

    private string WebHandlersUriBase => this.configuration.GetValue<string>("WebHook:Uri");

    /// <inheritdoc />
    public async Task<string> FormatStats(
        IEnumerable<BotChat> chats,
        CancellationToken cancellationToken)
    {
        if (!chats.Any())
        {
            return "There are no registered chats for this user!";
        }

        var msg = new StringBuilder($"<i>Data as of {DateTime.Now:yyyy-MM-dd HH:mm:ss}</i>\n\n");

        foreach (var chat in chats)
        {
            var chatTitle = await this.GetChatTitle(chat, cancellationToken);

            msg.AppendFormat("*** <b>{0}</b> ***\n", chatTitle);
            msg.AppendLine("At work 🏢");
            foreach (var chatStatus in chat.Statuses.Where(x => x.Status == Status.CameToWork))
            {
                var id = chatStatus.UserId;
                var name = chatStatus.User?.NickName ?? "unknown";
                var time = this.FormatTime(chatStatus.Time);

                if (!string.IsNullOrEmpty(chatStatus.User?.FirstName) ||
                    !string.IsNullOrEmpty(chatStatus.User?.LastName))
                {
                    name = $"{chatStatus.User.FirstName} {chatStatus.User.LastName}".Trim();
                }

                msg.AppendLine($"• <a href=\"tg://user?id={id}\">@{name}</a> <i>{time}</i>");
            }

            msg.AppendLine("At home 🏠");
            foreach (var chatStatus in chat.Statuses.Where(x => x.Status != Status.CameToWork))
            {
                var id = chatStatus.UserId;
                var name = chatStatus.User?.NickName ?? "unknown";
                var time = this.FormatTime(chatStatus.Time);

                if (!string.IsNullOrEmpty(chatStatus.User?.FirstName) ||
                    !string.IsNullOrEmpty(chatStatus.User?.LastName))
                {
                    name = $"{chatStatus.User.FirstName} {chatStatus.User.LastName}".Trim();
                }

                msg.AppendLine($"• <a href=\"tg://user?id={id}\">@{name}</a> <i>{time}</i>");
            }

            msg.AppendLine();
        }

        return msg.ToString();
    }

    /// <inheritdoc />
    public async Task<InlineKeyboardMarkup> FormatHooksKeyboardMarkup(
        Dictionary<long, Guid> hooks,
        CancellationToken cancellationToken)
    {
        List<IEnumerable<InlineKeyboardButton>> buttons = new ();

        foreach (var pair in hooks)
        {
            var chatInfo = await this.telegramBotClient.GetChatAsync(new ChatId(pair.Key), cancellationToken);
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

    private async Task<string> GetChatTitle(BotChat chat, CancellationToken cancellationToken)
    {
        try
        {
            var chatInfo = await this.telegramBotClient.GetChatAsync(new ChatId(chat.Id), cancellationToken);
            return chatInfo.Title ?? "null";
        }
        catch
        {
            return "not found";
        }
    }

    private string FormatTime(DateTime value)
    {
        if (value.Date == DateTime.Now.Date)
        {
            return $"{value:HH:mm:ss}";
        }

        return $"{value:HH:mm:ss dd MMM}";
    }
}