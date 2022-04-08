﻿namespace ServerApp.Entities;

/// <summary>
/// Basic user info.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user Id in the Telegram, also record Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the nickname.
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a list of registered chats.
    /// </summary>
    public List<Chat> Chats { get; set; } = new ();

    /// <summary>
    /// Gets or sets a phone numbers.
    /// </summary>
    public List<PhoneNumber> Phones { get; set; } = new ();
}