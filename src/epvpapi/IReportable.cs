using epvpapi.Connection;

namespace epvpapi
{
    /// <summary>
    ///     Interface implemented by objects that can be reported
    /// </summary>
    internal interface IReportable
    {
        /// <summary>
        ///     Reports the object
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason of the report </param>
        void Report(Session session, string reason);
    }
}