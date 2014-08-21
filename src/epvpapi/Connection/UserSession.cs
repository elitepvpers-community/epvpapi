using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    public class UserSession<TUser> : Session where TUser : User
    {
        /// <summary>
        /// Logged-in <c>User</c>, associated with the <c>Session</c>
        /// </summary>
        public TUser User { get; private set; }

        public UserSession(TUser user, string md5Password):
            this(user)
        {
            Login(md5Password);
        }

        public UserSession(TUser user)
        {
            Cookies = new CookieContainer();
            User = user;
        }

        public UserSession(TUser user, WebProxy proxy)
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

        public override void Update()
        {
            base.Update();

            // Update the user associated with the session
            User.Update(this);
        }

        /// <summary>
        /// Small wrapper function for throwing an exception if the session is invalid
        /// </summary>
        public override void ThrowIfInvalid()
        {
            if (!Valid) throw new InvalidSessionException("Session is not valid, Cookies: " + Cookies.Count +
                                                          " | Security Token: " + SecurityToken +
                                                          " | User: " + User.Name);
        }
    }
}
