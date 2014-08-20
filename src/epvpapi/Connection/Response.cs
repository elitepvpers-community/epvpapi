using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    /// <summary> <c>System.Net.Http</c> wrapper for easier access to a HTTP response returned by HTTP requests </summary>
    public class Response
    {
        /// <summary> Headers and information about the request </summary>
        public HttpResponseMessage Message { get; set; }

        /// <param name="msg"> <c>HttpResponseMessage</c> returned by a <c>System.Net.Http</c> request</param>
        public Response(HttpResponseMessage msg)
        {
            Message = msg;
        }

        /// <summary> Reading the response content from the <c>HttpResponseMessage</c> </summary>
        /// <returns> Pain response content as string </returns>
        public override string ToString()
        {
            return Message.Content.ReadAsStringAsync().Result;
        }
    }
}
