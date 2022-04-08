namespace ServerApp.Entities
{
    /// <summary>
    /// Phone number.
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Gets a record Id.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Gets or sets a user's key.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets an employee who owns the phone number.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets a phone number value.
        /// </summary>
        public string Phone { get; set; } = string.Empty;
    }
}