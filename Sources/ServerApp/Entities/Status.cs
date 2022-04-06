namespace ServerApp.Entities
{
    /// <summary>
    /// Possible user statuses.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Status unknown/undefined.
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
}