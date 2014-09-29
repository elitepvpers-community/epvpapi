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
using epvpapi.TBM;
using HtmlAgilityPack;

namespace epvpapi.Connection
{
    /// <summary>
    /// Represents a web session
    /// </summary>
    public class Session<TUser> where TUser : User
    {
        /// <summary>
        /// Container containing the cookies used in the current session
        /// </summary>
        public CookieContainer Cookies { get; private set; }

        /// <summary>
        /// Represents an unique ID identifiying the session which is frequently getting updated on each request
        /// Besides that, the security token needs to be provided for most of the forum actions.
        /// Although the token updates frequently, the initial security token is sufficient to be accepted by elitepvpers
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Proxy for issuing requests when behind a proxy
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        /// If set to true, the proxy will be used for all requests
        /// </summary>
        public bool UseProxy { get; set; }


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
            public Session<TUser> Session { get; private set; }


            public Profile(TUser user, Session<TUser> session)
            {
                User = user;
                Session = session;
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
                var res = Session.Post("http://www.elitepvpers.com/forum/login.php?do=login&langid=1",
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

                Session.Update();
            }

            /// <summary>
            /// Gets the current Secret word
            /// </summary>
            /// <returns> Current Secret word as string </returns>
            public string GetSecretWord()
            {
                Session.ThrowIfInvalid();

                var res = Session.Get("http://www.elitepvpers.com/theblackmarket/api/secretword/");
                var doc = new HtmlDocument();
                doc.LoadHtml(res.ToString());

                return doc.DocumentNode.Descendants().GetElementsByNameXHtml("secretword").FirstOrDefault().Attributes["value"].Value;
            }

            /// <summary>
            /// Sets the Secret word
            /// </summary>
            public void SetSecretWord(string newSecretWord)
            {
                Session.ThrowIfInvalid();

                Session.Post("http://www.elitepvpers.com/theblackmarket/api/secretword/",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("secretword", newSecretWord)
                            });
            }


            /// <summary>
            /// Publishes the <c>Blog.Entry</c> in the logged-in user's blog
            /// </summary>
            /// <param name="blogEntry"> Blog entry to be published </param>
            /// <param name="settings"> Additional options that can be set </param>
            public void Publish(Blog.Entry blogEntry, Blog.Entry.Settings settings = Blog.Entry.Settings.ParseUrl | Blog.Entry.Settings.AllowComments)
            {
                Publish(blogEntry, DateTime.Now, settings);
            }

            /// <summary>
            /// Publishes the <c>Blog.Entry</c> in the logged-in user's blog at the given time (automatically)
            /// </summary>
            /// <param name="blogEntry"> Blog entry to be published </param>
            /// <param name="publishDate"> Date and time when the entry will go live </param>
            /// <param name="settings"> Additional options that can be set </param>
            public void Publish(Blog.Entry blogEntry, DateTime publishDate,
                                Blog.Entry.Settings settings = Blog.Entry.Settings.ParseUrl | Blog.Entry.Settings.AllowComments)
            {
                Session.ThrowIfInvalid();

                blogEntry.Date = publishDate;
                var tags = "";
                foreach (var tag in blogEntry.Tags)
                {
                    tags += tag;
                    if (blogEntry.Tags.Last() != tag)
                        tags += ",";
                }

                Session.Post("http://www.elitepvpers.com/forum/blog_post.php?do=updateblog&blogid=",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("title", blogEntry.Title),
                                new KeyValuePair<string, string>("message", blogEntry.Content.ToString()),
                                new KeyValuePair<string, string>("wysiwyg", "0"),
                                new KeyValuePair<string, string>("s", String.Empty),
                                new KeyValuePair<string, string>("securitytoken", Session.SecurityToken),
                                new KeyValuePair<string, string>("do", "updateblog"),
                                new KeyValuePair<string, string>("b", String.Empty),
                                new KeyValuePair<string, string>("posthash", String.Empty),
                                new KeyValuePair<string, string>("poststarttime", Extensions.UnixTimestamp().ToString()),
                                new KeyValuePair<string, string>("loggedinuser", Session.User.ID.ToString()),
                                new KeyValuePair<string, string>("u", String.Empty),
                                new KeyValuePair<string, string>("taglist", tags),
                                new KeyValuePair<string, string>("allowcomments", Convert.ToUInt32(settings.HasFlag(Blog.Entry.Settings.AllowComments)).ToString()),
                                new KeyValuePair<string, string>("moderatecomments", Convert.ToUInt32(settings.HasFlag(Blog.Entry.Settings.ModerateComments)).ToString()),
                                new KeyValuePair<string, string>("private", Convert.ToUInt32(settings.HasFlag(Blog.Entry.Settings.Private)).ToString()),
                                new KeyValuePair<string, string>("status", (publishDate.Compare(DateTime.Now)) ? "publish_now" : "publish_on"),
                                new KeyValuePair<string, string>("publish[month]", blogEntry.Date.Month.ToString()),
                                new KeyValuePair<string, string>("publish[day]", blogEntry.Date.Day.ToString()),
                                new KeyValuePair<string, string>("publish[year]", blogEntry.Date.Year.ToString()),
                                new KeyValuePair<string, string>("publish[hour]", blogEntry.Date.Hour.ToString()),
                                new KeyValuePair<string, string>("publish[minute]", blogEntry.Date.Minute.ToString()),
                                new KeyValuePair<string, string>("parseurl", Convert.ToUInt32(settings.HasFlag(Blog.Entry.Settings.ParseUrl)).ToString()),
                                new KeyValuePair<string, string>("parseame", "1"),
                                new KeyValuePair<string, string>("emailupdate", "none"),
                                new KeyValuePair<string, string>("sbutton", "Submit")
                            });
            }

