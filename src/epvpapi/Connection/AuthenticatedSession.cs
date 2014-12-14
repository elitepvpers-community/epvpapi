using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi.Connection
{
    /// <summary>
    /// Represents a web session
    /// </summary>
    public class AuthenticatedSession<TUser> : GuestSession where TUser : User
    {      
        /// <summary>
        /// Represents an unique ID identifiying the session which is frequently getting updated on each request
        /// Besides that, the security token needs to be provided for most of the forum actions.
        /// Although the token updates frequently, the initial security token is sufficient to be accepted by elitepvpers
        /// </summary>
        public string SecurityToken { get; private set; }

       
        /// <summary>
        /// Searches the local cookie container for the session cookie.
        /// Determines whether the session is valid or not, i.e. if the session cookie has been set
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


        /// <summary>
        /// Profile representing the logged-in user being bound to the session
        /// </summary>
        public Profile<TUser> Profile { get; private set; }

        /// <summary>
        /// Shortcut for accessing the <c>User</c> object within the connected profile
        /// </summary>
        public TUser User
        {
            get { return Profile.User; }
            set { Profile.User = value; }
        }

        public AuthenticatedSession(TUser user, string md5Password):
            this(user)
        {
            Profile.Login(this, md5Password);
        }

        public AuthenticatedSession(TUser user, string md5Password, WebProxy proxy) :
            this(user, proxy)
        {
            Profile.Login(this, md5Password);
        }

        public AuthenticatedSession(TUser user, WebProxy proxy):
            this(user)
        {
            UseProxy = true;
            Proxy = proxy;
        }

        public AuthenticatedSession(TUser user)
        {
            Profile = new Profile<TUser>(user);
            Cookies = new CookieContainer();
        }   
    
        /// <summary>
        /// Logs out the logged-in user and destroys the session
        /// </summary>
        public void Destroy()
        {
            ThrowIfInvalid();
            Get("http://www.elitepvpers.com/forum/login.php?do=logout&logouthash=" + SecurityToken);
        }
   
        /// <summary>
        /// Updates required session information such as the SecurityToken
        /// </summary>
        public void Update()
        {
            var res = Get("http://www.elitepvpers.com/forum/");
            SecurityToken = new Regex("SECURITYTOKEN = \"(\\S+)\";").Match(res.ToString()).Groups[1].Value;

            if (String.IsNullOrEmpty(SecurityToken) || SecurityToken == "guest")
                throw new InvalidAuthenticationException("Credentials entered for user " + User.Name + " were invalid");

            // Automatically retrieve the logged-in user's id if it hasn't been set
            if (User.ID == 0)
            {
                var document = new HtmlDocument();
                document.LoadHtml(res.ToString());

                var userBarNode = document.GetElementbyId("userbaritems");
                if (userBarNode != null)
                {
                    var userProfileLinkNode = userBarNode.SelectSingleNode("li[1]/a[1]");

                    User.ID = userProfileLinkNode.Attributes.Contains("href")
                        ? epvpapi.User.FromUrl(userProfileLinkNode.Attributes["href"].Value)
                        : 0;
                }
            }

            // Update the session user
            User.Update(this);
        }

        /// <summary>
        /// Small wrapper function for throwing an exception if the session is invalid
        /// </summary>
        internal void ThrowIfInvalid()
        {
            if (!Valid)
                throw new InvalidSessionException(
                    String.Format("Session is not valid, Cookies: {0} | Security Token: {1} | User: {2}",
                    Cookies.Count, SecurityToken, User.Name));
        }
    }
}
