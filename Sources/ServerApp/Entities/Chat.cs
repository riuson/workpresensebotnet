namespace ServerApp.Entities
{
    /// <summary>
    /// Chat where the user was active.
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Gets or sets the chat Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the list of user's statuses for that chat.
        /// </summary>
        public List<ChatStatus> Statuses { get; set; } = new ();

        /// <summary>
        /// Gets or sets the list of pinned status messages for that chat.
        /// </summary>
        public List<PinnedMessage> PinnedStatusMessages { get; set; } = new ();
    }
}