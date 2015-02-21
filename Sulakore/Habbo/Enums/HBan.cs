namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the different types of bans found in-game.
    /// </summary>
    public enum HBan
    {
        /// <summary>
        /// The original value for the packet is RWUAM_BAN_USER_DAY.
        /// </summary>
        Hour = 0,
        /// <summary>
        /// The original value for the packet is RWUAM_BAN_USER_HOUR.
        /// </summary>
        Day = 1,
        /// <summary>
        /// The original value for the packet is RWUAM_BAN_USER_PERM.
        /// </summary>
        Permanent = 2
    }
}