namespace ServerApp.Entities
{
    /// <summary>
    /// Phone number of user.
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Gets a record Id.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Gets or sets a phone number value.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a user's key.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets a user who owns the phone number.
        /// </summary>
        public User? User { get; set; }
    }
}