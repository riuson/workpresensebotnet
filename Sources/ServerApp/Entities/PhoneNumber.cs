using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Entities
{
    /// <summary>
    /// Phone number of user.
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Gets or sets a record Id.
        /// </summary>
        public long PhoneNumberId { get; set; }

        /// <summary>
        /// Gets or sets a user who owns the phone number.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Gets or sets a phone number value.
        /// </summary>
        public string Number { get; set; } = string.Empty;
    }
}