using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    public class Session
    {
        /// <summary>
        /// Cookies used in the <c>Session</c>
        /// </summary>
        public CookieContainer Cookies { get; private set; }

        /// <summary>
        /// Represents an unique ID identifiying the <c>Session</c> which has to be transmitted during nearly all requests
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// <c>User</c> which created the session, logged-in <c>User</c>
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// <c>WebProxy</c> for using the Library when behind a Proxy
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// If true, the proxy will be used for all requests
        /// </summary>
        public bool UseProxy { get; set; }


        /// <summary>
        /// Looks up the Cookies and tries to figure out wether the session cookie 
        /// is set in order to return if the <c>Session</c> is valid and ready to use
        /// </summary>
        public bool Valid
        {
            get
            {
                foreach (Cookie cookie in Cookies.GetCookies(new Uri("http://www.elitepvpers.com/forum/")))
                    if(cookie.Name == "bbsessionhash" && !String.IsNullOrEmpty(cookie.Value))
                        return true;

                return false;
            }
        }

        public Session(User user, string md5Password):
            this(user)
        {
            Login(md5Password);
        }

        public Session(User user)
        {
            Cookies = new CookieContainer();
            User = user;
        }

        public Session(User user, WebProxy proxy)
        {
            Cookies = new CookieContainer();
            User = user;
            Proxy = proxy;
            UseProxy = true;
        }

        /// <summary>
        /// Logs in the session user
        /// </summary>
        /// <param name="md5Password"> Hashed password (MD5) of the session user </param>
        public void Login(string md5Password)
        {
            Response res = Post("http://www.elitepvpers.com/forum/login.php?do=login",
                                        new List<KeyValuePair<string, string>>()
                                        {
                                            new KeyValuePair<string, string>("vb_login_username", User.Name),
                                            new KeyValuePair<string, string>("cookieuser", "1"),
                                            new KeyValuePair<string, string>("s", String.Empty),
                                            new KeyValuePair<string, string>("securitytoken", "guest"),
                                            new KeyValuePair<string, string>("do", "login"),
                                            new KeyValuePair<string, string>("vb_login_md5password", md5Password),
                                            new KeyValuePair<string, string>("vb_login_md5password_utf", md5Password)
                                        });

            Update();
            if (String.IsNullOrEmpty(SecurityToken))
                throw new InvalidCredentialsException("Credentials entered for user " + User.Name + " were invalid");
        }


        /// <summary>
        /// Logs out the session user and destroys the session
        /// </summary>
        public void Logout()
        {
            ThrowIfInvalid();
            Get("http://www.elitepvpers.com/forum/login.php?do=logout&logouthash=" + SecurityToken);
        }

        /// <summary>
        /// Updates required session information such as the SecurityToken
        /// </summary>
        public void Update()
        {
            Response res = Get("http://www.elitepvpers.com/forum/");
            SecurityToken = new Regex("SECURITYTOKEN = \"(\\S+)\";").Match(res.ToString()).Groups[1].Value;

            // Update the user associated with the session
            // User.Update(this);
        }

        /// <summary>
        /// Small wrapper function for throwing an exception if the session is invalid
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (!Valid) throw new InvalidSessionException("Session is not valid, Cookies: " + Cookies.Count +
                                                          " | Security Token: " + SecurityToken +
                                                          " | User: " + User.Name);
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        public Response Get(Uri url)
        {
            try
            {
                HttpClientHandler Handler = new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = Cookies
                };

                if(UseProxy)
                {
                    Handler.UseProxy = true;
                    Handler.Proxy = Proxy;
                }

                HttpClient Client = new HttpClient(Handler);
                
                Task<HttpResponseMessage> response = Client.GetAsync(url);
                if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther)
                    throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                return new Response(response.Result);
            }
            catch (System.Net.CookieException exception)
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
        /// <param name="referer"> Location of the referer </param>
        /// <returns> <c>Response</c> associated to the Request sent </returns>
        public Response Post(string url, List<KeyValuePair<string, string>> content)
        {
            try
            {
                Uri URL = new Uri(url);

                HttpClientHandler Handler = new HttpClientHandler()
                {
                    UseCookies = true,
                    CookieContainer = Cookies,
                    AllowAutoRedirect = true
                };

                if (UseProxy)
                {
                    Handler.UseProxy = true;
                    Handler.Proxy = Proxy;
                }

                HttpClient Client = new HttpClient(Handler);
                Client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
                Client.DefaultRequestHeaders.Add("Accept-Language", "de-DE,de;q=0.8,en-US;q=0.6,en;q=0.4");
                Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36");

                FormUrlEncodedContent EncodedContent = new FormUrlEncodedContent(content);
                EncodedContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                EncodedContent.Headers.ContentType.CharSet = "UTF-8";

                Task<HttpResponseMessage> response = Client.PostAsync(URL, EncodedContent);
                if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther && response.Result.StatusCode != HttpStatusCode.Redirect)
                    throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                return new Response(response.Result);
            }
            catch (System.Net.CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }
    }
}