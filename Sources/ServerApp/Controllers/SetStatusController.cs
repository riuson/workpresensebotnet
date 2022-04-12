using Microsoft.AspNetCore.Mvc;
using ServerApp.ChatBot;
using ServerApp.Database;
using ServerApp.Entities;

namespace ServerApp.Controllers;

/// <summary>
/// Controller for statuses.
/// </summary>
[ApiController]
[Route("/")]
public class SetStatusController : ControllerBase
{
    private readonly ILogger<SetStatusController> logger;
    private readonly IDatabase database;
    private readonly IPinnedMessagesManager pinnedMessagesManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetStatusController"/> class.
    /// </summary>
    /// <param name="logger">Logger for <see cref="SetStatusController"/>.</param>
    /// <param name="database">Interface to access persistent data.</param>
    /// <param name="pinnedMessagesManager">Pinned messages manager.</param>
    public SetStatusController(
        ILogger<SetStatusController> logger,
        IDatabase database,
        IPinnedMessagesManager pinnedMessagesManager)
    {
        this.logger = logger;
        this.database = database;
        this.pinnedMessagesManager = pinnedMessagesManager;
    }

    /// <summary>
    /// Gets collection of statuses.
    /// </summary>
    /// <param name="hookId">Unique for user Id of web hook.</param>
    /// <param name="status">New status.</param>
    /// <returns>Collection of forecasts.</returns>
    [HttpGet("{hookId:guid}/{status}", Name = "SetStatus")]
    public async Task<IActionResult> Get(Guid hookId, string status)
    {
        var newStatus = Status.Unknown;

        switch (status)
        {
            case "came":
            {
                newStatus = Status.CameToWork;
                break;
            }

            case "left":
            {
                newStatus = Status.LeftWork;
                break;
            }

            case "stay":
            {
                newStatus = Status.StayAtHome;
                break;
            }

            default:
            {
                return this.NotFound("Failed! Specified url was not found.");
            }
        }

        var (isSuccessfull, chatId, previousStatus, time) = await this.database.UpdateUserStatusAsync(
            hookId,
            newStatus,
            CancellationToken.None);

        if (!isSuccessfull)
        {
            return this.NotFound("Failed! Specified Id is not exists.");
        }

        this.pinnedMessagesManager.MarkChat(chatId);

        if (previousStatus != newStatus)
        {
            return this.Ok($"Success! Status updated from {previousStatus} to {newStatus} at {time}.");
        }

        return this.Ok($"Success! Status updated to {newStatus} at {time}.");
    }
}