﻿using System.Text;
using ServerApp.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
            var chatInfo = await this.telegramBotClient.GetChatAsync(new ChatId(pair.Key), cancellationToken);

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
}