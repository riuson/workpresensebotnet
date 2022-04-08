namespace ServerApp.Entities
{
    /// <summary>
    /// Chat where the user was active.
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Gets or sets the record Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the related user.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the related user.
        /// </summary>
        public User User { get; set; } = new ();

        /// <summary>
        /// Gets or sets the actual (in the Telegram) chat Id.
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Gets or sets the status of the user in the chat.
        /// </summary>
        public ChatStatus Status { get; set; } = new ();
    }
}