namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the set of pages that are compatible with Sulakore.HSession[HPages].
    /// </summary>
    public enum HPage
    {
        /// <summary>
        /// Represents http://www.Habbo-/me
        /// </summary>
        Me = 1,
        /// <summary>
        /// Represents http://www.Habbo-/home/-
        /// </summary>
        Home = 2,
        /// <summary>
        /// Represents http://www.Habbo-/client
        /// </summary>
        Client = 3,
        /// <summary>
        /// Represents http://www.Habbo-/profile
        /// </summary>
        Profile = 4,
        /// <summary>
        /// Represents https://www.Habbo-/identity/avatars
        /// </summary>
        IdAvatars = 5,
        /// <summary>
        /// Represents http://www.Habbo-/identity/settings
        /// </summary>
        IdSettings = 6
    }
}