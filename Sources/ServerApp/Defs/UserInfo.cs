namespace ServerApp.Defs;

/// <summary>
/// Information about user.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets User's Id from Telegram.
    /// </summary>
    public long UserId { get; set; } = 0;

    /// <summary>
    /// Gets or sets User's state.
    /// </summary>
    public UserState State { get; set; } = UserState.Unknown;

    /// <summary>
    /// Gets or sets last time when <see cref="State"/> was changed.
    /// </summary>
    public DateTime StateTimeStamp { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets User's Phone Number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets unique Id for web hooks.
    /// </summary>
    public Guid WebHookId { get; set; } = Guid.Empty;
}