            /// <summary>
            /// Gets all private messages that are stored in the specified folder
            /// </summary>
            /// <param name="folder"> 
            /// The folder to retrieve messages from. Either a pre-defined folder (such as <c>PrivateMessage.Folder.Received</c>
            /// or <c>PrivateMessage.Folder.Sent</c>) can be used or you can specify your own folder you've created by passing
            /// in the id of the folder
            /// </param>
            /// <returns> All private messages that could be retrieved </returns>
            public List<PrivateMessage> GetPrivateMessages(PrivateMessage.Folder folder)
            {
                return GetPrivateMessages(1, 1, folder);
            }

            /// <summary>
            /// Gets all private messages that are stored in the specified folder within the speicifed boundaries
            /// </summary>
            /// <param name="pageCount"> Amount of pages to retrieve, one page may contain up to 100 messages </param>
            /// <param name="startIndex"> Index of the first page to request (1 for the first page, 2 for the second, ...) </param>
            /// <param name="folder"> 
            /// The folder to retrieve messages from. Either a pre-defined folder (such as <c>PrivateMessage.Folder.Received</c>
            /// or <c>PrivateMessage.Folder.Sent</c>) can be used or you can specify your own folder you've created by passing
            /// in the id of the folder
            /// </param>
            /// <returns> All private messages that could be retrieved </returns>
            public List<PrivateMessage> GetPrivateMessages(uint pageCount, uint startIndex, PrivateMessage.Folder folder)
            {
                var fetchedMessages = new List<PrivateMessage>();

                for (var i = startIndex; i < (startIndex + pageCount); ++i)
                {
                    // setting 'pp' to 100 will get you exactly 100 messages per page. This is the highest count that can be set.
                    var res = Session.Get("http://www.elitepvpers.com/forum/private.php?folderid=" + folder.ID + "&pp=100&sort=date&page=" + i);
                    var document = new HtmlDocument();
                    document.LoadHtml(res.ToString());

                    var formRootNode = document.GetElementbyId("pmform");
                    if (formRootNode == null) continue;

                    formRootNode = formRootNode.ParentNode;
                    if (formRootNode == null) continue;

                    var tborderNode = formRootNode.SelectSingleNode("table[2]");
                    if (tborderNode == null) continue;

                    // Get the amount of messages stored in the specified folder
                    // Some people had issues with this, since their message count was shown within table[3] while on a testaccount, the count was shown within table[2]
                    var messageCountNode = tborderNode.SelectSingleNode("thead[1]/tr[1]/td[1]/span[1]/label[1]/strong[1]");
                    if (messageCountNode == null)
                    {
                        tborderNode = formRootNode.SelectSingleNode("table[3]");
                        if (tborderNode == null) continue;
                        messageCountNode = tborderNode.SelectSingleNode("thead[1]/tr[1]/td[1]/span[1]/label[1]/strong[1]");
                    }

                    if (messageCountNode == null) continue;
                    var messageCount = (uint)double.Parse(messageCountNode.InnerText, CultureInfo.InvariantCulture);
                    if (messageCount == 0) break;

                    // If the requested page count is higher than the available pages, adjust the pageCount variable to fit the actual count
                    // Otherwise, vBulletin will redirect you to the previous page if it can't find the page index causing duplicate messages
                    var actualPageCount = (uint)Math.Ceiling((double)messageCount / 100);
                    if (pageCount > actualPageCount)
                        pageCount = actualPageCount;

                    var categoryNodes = new List<HtmlNode>(tborderNode.ChildNodes.GetElementsByTagName("tbody").Where(node => node.Id != String.Empty));
                    foreach (var subNodes in categoryNodes.Select(categoryNode => categoryNode.ChildNodes.GetElementsByTagName("tr")))
                    {
                        foreach (var subNode in subNodes)
                        {
                            var tdBaseNode = subNode.SelectSingleNode("td[3]");
                            if (tdBaseNode == null) continue;
                            uint pmID = Convert.ToUInt32(new string(tdBaseNode.Id.Skip(1).ToArray())); // skip the first character that is always prefixed before the actual id

                            var dateNode = tdBaseNode.SelectSingleNode("div[1]/span[1]");
                            string date = (dateNode != null) ? dateNode.InnerText : "";

                            var timeNode = tdBaseNode.SelectSingleNode("div[2]/span[1]");
                            string time = (timeNode != null) ? timeNode.InnerText : "";

                            var messageUnread = false;

                            string title = "";
                            var titleNode = tdBaseNode.SelectSingleNode("div[1]/a[1]");
                            if (titleNode == null)
                            {
                                // Unread messages are shown with bold font
                                titleNode = tdBaseNode.SelectSingleNode("div[1]/a[1]/strong[1]");
                                messageUnread = true;
                            }
                            title = (titleNode != null) ? titleNode.InnerText : "";

                            string userName = "";
                            var userNameNode = tdBaseNode.SelectSingleNode("div[2]/span[2]");
                            if (userNameNode == null)
                            {
                                // Unread messages are shown with bold font
                                userNameNode = tdBaseNode.SelectSingleNode("div[2]/strong[1]/span[1]");
                                messageUnread = true;
                            }
                            userName = (userNameNode != null) ? userNameNode.InnerText : "";

                            var sender = new User(userName);
                            if (userNameNode != null)
                            {
                                var regexMatch = Regex.Match(userNameNode.Attributes["onclick"].Value,
                                    @"window.location='(\S+)';");
                                // the profile link is stored within a javascript page redirect command
                                if (regexMatch.Groups.Count > 1)
                                    sender = new User(userName, epvpapi.User.FromUrl(regexMatch.Groups[1].Value));
                            }

                            var fetchedPrivateMessage = new PrivateMessage(pmID)
                            {
                                Title = title,
                                Date = (date + " " + time).ToElitepvpersDateTime(),
                                Unread = messageUnread
                            };

                            // Messages that were send are labeled with the user that received the message. If messages were received, they were labeled with the sender
                            // so we need to know wether the folder stores received or sent messages
                            if (folder.StorageType == PrivateMessage.Folder.Storage.Received)
                            {
                                fetchedPrivateMessage.Recipients = new List<User>() { User };
                                fetchedPrivateMessage.Sender = sender;
                            }
                            else
                            {
                                fetchedPrivateMessage.Recipients = new List<User>() { sender };
                                fetchedPrivateMessage.Sender = User;
                            }

                            fetchedMessages.Add(fetchedPrivateMessage);
                        }
                    }
                }

                return fetchedMessages;
            }

