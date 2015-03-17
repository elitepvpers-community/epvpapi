using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace epvpapi.Connection
{
    public class GuestSession
    {
        /// <summary>
        /// Container containing the cookies used in the current session
        /// </summary>
        public CookieContainer Cookies { get; protected set; }

        /// <summary>
        /// Proxy for issuing requests when behind a proxy
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// If set to true, the proxy will be used for all requests
        /// </summary>
        public bool UseProxy { get; set; }

        public GuestSession(WebProxy proxy) :
            this()
        {
            UseProxy = true;
            Proxy = proxy;
        }

        public GuestSession()
        {
            Cookies = new CookieContainer();
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <param name="headers"> HTTP Headers that are transmitted with the request </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Get(string url, List<HttpHeader> headers)
        {
            try
            {
                using (var handler = new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = Cookies,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
                {
                    if (UseProxy)
                    {
                        handler.UseProxy = true;
                        handler.Proxy = Proxy;
                    }

                    using (var client = new HttpClient(handler))
                    {
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);

                        var response = client.GetAsync(url);
                        if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther)
                            throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                        return new Response(response.Result);
                    }
                }

            }
            catch (CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Get(string url)
        {
            return Get(url, HttpHeader.CommonHeaders);
        }

        /// <summary> Performs a HTTP POST request </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <param name="headers"> HTTP Headers that are transmitted with the request </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Post(string url, IEnumerable<KeyValuePair<string, string>> content, List<HttpHeader> headers)
        {
            try
            {
                var targetUrl = new Uri(url);

                using (var handler = new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = Cookies,
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
                {
                    if (UseProxy)
                    {
                        handler.UseProxy = true;
                        handler.Proxy = Proxy;
                    }

                    using (var client = new HttpClient(handler))
                    {
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);

                        var encodedContent = new FormUrlEncodedContent(content);
                        encodedContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                        encodedContent.Headers.ContentType.CharSet = "UTF-8";

                        var response = client.PostAsync(targetUrl, encodedContent);
                        if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther && response.Result.StatusCode != HttpStatusCode.Redirect)
                            throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                        return new Response(response.Result);
                    }
                }
            }
            catch (CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        internal Response Post(string url, IEnumerable<KeyValuePair<string, string>> content)
        {
            return Post(url, content, HttpHeader.DefaultPostHeaders);
        }

        /// <summary>
        /// Performs a Multipart POST request
        /// </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <returns> Server <c>Response</c> to the sent request  </returns>
        internal Response PostMultipartFormData(Uri url, MultipartFormDataContent content)
        {
            using (var handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = Cookies,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                if (UseProxy)
                {
                    handler.UseProxy = true;
                    handler.Proxy = Proxy;
                }

                using (var client = new HttpClient(handler))
                {
                    var response = client.PostAsync(url, content);
                    if (!response.Result.IsSuccessStatusCode)
                        throw new RequestFailedException("Server returned " + response.Result.StatusCode);

                    return new Response(response.Result);
                }
            }
        }
    }
}
