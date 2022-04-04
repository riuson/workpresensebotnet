namespace ServerApp.Entities
{
    /// <summary>
    /// Basic User data.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets record Id.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the user in the Telegram.
        /// </summary>
        public long TelegramUserId { get; set; }

        /// <summary>
        /// Gets or sets first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets nickname.
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a user status.
        /// </summary>
        public Status Status { get; set; } = Status.Unknown;

        /// <summary>
        /// Gets or sets a web hook id.
        /// </summary>
        public Guid WebHookId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets a phone numbers of the user.
        /// </summary>
        public virtual List<PhoneNumber>? PhoneNumbers { get; set; }
    }
}