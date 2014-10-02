using epvpapi.Connection;

namespace epvpapi
{
    /// <summary>
    /// Interface for objects that are deletable
    /// </summary>
    interface IDeletable
    {
        /// <summary>
        /// Deletes the object using the given <c>Session</c>
        /// </summary>
        /// <param name="authenticatedSession"> Session that is used for sending the request </param>
        void Delete<TUser>(AuthenticatedSession<TUser> authenticatedSession) where TUser : User; 
    }
}
