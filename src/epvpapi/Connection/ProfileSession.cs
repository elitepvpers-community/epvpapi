using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace epvpapi.Connection
{
    public class ProfileSession<TUser> : Session where TUser : User
    {
        /// <summary>
        /// Represents a Profile for modeling the logged-in user.
        /// Used for accessing functions and properties that are only available for the logged-in user itself
        /// </summary>
        public class Profile
        {
            public TUser User { get; set; }
            public ProfileSession<TUser> Session { get; set; }

            public Profile(TUser user, ProfileSession<TUser> session)
            {
                User = user;
                Session = session;
            }

            /// <summary>
            /// Logs in the user
            /// </summary>
            /// <param name="md5Password"> Hashed (MD5) password of the session user </param>
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
            /// Gets all private messages stored in the specified folder
            /// </summary>
            /// <param name="folder"> 
            /// The folder where the private messages are stored. Either a pre-defined folder (such as <c>PrivateMessage.Folder.Received</c>
            /// or <c>PrivateMessage.Folder.Sent</c>) can be used or you can specify your own folder you've created by transmitting the folder ID 
            /// </param>
            /// <returns> All private messages that could be retrieved </returns>
            /// <remarks>
            /// Every page contains 100 messages - if available
            /// </remarks>
            public List<PrivateMessage> GetPrivateMessages(PrivateMessage.Folder folder)
            {
                return GetPrivateMessages(1, 1, folder);
            }

            /// <summary>
            /// Gets all private messages stored in the specified folder
            /// </summary>
            /// <param name="firstPage"> Index of the first page to request </param>
            /// <param name="pageCount"> How many pages will be requested </param>
            /// <param name="folder"> 
            /// The folder where the private messages are stored. Either a pre-defined folder (such as <c>PrivateMessage.Folder.Received</c>
            /// or <c>PrivateMessage.Folder.Sent</c>) can be used or you can specify your own folder you've created by transmitting the folder ID 
            /// </param>
            /// <returns> All private messages that could be retrieved </returns>
            /// <remarks>
            /// Every page contains 100 messages - if available
            /// </remarks>
            public List<PrivateMessage> GetPrivateMessages(uint firstPage, uint pageCount, PrivateMessage.Folder folder)
            {
                var fetchedMessages = new List<PrivateMessage>();

                for (var i = 0; i < pageCount; ++i)
                {
                    // setting 'pp' to 100 will get you exactly 100 messages per page. This is the highest count that can be set.
                    Response res = Session.Get("http://www.elitepvpers.com/forum/private.php?folderid=" + folder.ID + "&pp=100&sort=date&page=" + i);
                    var document = new HtmlDocument();
                    document.LoadHtml(res.ToString());

                    var formRootNode = document.GetElementbyId("pmform");
                    if (formRootNode == null) continue;
                    formRootNode = formRootNode.ParentNode;

                    if (formRootNode == null) continue;
                    var tborderNode = formRootNode.SelectSingleNode("table[2]");
                    if (tborderNode == null) continue;

                    // Get the amount of messages stored in the specified folder
                    // If the amount of messages is lower than the specified page count * 100, adjust the pageCount variable to fit
                    // Otherwise, vBulletin will redirect you to the previous page if it can't find the page index causing duplicate messages
                    var messageCountNode = tborderNode.SelectSingleNode("thead[1]/tr[1]/td[1]/span[1]/label[1]/strong[1]");
                    if (messageCountNode == null) break;

                    uint messageCount = Convert.ToUInt32(messageCountNode.InnerText);
                    if (messageCount == 0)
                        break;
                    
                    pageCount = (uint)Math.Ceiling((double)messageCount / 100);

                    var categoryNodes = new List<HtmlNode>(tborderNode.GetElementsByTagName("tbody").Where(node => node.Id != String.Empty));
                    foreach (var subNodes in categoryNodes.Select(categoryNode => categoryNode.GetElementsByTagName("tr")))
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
                                    sender = new User(userName, epvpapi.User.FromURL(regexMatch.Groups[1].Value));
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
                                fetchedPrivateMessage.Recipients = new List<User>() {User};
                                fetchedPrivateMessage.Sender = sender;
                            }
                            else
                            {
                                fetchedPrivateMessage.Recipients = new List<User>() {sender};
                                fetchedPrivateMessage.Sender = User;
                            }

                            fetchedMessages.Add(fetchedPrivateMessage);
                        }
                    }
                }

                return fetchedMessages;
            }

            /// <summary>
            /// Removes/disables the current Avatar of the <c>User</c>
            /// </summary>
            public void RemoveAvatar()
            {
                SetAvatar(new Image(), -1);
            }

            /// <summary>
            /// Sets the Avatar of the <c>User</c>
            /// </summary>
            /// <param name="image"> <c>Image</c> to set as new avatar </param>
            public void SetAvatar(Image image)
            {
                SetAvatar(image, 0);
            }

            /// <summary>
            /// Sets the Avatar of the <c>User</c>
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
        /// Profile representing the connected <c>User</c>
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

        public ProfileSession(TUser user, string md5Password)
        {
            Cookies = new CookieContainer();
            ConnectedProfile = new Profile(user, this);
            ConnectedProfile.Login(md5Password);
        }

        public ProfileSession(Profile profile, WebProxy proxy)
            : this(profile)
        {
            Proxy = proxy;
            UseProxy = true;
        }

        public ProfileSession(Profile profile)
        {
            Cookies = new CookieContainer();
            ConnectedProfile = profile;
        }      

        public override void Update()
        {
            base.Update();
            if (String.IsNullOrEmpty(SecurityToken) || SecurityToken == "guest")
                throw new InvalidAuthenticationException("Credentials entered for user " + User.Name + " were invalid");

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
