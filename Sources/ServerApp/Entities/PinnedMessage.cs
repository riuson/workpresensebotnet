namespace ServerApp.Entities
{
    /// <summary>
    /// Pinned message in a chat.
    /// </summary>
    public class PinnedMessage
    {
        /// <summary>
        /// Gets or sets the record Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets Id of the chat where message is pinned.
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Gets or sets the chat where message is pinned.
        /// </summary>
        public Chat? Chat { get; set; }

        /// <summary>
        /// Gets or sets a pinned message's Id.
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// Gets or sets a pinned message type.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets last time of pinned message update.
        /// </summary>
        public DateTime Time { get; set; }
    }
}