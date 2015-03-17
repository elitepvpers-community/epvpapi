using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using HtmlAgilityPack;

namespace epvpapi
{
    public class PrivateMessageFolder
    {
        /// <summary>
        /// Unique identifier for identifying the folder in requests
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Name of the folder. Pre-defined folders always have default names, custom folders can be named
        /// </summary>
        public string Name { get; set; }

        public static PrivateMessageFolder Outbox
        {
            get { return new PrivateMessageFolder(-1, "Outbox"); }
        }

        public static PrivateMessageFolder Inbox
        {
            get { return new PrivateMessageFolder(0, "Inbox"); }
        }

        public PrivateMessageFolder(int id, string name = null)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// Gets all private messages that are stored in the specified folder within the speicifed boundaries
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="pageCount"> Amount of pages to retrieve, one page may contain up to 100 messages </param>
        /// <param name="startIndex"> Index of the first page to request (1 for the first page, 2 for the second, ...) </param>
        /// <returns> All private messages that could be retrieved </returns>
        public List<PrivateMessage> GetMessages<TUser>(AuthenticatedSession<TUser> session, uint pageCount = 1, uint startIndex = 1) where TUser : User
        {
            var fetchedMessages = new List<PrivateMessage>();

            for (var i = startIndex; i < (startIndex + pageCount); ++i)
            {
                // setting 'pp' to 100 will get you exactly 100 messages per page. This is the highest count that can be set.
                var res = session.Get("http://www.elitepvpers.com/forum/private.php?folderid=" + ID + "&pp=100&sort=date&page=" + i);
                var document = new HtmlDocument();
                document.LoadHtml(res);

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
                        var pmID = new string(tdBaseNode.Id.Skip(1).ToArray()).To<int>(); // skip the first character that is always prefixed before the actual id

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

                        var userNameNode = tdBaseNode.SelectSingleNode("div[2]/span[2]");
                        if (userNameNode == null)
                        {
                            // Unread messages are shown with bold font
                            userNameNode = tdBaseNode.SelectSingleNode("div[2]/strong[1]/span[1]");
                            messageUnread = true;
                        }

                        var userName = (userNameNode != null) ? userNameNode.InnerText : "";
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
                        if (ID == Inbox.ID)
                        {
                            fetchedPrivateMessage.Recipients = new List<User>() { session.User };
                            fetchedPrivateMessage.Sender = sender;
                        }
                        else
                        {
                            fetchedPrivateMessage.Recipients = new List<User>() { sender };
                            fetchedPrivateMessage.Sender = session.User;
                        }

                        fetchedMessages.Add(fetchedPrivateMessage);
                    }
                }
            }

            return fetchedMessages;
        }
    }
}
