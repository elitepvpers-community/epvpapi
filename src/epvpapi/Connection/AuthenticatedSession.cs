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
using epvpapi.TBM;
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
        /// Represents the logged-in user that is able to access functions and properties that are only available for logged-in users
        /// </summary>
        public class Profile
        {
            /// <summary>
            /// The user object representing information that are publicly available for everyone
            /// </summary>
            public TUser User { get; set; }

            /// <summary>
            /// Session used for sending the requests
            /// </summary>
            public AuthenticatedSession<TUser> AuthenticatedSession { get; private set; }


            public Profile(TUser user, AuthenticatedSession<TUser> session)
            {
                User = user;
                AuthenticatedSession = session;
            }

            /// <summary>
            /// Logs in the user
            /// </summary>
            /// <param name="md5Password"> Hashed (MD5) password of the session user </param>
            /// <remarks>
            /// In order for this function to work, either the real username or the e-mail address has to be set in the <c>User</c> property
            /// </remarks>
            public void Login(string md5Password)
            {
                var res = AuthenticatedSession.Post("http://www.elitepvpers.com/forum/login.php?do=login&langid=1",
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

                AuthenticatedSession.Update();
            }

            /// <summary>
            /// Gets the current Secret word
            /// </summary>
            /// <returns> Current Secret word as string </returns>
            public string GetSecretWord()
            {
                AuthenticatedSession.ThrowIfInvalid();

                var res = AuthenticatedSession.Get("http://www.elitepvpers.com/theblackmarket/api/secretword/");
                var doc = new HtmlDocument();
                doc.LoadHtml(res.ToString());

                return doc.DocumentNode.Descendants().GetElementsByNameXHtml("secretword").FirstOrDefault().Attributes["value"].Value;
            }

            /// <summary>
            /// Sets the Secret word
            /// </summary>
            public void SetSecretWord(string newSecretWord)
            {
                AuthenticatedSession.ThrowIfInvalid();

                AuthenticatedSession.Post("http://www.elitepvpers.com/theblackmarket/api/secretword/",
                    new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("secretword", newSecretWord)
                    });
            }


            /// <summary>
            /// Retrieves all subscribed <c>SectionThread</c>s using the logged-in user account
            /// </summary>
            /// <returns> List of all subscribed <c>SectionThread</c>s that could be retrieved </returns>
            public List<SectionThread> GetSubscribedThreads()
            {
                AuthenticatedSession.ThrowIfInvalid();

                var subscribedThreads = new List<SectionThread>();

                var res = AuthenticatedSession.Get("http://www.elitepvpers.com/forum/subscription.php?do=viewsubscription&daysprune=-1&folderid=all");
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(res.ToString());

                // a bit ugly but it works so far
                var rootNode = htmlDocument.DocumentNode.SelectSingleNode("//tr/td[@class='tcat']/span[@class='smallfont']").ParentNode.ParentNode.ParentNode;
                if (rootNode == null)
                    return null;

                foreach (var node in rootNode.ChildNodes.GetElementsByName("tr"))
                {
                    if (node.SelectSingleNode("td[@class='thead']") != null
                        || node.SelectSingleNode("td[@class='tcat']") != null
                        || node.SelectSingleNode("td[@class='tfoot']") != null)
                        continue;

                    var threadId = new Regex("<td class=\"alt1\" id=\"td_threadtitle_(.*?)\"").Match(node.InnerHtml).Groups[1].Value.To<int>();
                    var threadSection = new Regex("<a href=\"(.*?)/.*?\" id=\"thread_title_.*?\">.*?</a>").Match(node.InnerHtml).Groups[1].Value;
                    var section = Section.Sections.Where(n => n.UrlName == threadSection).FirstOrDefault();

                    var subscribedThread = new SectionThread(threadId, section);
                    subscribedThreads.Add(subscribedThread);
                }

                return subscribedThreads;
            }

            /// <summary>
            /// Retrieves all <c>Treasure</c>s that have been bought and/or sold using the logged-in user account
            /// </summary>
            /// <param name="queryStatus">
            /// Type of <c>Treasure</c> to query. Either <c>Treasure.Query.SoldListed</c> 
            /// for querying treasures that have been sold/listed or <c>Treasure.Query.Bought</c> 
            /// for treasures that were bought </param>
            /// <param name="pageCount"> Amount of pages to retrieve, one page may contain up to 15 treasures </param>
            /// <param name="startIndex"> Index of the first page to request (1 for the first page, 2 for the second, ...) </param>
            /// <returns> List of all <c>Treasure</c>s that could be retrieved </returns>
            public List<Treasure> GetTreasures(Treasure.Query queryStatus = Treasure.Query.SoldListed, uint pageCount = 1, uint startIndex = 1)
            {
                AuthenticatedSession.ThrowIfInvalid();

                var listedTreasures = new List<Treasure>();
                for (var i = startIndex; i < (startIndex + pageCount); ++i)
                {
                    var res = AuthenticatedSession.Get("http://www.elitepvpers.com/theblackmarket/treasures/" +
                        ((queryStatus == Treasure.Query.Bought) ? "bought" : "soldunsold")
                        + "/" + i);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(res.ToString());

                    var rootFormNode = htmlDocument.GetElementbyId("contentbg");
                    if (rootFormNode == null) continue;

                    var tableNode = rootFormNode.SelectSingleNode("table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]/div[1]/div[3]/table[1]");
                    if (tableNode == null) continue;

                    // skip the first <tr> element since that is the table header
                    foreach (var treasureListingNode in tableNode.ChildNodes.GetElementsByTagName("tr").Skip(1))
                    {
                        var idNode = treasureListingNode.SelectSingleNode("td[1]");
                        var titleNode = treasureListingNode.SelectSingleNode("td[2]");
                        var costNode = treasureListingNode.SelectSingleNode("td[3]");
                        var opponentNode = treasureListingNode.SelectSingleNode("td[4]");
                        var listedTreasure = new Treasure
                        {
                            // first column is the id with a trailing #
                            ID = (idNode != null) ? idNode.InnerText.TrimStart('#').To<int>() : 0,

                            // second column is the treasure title
                            Title = (titleNode != null) ? titleNode.InnerText : "",
                        };

                        // since this function is only available for logged-in users, the seller (or buyer, depends on the request) is automatically the logged-in user
                        if (queryStatus == Treasure.Query.Bought)
                            listedTreasure.Buyer = User;
                        else
                            listedTreasure.Seller = User;

                        // third column is the cost
                        var match = new Regex(@"([0-9]+) eg").Match(costNode.InnerText);
                        if (match.Groups.Count > 1)
                            listedTreasure.Cost = Convert.ToUInt32(match.Groups[1].Value);

                        // the last column is the treasure buyer or seller
                        if (opponentNode != null)
                        {
                            opponentNode = opponentNode.SelectSingleNode("a[1]");
                            if (opponentNode != null)
                            {
                                var opponent = opponentNode.Attributes.Contains("href")
                                    ? new User(opponentNode.InnerText,
                                        epvpapi.User.FromUrl(opponentNode.Attributes["href"].Value))
                                    : new User();

                                if (queryStatus == Treasure.Query.Bought)
                                {
                                    listedTreasure.Seller = opponent;
                                    listedTreasure.Available = false;
                                }
                                else
                                {
                                    listedTreasure.Buyer = opponent;
                                    listedTreasure.Available = false;
                                }
                            }
                        }

                        listedTreasures.Add(listedTreasure);
                    }
                }

                return listedTreasures;
            }

            /// <summary>
            /// Removes/disables the current Avatar
            /// </summary>
            public void RemoveAvatar()
            {
                SetAvatar(new Image(), -1);
            }

            /// <summary>
            /// Sets the Avatar
            /// </summary>
            /// <param name="image"> <c>Image</c> to set as new avatar </param>
            public void SetAvatar(Image image)
            {
                SetAvatar(image, 0);
            }

            /// <summary>
            /// Sets the Avatar
            /// </summary>
            /// <param name="image"> <c>Image</c> to set as new avatar </param>
            /// <param name="changeType"> 0 for uploading a new avatar, -1 for deleting the old one without uploading a new one </param>
            protected void SetAvatar(Image image, int changeType)
            {
                var content = new MultipartFormDataContent
                {
                    {
                        new ByteArrayContent(image.Data), "upload",
                        (String.IsNullOrEmpty(image.Name)) ? "Unnamed.jpeg" : image.Name + image.Format
                    },
                    { new StringContent(String.Empty), "s" },
                    { new StringContent(AuthenticatedSession.SecurityToken), "securitytoken" },
                    { new StringContent("updateavatar"), "do" },
                    { new StringContent(changeType.ToString()), "avatarid" }
                };

                AuthenticatedSession.PostMultipartFormData(new Uri("http://www.elitepvpers.com/forum/profile.php?do=updateavatar"), content);
            }

            /// <summary>
            /// Sets the signature content
            /// </summary>
            /// <param name="signatureContent"> Content to set as signature </param>
            public void SetSignature(Content signatureContent)
            {
            
                var content = new MultipartFormDataContent
                {
                    // Encoding (SBCSCodePageEncoding) is important and required to transmit german characters such as Ä, Ü, Ö...
                    { new StringContent(signatureContent.ToString(), System.Text.Encoding.GetEncoding("ISO-8859-1")), "message" }, 
                    { new StringContent("0"), "wysiwyg" },
                    { new StringContent("http://www.elitepvpers.com/forum/usercp.php"), "url" },
                    { new StringContent(String.Empty), "s" },
                    { new StringContent(AuthenticatedSession.SecurityToken), "securitytoken" },
                    { new StringContent("updatesignature"), "do" },
                    { new StringContent("52428800"), "MAX_FILE_SIZE" }
                };

                AuthenticatedSession.PostMultipartFormData(new Uri("http://www.elitepvpers.com/forum/profile.php?do=updatesignature"), content);
            }
        }


        /// <summary>
        /// Profile representing the logged-in user being bound to the session
        /// </summary>
        public Profile ConnectedProfile { get; private set; }

        /// <summary>
        /// Shortcut for accessing the <c>User</c> object within the connected profile
        /// </summary>
        public TUser User
        {
            get { return ConnectedProfile.User; }
            set { ConnectedProfile.User = value; }
        }

        public AuthenticatedSession(TUser user, string md5Password):
            this(user)
        {
            ConnectedProfile.Login(md5Password);
        }

        public AuthenticatedSession(TUser user, string md5Password, WebProxy proxy) :
            this(user, proxy)
        {
            ConnectedProfile.Login(md5Password);
        }

        public AuthenticatedSession(TUser user, WebProxy proxy):
            this(user)
        {
            UseProxy = true;
            Proxy = proxy;
        }

        public AuthenticatedSession(TUser user)
        {
            ConnectedProfile = new Profile(user, this);
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
