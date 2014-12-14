using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    /// <summary>
    /// Represents a HTTP header used in all types of HTTP requests
    /// </summary>
    public class HttpHeader
    {
        /// <summary>
        /// Name (or key) of the header, needs to be the exact header name 
        /// or the header might not be accepted by the the HTTP library
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the header
        /// </summary>
        public string Value { get; set; }

        public HttpHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// List of headers that are shared within all types of HTTP requests
        /// </summary>
        public static List<HttpHeader> CommonHeaders
        {
            get
            {
                return new List<HttpHeader>()
                {
                    UserAgent
                };
            }
        }

        /// <summary>
        /// Default headers that are set in every standard HTTP post request on elitepvpers.com
        /// </summary>
        public static List<HttpHeader> DefaultPostHeaders
        {
            get
            {
                var defaultPostHeaders = new List<HttpHeader>()
                {
                    new HttpHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"),
                    new HttpHeader("Accept-Encoding", "gzip,deflate,sdch"),
                    new HttpHeader("Accept-Language", "de-DE,de;q=0.8,en-US;q=0.6,en;q=0.4")
                };

                defaultPostHeaders.AddRange(CommonHeaders);
                return defaultPostHeaders;
            }
        }

        /// <summary>
        /// User-Agent header that contains information about the epvpapi version being used
        /// </summary>
        public static HttpHeader UserAgent
        {
            get
            {
                return new HttpHeader("User-Agent", String.Format("epvpapi - .NET Library v{0}",
                                      FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion));
            }
        }
    }
}
