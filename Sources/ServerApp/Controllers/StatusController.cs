using Microsoft.AspNetCore.Mvc;
using ServerApp.Entities;

namespace ServerApp.Controllers;

/// <summary>
/// Controller for statuses.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly ILogger<StatusController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusController"/> class.
    /// </summary>
    /// <param name="logger">Logger for <see cref="StatusController"/>.</param>
    public StatusController(
        ILogger<StatusController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Gets collection of statuses.
    /// </summary>
    /// <returns>Collection of forecasts.</returns>
    [HttpGet(Name = "GetStatus")]
    public IEnumerable<Status> Get()
    {
        return new Status[] { Status.CameToWork, Status.LeftWork, Status.StayAtHome, Status.Unknown };
    }
}