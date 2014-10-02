using epvpapi.Connection;

namespace epvpapi
{
    /// <summary>
    /// Interface for objects that are deletable requiring a reason for the deletion
    /// </summary>
    public interface IReasonableDeletable
    {
        /// <summary>
        /// Deletes the object using the given <c>Session</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        void Delete<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User;
    }
}
