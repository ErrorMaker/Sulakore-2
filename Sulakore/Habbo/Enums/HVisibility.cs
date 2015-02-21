namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the different types of choices given on the page represented by Sulakore.HPages.Profile.
    /// </summary>
    public enum HVisibility
    {
        Unknown = 0,
        /// <summary>
        /// Represents a value in-which the object being referenced has zero visibility to the public.
        /// </summary>
        Nobody = 1,
        /// <summary>
        /// Represents a value in-which the object being referenced allows the public's view.
        /// </summary>
        Everybody = 2,
        /// <summary>
        /// Represents a value in-which the object being referenced only allows to been seen by friends.
        /// </summary>
        MyFriends = 3
    }
}