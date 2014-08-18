using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        void Delete(Session session, string reason);
    }
}
