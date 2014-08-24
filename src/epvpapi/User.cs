using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents an user in elitepvpers
    /// </summary>
    public class User : UniqueObject
    {
        /// <summary>
        /// Available usergroups an user can have 
        /// </summary>
        [Flags]
        public enum Usergroups
        {
            [Description("Keine")]
            None,
            [Description("Premium")]
            Premium,
            [Description("Level 2")]
            Level2,
            [Description("Level 3")]
            Level3,
            [Description("Moderator")]
            Moderator,
            [Description("Global Moderator")]
            GlobalModerator,
            [Description("Administrator")]
            Administrator
        };

        public enum Status
        {
            Online,
            Offline,
            Invisible
        }

        /// <summary>
        /// Name of the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Color of the name
        /// </summary>
        public string Namecolor { get; set; }

        /// <summary>
        /// Custom user title displayed beneath the name
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The current status of the user
        /// </summary>
        public Status CurrentStatus { get; set; }


        /// <summary>
        /// The last activity of the user given is <c>DateTime</c> format
        /// </summary>
        public DateTime LastActivity { get; set; }


        /// <summary>
        /// Amount of thanks the user has given
        /// </summary>
        public uint ThanksGiven { get; set; }

        /// <summary>
        /// Amount of thanks the user has received
        /// </summary>
        public uint ThanksReceived { get; set; }

        /// <summary>
        /// The associated user blog
        /// </summary>
        public Blog Blog { get; set; }

        /// <summary>
        /// List of usergroups the user has
        /// </summary>
        public Usergroups Groups { get; set; }


        public User(uint id = 0)
            : this(null, id)
        { }

        /// <param name="name"> Name of the user </param>
        /// <param name="id"> ID of the user (profile ID)</param>
        public User(string name, uint id = 0)
            : base(id)
        {
            Name = name;
            Blog = new Blog();
            LastActivity = new DateTime();
            Groups |= Usergroups.None;
        }

        /// <summary>
        /// Updates the user by requesting the profile
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            Response res = session.Get("http://www.elitepvpers.com/forum/members/" + ID.ToString() + "--.html");
            Parse(res.ToString());
        }


        /// <summary>
        /// Parses the responseContent in order to fetch all available information of the user
        /// </summary>
        /// <param name="responseContent"> Plain HTML response content </param>
        private void Parse(string responseContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseContent);

            HtmlNode userNameBoxNode = doc.GetElementbyId("username_box"); // root element
            if (userNameBoxNode == null) throw new ParsingFailedException("User could not be parsed, root node is invalid or was not found");
                
            HtmlNode userNameNode = userNameBoxNode.SelectSingleNode("h1[1]/span[1]");
            if (userNameNode != null)
            {
                Name = userNameNode.InnerText;
            }
            else
            {
                // In case the user has no special color, the <span> element will be missing and no attributes are used
                userNameNode = userNameBoxNode.SelectSingleNode("h1[1]");
                Name = (userNameNode != null) ? userNameNode.InnerText : String.Empty;
            }

            HtmlNode userRankNode = userNameBoxNode.SelectSingleNode("h2[1]");
            Title = (userRankNode != null) ? userRankNode.InnerText : String.Empty;

            Namecolor = userNameNode.Attributes.Count != 0 ? userNameNode.Attributes.First().Value : "black";


            HtmlNode userStatusNode = userNameBoxNode.SelectSingleNode("h1[1]/img[1]");
            string userStatusLink = userStatusNode.Attributes["src"].Value;
            if (userStatusLink.Contains("invisible"))
                CurrentStatus = Status.Invisible;
            else if (userStatusLink.Contains("offline"))
                CurrentStatus = Status.Offline;
            else if (userStatusLink.Contains("online"))
                CurrentStatus = Status.Online;

            HtmlNode lastActivityNode = doc.GetElementbyId("last_online");
            if (lastActivityNode != null)
            {
                HtmlNode lastActivityDateNode = lastActivityNode.SelectSingleNode("text()[2]");
                string date = (lastActivityDateNode != null) ? lastActivityDateNode.InnerText.Strip() : String.Empty;

                if (date == "Heute" || date == "Today")
                    date = DateTime.Now.Date.ToString("dd/MM/yyyy");

                HtmlNode lastActivityTimeNode = lastActivityNode.SelectSingleNode("span[2]");
                string time = (lastActivityTimeNode != null) ? lastActivityTimeNode.InnerText.Strip() : String.Empty;

                DateTime parsedDateTime = new DateTime();
                DateTime.TryParse(date + " " + time, out parsedDateTime);
                LastActivity = parsedDateTime; 
            }
        }

        /// <summary>
        /// Gets all private messages stored in the specified folder
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
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
        public List<PrivateMessage> GetPrivateMessages(Session session, uint firstPage = 1, uint pageCount = 1, PrivateMessage.Folder folder = PrivateMessage.Folder.Received)
        {
            List<PrivateMessage> fetchedMessages = new List<PrivateMessage>();

            for (int i = 0; i < pageCount; ++i)
            {
                // setting 'pp' to 100 will get you exactly 100 messages per page. This is the highest count that can be set.
                Response res = session.Get("http://www.elitepvpers.com/forum/private.php?folderid=" + Convert.ToInt32(folder) + "&pp=100&sort=date&page=" + i);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(res.ToString());

                HtmlNode formRootNode = document.GetElementbyId("pmform");
                if (formRootNode == null) continue;
                formRootNode = formRootNode.ParentNode;

                if (formRootNode == null) continue;
                HtmlNode tborderNode = formRootNode.SelectSingleNode("table[2]");
                if (tborderNode == null) continue;

                // Get the amount of messages stored in the specified folder
                // If the amount of messages is lower than the specified page count * 100, adjust the pageCount variable to fit
                // Otherwise, vBulletin will redirect you to the previous page if it can't find the page index causing duplicate messages
                HtmlNode messageCountNode = tborderNode.SelectSingleNode("thead[1]/tr[1]/td[1]/span[1]/label[1]/strong[1]");
                if (messageCountNode == null) break;

                uint messageCount = Convert.ToUInt32(messageCountNode.InnerText);
                if (messageCount == 0)
                    break;
                else 
                    pageCount = (uint) Math.Ceiling((double) messageCount / 100);

                List<HtmlNode> categoryNodes = new List<HtmlNode>(tborderNode.GetElementsByTagName("tbody").Where(node => node.Id != String.Empty));
                foreach (var subNodes in categoryNodes.Select(categoryNode => categoryNode.GetElementsByTagName("tr")))
                {
                    foreach (var subNode in subNodes)
                    {
                        HtmlNode tdBaseNode = subNode.SelectSingleNode("td[3]");
                        if (tdBaseNode == null) continue;
                        uint pmID = Convert.ToUInt32(new string(tdBaseNode.Id.Skip(1).ToArray())); // skip the first character that is always prefixed before the actual id

                        HtmlNode dateNode = tdBaseNode.SelectSingleNode("div[1]/span[1]");
                        string date = (dateNode != null) ? dateNode.InnerText : "";

                        HtmlNode timeNode = tdBaseNode.SelectSingleNode("div[2]/span[1]");
                        string time = (timeNode != null) ? timeNode.InnerText : "";

                        bool messageUnread = false;

                        string title = "";
                        HtmlNode titleNode = tdBaseNode.SelectSingleNode("div[1]/a[1]");
                        if (titleNode == null)
                        {
                            // Unread messages are shown with bold font
                            titleNode = tdBaseNode.SelectSingleNode("div[1]/a[1]/strong[1]");
                            title = (titleNode != null) ? titleNode.InnerText : "";
                            messageUnread = true;
                        }
                        else
                            title = (titleNode != null) ? titleNode.InnerText : "";

                        string userName = "";
                        HtmlNode userNameNode = tdBaseNode.SelectSingleNode("div[2]/span[2]");
                        if (userNameNode == null)
                        {
                            // Unread messages are shown with bold font
                            userNameNode = tdBaseNode.SelectSingleNode("div[2]/strong[1]/span[1]");
                            userName = (userNameNode != null) ? userNameNode.InnerText : "";
                            messageUnread = true;
                        }
                        else
                            userName = (userNameNode != null) ? userNameNode.InnerText : "";

                        DateTime dateTime = new DateTime();
                        DateTime.TryParseExact(date + " " + time, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

                        User sender = new User(userName);
                        Match regexMatch = Regex.Match(userNameNode.Attributes["onclick"].Value, @"window.location='(\S+)';"); // the profile link is stored within a javascript page redirect command
                        if (regexMatch.Groups.Count > 1)
                            sender = new User(userName, User.FromURL(regexMatch.Groups[1].Value));

                        fetchedMessages.Add(new PrivateMessage(pmID, String.Empty, new List<User>() { this }, sender, title, dateTime, messageUnread));
                    }
                }
            }

            return fetchedMessages;
        }


        /// <summary>
        /// Retrieves the profile ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved profile ID </returns>
        public static uint FromURL(string url)
        {
            var match = Regex.Match(url, @"(?:http://www.elitepvpers.com/forum/)?members/(\d+)-\S+.html");
            if(match.Groups.Count < 2) throw new ParsingFailedException("User could not be exported from the given URL");
            
            return Convert.ToUInt32(match.Groups[1].Value);
        }
    }
}