            /// <summary>
            /// Retrieves all subscribed <c>SectionThread</c>s using the logged-in user account
            /// </summary>
            /// <returns> List of all subscribed <c>SectionThread</c>s that could be retrieved </returns>
            public List<SectionThread> GetSubscribedThreads()
            {
                Session.ThrowIfInvalid();

                var subscribedThreads = new List<SectionThread>();

                var res = Session.Get("http://www.elitepvpers.com/forum/subscription.php?do=viewsubscription&daysprune=-1&folderid=all");
                var htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(res.ToString());

                // a bit ugly but it works so far
                var rootNode = htmlDocument.DocumentNode.SelectSingleNode("//tr/td[@class='tcat']/span[@class='smallfont']").ParentNode.ParentNode.ParentNode;
                if (rootNode == null)
                    return null;

                foreach (var node in rootNode.ChildNodes.GetElementsByName("tr"))
                {
                    if (node.SelectSingleNode("td[@class='thead']") != null || 
                        node.SelectSingleNode("td[@class='tcat']") != null || 
                        node.SelectSingleNode("td[@class='tfoot']") != null)
                        continue;

                    uint threadId = Convert.ToUInt32(new Regex("<td class=\"alt1\" id=\"td_threadtitle_(.*?)\"").Match(node.InnerHtml).Groups[1].Value);
                    string threadSection = new Regex("<a href=\"(.*?)/.*?\" id=\"thread_title_.*?\">.*?</a>").Match(node.InnerHtml).Groups[1].Value;
                    Section section = Section.Sections.Where(n => n.UrlName == threadSection).FirstOrDefault();

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
                Session.ThrowIfInvalid();

                var listedTreasures = new List<Treasure>();
                for (var i = startIndex; i < (startIndex + pageCount); ++i)
                {
                    var res = Session.Get("http://www.elitepvpers.com/theblackmarket/treasures/" +
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
                            ID = (idNode != null) ? Convert.ToUInt32(idNode.InnerText.TrimStart('#')) : 0,

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
                    { new StringContent(Session.SecurityToken), "securitytoken" },
                    { new StringContent("updateavatar"), "do" },
                    { new StringContent(changeType.ToString()), "avatarid" }
                };

                Session.PostMultipartFormData(new Uri("http://www.elitepvpers.com/forum/profile.php?do=updateavatar"), content);
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


        public Session(TUser user, string md5Password):
            this(user)
        {
            ConnectedProfile.Login(md5Password);
        }

        public Session(TUser user, string md5Password, WebProxy proxy) :
            this(user, proxy)
        {
            ConnectedProfile.Login(md5Password);
        }

        public Session(TUser user, WebProxy proxy):
            this(user)
        {
            UseProxy = true;
            Proxy = proxy;
        }

        public Session(TUser user)
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
            if (!Valid) throw new InvalidSessionException("Session is not valid, Cookies: " + Cookies.Count +
                                                          " | Security Token: " + SecurityToken +
                                                          " | User: " + User.Name);
        }


        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Get(Uri url)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = Cookies
                };

                if(UseProxy)
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
            catch (System.Net.CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        /// <summary> Performs a HTTP GET request </summary>
        /// <param name="url"> Location to request </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Get(string url)
        {
            return Get(new Uri(url));
        }


        /// <summary> Performs a HTTP POST request </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <returns> Server <c>Response</c> to the sent request </returns>
        internal Response Post(string url, IEnumerable<KeyValuePair<string, string>> content)
        {
            try
            {
                var targetUrl = new Uri(url);

                var handler = new HttpClientHandler()
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
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
                client.DefaultRequestHeaders.Add("Accept-Language", "de-DE,de;q=0.8,en-US;q=0.6,en;q=0.4");
                client.DefaultRequestHeaders.Add("User-Agent", "epvpapi - .NET Library v." 
                                                 + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);

                var encodedContent = new FormUrlEncodedContent(content);
                encodedContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                encodedContent.Headers.ContentType.CharSet = "UTF-8";

                var response = client.PostAsync(targetUrl, encodedContent);
                if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != HttpStatusCode.SeeOther && response.Result.StatusCode != HttpStatusCode.Redirect)
                    throw new RequestFailedException("Request failed, Server returned " + response.Result.StatusCode);

                return new Response(response.Result);
            }
            catch (System.Net.CookieException exception)
            {
                throw new RequestFailedException("The Session could not be resolved", exception);
            }
        }

        /// <summary>
        /// Performs a Multipart POST request
        /// </summary>
        /// <param name="url"> Location where to post the data </param>
        /// <param name="content"> Contents to post </param>
        /// <returns> Server <c>Response</c> to the sent request  </returns>
        internal Response PostMultipartFormData(Uri url, MultipartFormDataContent content)
        {
            var handler = new HttpClientHandler()
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

            var response = client.PostAsync(url, content);
            if (!response.Result.IsSuccessStatusCode) throw new RequestFailedException("Server returned " + response.Result.StatusCode);
            return new Response(response.Result);
        }
    }
}