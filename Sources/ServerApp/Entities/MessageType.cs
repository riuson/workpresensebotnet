namespace ServerApp.Entities
{
    /// <summary>
    /// Type of pinned message.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Pinned status message.
        /// </summary>
        Status = 0,

        /// <summary>
        /// Pinned poll message.
        /// </summary>
        Poll = 1,
    }
}