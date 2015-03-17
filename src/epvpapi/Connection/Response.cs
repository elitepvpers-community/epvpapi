using System.Net.Http;

namespace epvpapi.Connection
{
    /// <summary> Stores information about the result of a HTTP request </summary>
    public class Response
    {
        /// <summary>
        /// Response content of the associated request
        /// </summary>
        public string Content { get; set; }

        public Response(string content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}
