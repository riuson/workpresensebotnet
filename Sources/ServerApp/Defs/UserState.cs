namespace ServerApp.Defs;

/// <summary>
/// Supported user's states.
/// </summary>
public enum UserState
{
    /// <summary>
    /// State unknown/undefined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// User stay at home.
    /// </summary>
    StayAtHome = 1,

    /// <summary>
    /// User came to work.
    /// </summary>
    CameToWork = 2,

    /// <summary>
    /// User left work.
    /// </summary>
    LeftWork = 3,
}