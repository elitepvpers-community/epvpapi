using epvpapi.Connection;

namespace epvpapi
{
    /// <summary>
    /// Interface implemented by objects that can be reported
    /// </summary>
    interface IReportable
    {
        /// <summary>
        /// Reports the object 
        /// </summary>
        /// <param name="authenticatedSession"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason of the report </param>
        void Report<TUser>(AuthenticatedSession<TUser> authenticatedSession, string reason) where TUser : User;
    }
}
