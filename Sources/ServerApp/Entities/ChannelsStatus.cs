namespace ServerApp.Entities;

/// <summary>
///  Chat's status info.
/// </summary>
public class ChatStatus
{
    /// <summary>
    /// Gets or sets a record Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the related chat Id.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Gets or sets related chat.
    /// </summary>
    public Chat? Chat { get; set; }

    /// <summary>
    /// Gets or sets status for related chat.
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// Gets or sets the time of last update.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets unique Id for using in web hooks.
    /// </summary>
    public Guid HookId { get; set; }
}