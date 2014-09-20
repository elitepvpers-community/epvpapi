using System.Net.Http;

namespace epvpapi.Connection
{
    /// <summary> <c>System.Net.Http</c> wrapper for easier access to a HTTP response returned by HTTP requests </summary>
    public class Response
    {
        /// <param name="msg"> <c>HttpResponseMessage</c> returned by a <c>System.Net.Http</c> request</param>
        public Response(HttpResponseMessage msg)
        {
            Message = msg;
        }

        /// <summary> Headers and information about the request </summary>
        private HttpResponseMessage Message { get; set; }

        /// <summary> Reading the response content from the <c>HttpResponseMessage</c> </summary>
        /// <returns> Pain response content as string </returns>
        public override string ToString()
        {
            return Message.Content.ReadAsStringAsync().Result;
        }
    }
}