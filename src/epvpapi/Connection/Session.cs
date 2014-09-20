using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    /// <summary>
    ///     Represents a simple websession
    /// </summary>
    public class Session
    {
        /// <summary>
        ///     Cookies used in the <c>Session</c>
        /// </summary>
        protected CookieContainer Cookies { get; set; }

        /// <summary>
        ///     Represents an unique ID identifiying the <c>Session</c> which has to be transmitted during nearly all requests
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        ///     <c>WebProxy</c> for using the Library when behind a Proxy
        /// </summary>
        protected WebProxy Proxy { private get; set; }

        /// <summary>
        ///     If true, the proxy will be used for all requests
        /// </summary>
        protected bool UseProxy { private get; set; }


        /// <summary>
        ///     Looks up the Cookies and tries to figure out wether the session cookie
        ///     is set in order to return if the <c>Session</c> is valid and ready to use
        /// </summary>
        protected bool Valid
        {
            get
            {
                return
                    Cookies.GetCookies(new Uri("http://www.elitepvpers.com/forum/"))
                        .Cast<Cookie>()
                        .Any(cookie => cookie.Name == "bbsessionhash" && !String.IsNullOrEmpty(cookie.Value));
            }
        }


        /// <summary>
        ///     Logs out the session user and destroys the session
        /// </summary>
        public void Destroy()
        {
            ThrowIfInvalid();
            Get("http://www.elitepvpers.com/forum/login.php?do=logout&logouthash=" + SecurityToken);
        }

        /// <summary>
        ///     Updates required session information such as the SecurityToken
        /// </summary>
        protected virtual void Update()
        {
            Response res = Get("http://www.elitepvpers.com/forum/");
            SecurityToken = new Regex("SECURITYTOKEN = \"(\\S+)\";").Match(res.ToString()).Groups[1].Value;
        }


        /// <summary>
        ///     Small wrapper function for throwing an exception if the session is invalid
        /// </summary>
        public virtual void ThrowIfInvalid()
        {
            if (!Valid)
                throw new InvalidSessionException("Session is not valid, Cookies: " + Cookies.Count +
                                                  " | Security Token: " + SecurityToken);
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        private Response Get(Uri url)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = Cookies
                };

                if (UseProxy)
                {
                    handler.UseProxy = true;
                    handler.Proxy = Proxy;
                }

                var client = new HttpClient(handler);

                Task<HttpResponseMessage> response = client.GetAsync(url);
                if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther)
                    throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                return new Response(response.Result);
            }
            catch (CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        public Response Get(string url)
        {
            return Get(new Uri(url));
        }


        /// <summary> Performs a HTTP POST request </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Content to post </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        public Response Post(string url, IEnumerable<KeyValuePair<string, string>> content)
        {
            var URL = new Uri(url);
            try
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = Cookies,
                    AllowAutoRedirect = true
                };

                if (UseProxy)
                {
                    handler.UseProxy = true;
                    handler.Proxy = Proxy;
                }

                var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
                client.DefaultRequestHeaders.Add("Accept-Language", "de-DE,de;q=0.8,en-US;q=0.6,en;q=0.4");
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36");

                var encodedContent = new FormUrlEncodedContent(content);
                encodedContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                encodedContent.Headers.ContentType.CharSet = "UTF-8";

                Task<HttpResponseMessage> response = client.PostAsync(URL, encodedContent);
                if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther &&
                    response.Result.StatusCode != HttpStatusCode.Redirect)
                    throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                return new Response(response.Result);
            }
            catch (CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        /// <summary>
        ///     Performs a Multipart POST request
        /// </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Content to post </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        protected void PostMultipartFormData(Uri url, MultipartFormDataContent content)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = Cookies,
                AllowAutoRedirect = true
            };

            if (UseProxy)
            {
                handler.UseProxy = true;
                handler.Proxy = Proxy;
            }

            var client = new HttpClient(handler);

            Task<HttpResponseMessage> response = client.PostAsync(url, content);
            if (!response.Result.IsSuccessStatusCode)
                throw new RequestFailedException("Server returned " + response.Result.StatusCode);
            new Response(response.Result);
        }
    }
}