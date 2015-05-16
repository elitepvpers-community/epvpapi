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

        /// <summary> Performs a HTTP request </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="headers"> HTTP Headers that are transmitted with the request </param>
        /// <param name="method"> <c>HttpMethod</c> that is used for the request </param>
        /// <param name="content"> Content to post (if <c>HttpMethod.Post</c> was specified) </param>
        /// <returns> Plain response content </returns>
        public string Request(string url, List<HttpHeader> headers, 
                                HttpMethod method = HttpMethod.Get,
                                HttpContent content = null)
        {
            try
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
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);


                        HttpResponseMessage response;
                        switch (method)
                        {
                            case HttpMethod.Post:
                            {
                                if(content == null) throw new ArgumentException("Content must not be null when using POST as HTTP method");
                                response = client.PostAsync(url, content).Result;
                                break;
                            }
                            case HttpMethod.Get:
                            {
                                response = client.GetAsync(url).Result;
                                break;
                            }
                            default:
                                throw new ArgumentException("This HTTP method is not supported, please use either HttpMethod.POST or HttpMethod.GET");
                        }

                        using (response)
                        {
                            if (!response.IsSuccessStatusCode &&
                                response.StatusCode != HttpStatusCode.SeeOther)
                                throw new RequestFailedException("Request failed, Server returned " + response.StatusCode);

                            return response.Content.ReadAsStringAsync().Result;
                        }
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
        /// <param name="headers"> HTTP Headers that are transmitted with the request </param>
        /// <returns> Plain response content </returns>
        public string Get(string url, List<HttpHeader> headers)
        {
            return Request(url, headers);
        }

        /// <summary> Performs a HTTP GET request with default get headers </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> Plain response content </returns>
        public string Get(string url)
        {
            return Get(url, HttpHeader.CommonHeaders);
        }

        /// <summary> Performs a HTTP POST request </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <param name="headers"> HTTP Headers that are transmitted with the request </param>
        /// <returns> Plain response content </returns>
        public string Post(string url, IEnumerable<KeyValuePair<string, string>> content, List<HttpHeader> headers)
        {
            var encodedContent = new FormUrlEncodedContent(content);
            encodedContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
            encodedContent.Headers.ContentType.CharSet = "UTF-8";
            return Request(url, headers, HttpMethod.Post, encodedContent);
        }

        /// <summary> Performs a HTTP POST request with default post headers </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <returns> Plain response content </returns>
        public string Post(string url, IEnumerable<KeyValuePair<string, string>> content)
        {
            return Post(url, content, HttpHeader.DefaultPostHeaders);
        }

        /// <summary>
        /// Performs a Multipart POST request
        /// </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <returns> Plain response content </returns>
        public string PostMultipartFormData(string url, MultipartFormDataContent content)
        {
            return Request(url, HttpHeader.CommonHeaders, HttpMethod.Post, content);
        }
    }
}
