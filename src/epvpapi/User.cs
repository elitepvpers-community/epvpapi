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
        public class Usergroup
        {
            public string Name { get; set; }
            public string File { get; set; }
            public string Path { get; set; }

            public static Usergroup Premium
            {
                get { return new Usergroup("Premium", "premium.png"); }
            }

            public static Usergroup Level2
            {
                get { return new Usergroup("Level 2", "level2.png"); }
            }

            public static Usergroup Level3
            {
                get { return new Usergroup("Level 3", "level3.png"); }
            }

            public static Usergroup Moderator
            {
                get { return new Usergroup("Moderator", "moderator.png"); }
            }

            public static Usergroup GlobalModerator
            {
                get { return new Usergroup("Global Moderator", "globalmod.png"); }
            }

            public static Usergroup Administrator
            {
                get { return new Usergroup("Administrator", "coadmin.png"); }
            }

            public static Usergroup EliteGoldTrader
            {
                get { return new Usergroup("elite*gold Trader", "egtrader.png"); }
            }

            public static Usergroup FormerVolunteer
            {
                get { return new Usergroup("Former Volunteer", "formervolunteer.png"); }
            }

            public static Usergroup FormerStaff
            {
                get { return new Usergroup("Former Staff", "formerstaff.png"); }
            }

            public static Usergroup Guardian
            {
                get { return new Usergroup("Guardian", "guard.png"); }
            }

            public static Usergroup Translator
            {
                get { return new Usergroup("Translator", "translator.png"); }
            }

            public static Usergroup Editor
            {
                get { return new Usergroup("Editor", "editor.png"); }
            }

            public static Usergroup EventPlanner
            {
                get { return new Usergroup("Event Planner", "eventplanner.png"); }
            }

            public static Usergroup Podcaster
            {
                get { return new Usergroup("Podcaster", "podcaster.png"); }
            }

            public static Usergroup Broadcaster
            {
                get { return new Usergroup("Broadcaster", "broadcaster.png"); }
            }

            public static Usergroup IDVerified
            {
                get { return new Usergroup("ID Verified", "idverified.png"); }
            }

            public static Usergroup Founder
            {
                get { return new Usergroup("Founder", "founder.png"); }
            }

            private static string _DefaultDirectory = "http://cdn.elitepvpers.org/forum/images/teamicons/relaunch/";
            public static string DefaultDirectory
            {
                get { return _DefaultDirectory; }
            }

            public Usergroup(string name = null, string file = null):
                this(name, file, DefaultDirectory + file)
            { }

            public Usergroup(string name, string file, string path)
            {
                Name = name;
                File = file;
                Path = path;
            }

            public static bool FromURL(string url, out Usergroup group)
            {
                group = new Usergroup();
                Match match = Regex.Match(url, @"http://cdn.elitepvpers.org/forum/images/teamicons/relaunch/(\S+)");
                if (match.Groups.Count < 2) return false;

                var availableUsergroups = typeof(Usergroup).GetProperties().Where(property => property.PropertyType == typeof(Usergroup));
                
                foreach(var reflectedUsergroup in availableUsergroups)
                {
                    Usergroup usergroup = (reflectedUsergroup.GetValue(null) as Usergroup);
                    if (usergroup.File == match.Groups[1].Value)
                    {
                        group = usergroup;
                        return true;
                    }
                }

                return false;
            }
        }

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
        /// Total amount of posts the user has written
        /// </summary>
        public uint Posts { get; set; }

        /// <summary>
        /// Average amount of posts written per day
        /// </summary>
        public double PostsPerDay { get; set; }

        /// <summary>
        /// Total amount of <c>VisitorMessage</c>s the user has received
        /// </summary>
        public uint VisitorMessages { get; set; }

        /// <summary>
        /// Date and time of the last <c>VisitorMessage</c> that was received
        /// </summary>
        public DateTime LastVisitorMessage { get; set; }

        /// <summary>
        /// Total amount of user notes the user has
        /// </summary>
        public uint UserNotes { get; set; }

        /// <summary>
        /// Date and time of the last user note entry
        /// </summary>
        public DateTime LastUserNote { get; set; }

        /// <summary>
        /// The associated user blog
        /// </summary>
        public Blog Blog { get; set; }

        /// <summary>
        /// List of usergroups the user has
        /// </summary>
        public List<Usergroup> Groups { get; set; }

        /// <summary>
        /// Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        public string Biography { get; set; }

        /// <summary>
        /// Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        public string Interests { get; set; }

        /// <summary>
        /// Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        public string Occupation { get; set; }

        /// <summary>
        /// Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        public string SteamID { get; set; }

        /// <summary>
        /// Web URL to the profile page
        /// </summary>
        public string URL 
        {
            get { return "http://www.elitepvpers.com/forum/members/" + ID + "-" + Name.ToLower() + ".html"; }
        }

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
            Groups = new List<Usergroup>();
            Namecolor = "black";
            LastVisitorMessage = new DateTime();
        }

        /// <summary>
        /// Updates the user by requesting the profile
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update<T>(UserSession<T> session) where T : User
        {
            session.ThrowIfInvalid();
            Response res = session.Get("http://www.elitepvpers.com/forum/members/" + ID.ToString() + "--.html");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            HtmlNode userNameBoxNode = doc.GetElementbyId("username_box"); // root element
            if (userNameBoxNode == null) throw new ParsingFailedException("Root node is invalid or was not found");
                
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

            HtmlNode userTitleNode = userNameBoxNode.SelectSingleNode("h2[1]");
            Title = (userTitleNode != null) ? userTitleNode.InnerText : String.Empty;

            if(userNameNode.Attributes.Contains("style"))
            {
                Match match = Regex.Match(userNameNode.Attributes["style"].Value, @"color:(\S+)");
                if (match.Groups.Count > 1)
                    Namecolor = match.Groups[1].Value;
            }

            // Fetch the user title badges. User who do not belong to any group or who don't got any badges, will be lacking of the 'rank' element in their profile page
            HtmlNode userRankNode = doc.GetElementbyId("rank");
            if(userRankNode != null)
            {
                List<HtmlNode> rankNodes = new List<HtmlNode>(userRankNode.GetElementsByTagName("img")); // every rank badge got his very own 'img' element

                foreach(var node in rankNodes)
                {
                    if (!node.Attributes.Contains("src")) continue;

                    Usergroup parsedGroup = new Usergroup();
                    if (Usergroup.FromURL(node.Attributes["src"].Value, out parsedGroup)) // 'src' holds the url to the rank image
                        Groups.Add(parsedGroup);
                }
            }

            HtmlNode userStatusNode = userNameBoxNode.SelectSingleNode("h1[1]/img[1]");
            if(userStatusNode != null)
            {
                if(userStatusNode.Attributes.Contains("src"))
                {
                    string userStatusLink = userStatusNode.Attributes["src"].Value;
                    if (userStatusLink.Contains("invisible"))
                        CurrentStatus = Status.Invisible;
                    else if (userStatusLink.Contains("offline"))
                        CurrentStatus = Status.Offline;
                    else if (userStatusLink.Contains("online"))
                        CurrentStatus = Status.Online;
                }
            }
         
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

            
            // Parsing additional information
            // In case the user is the logged in user, all fields are editable and therefore got his own ids. 
            if (this == session.User)
            {
                HtmlNode biographyNode = doc.GetElementbyId("profilefield_value_1");
                Biography = (biographyNode != null) ? biographyNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                HtmlNode locationNode = doc.GetElementbyId("profilefield_value_2");
                Location = (locationNode != null) ? locationNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                HtmlNode interestsNode = doc.GetElementbyId("profilefield_value_3");
                Interests = (interestsNode != null) ? interestsNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                HtmlNode occupationNode = doc.GetElementbyId("profilefield_value_4");
                Occupation = (occupationNode != null) ? occupationNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                HtmlNode steamIDNode = doc.GetElementbyId("profilefield_value_8");
                SteamID = (steamIDNode != null) ? steamIDNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";
            }
            else // otherwise, fields are not owning an id
            {
                HtmlNode aboutMeTabNode = doc.GetElementbyId("collapseobj_aboutme");
                if(aboutMeTabNode != null)
                {
                    HtmlNode profilefieldlistNode = aboutMeTabNode.SelectSingleNode("div[1]/ul[1]/li[1]/dl[1]");
                    if(profilefieldlistNode != null)
                    {
                        List<HtmlNode> fieldNodes = new List<HtmlNode>(profilefieldlistNode.GetElementsByTagName("dt"));
                        List<HtmlNode> valueNodes = new List<HtmlNode>(profilefieldlistNode.GetElementsByTagName("dd"));

                        if(fieldNodes.Count == valueNodes.Count)
                        {
                            foreach(var fieldNode in fieldNodes)
                            {
                                string actualValue = valueNodes.ElementAt(fieldNodes.IndexOf(fieldNode)).InnerText;

                                if (fieldNode.InnerText == "Biography")
                                    Biography = actualValue;
                                else if (fieldNode.InnerText == "Location")
                                    Location = actualValue;
                                else if (fieldNode.InnerText == "Interests")
                                    Interests = actualValue;
                                else if (fieldNode.InnerText == "Occupation")
                                    Occupation = actualValue;
                                else if (fieldNode.InnerText == "Steam ID")
                                    SteamID = actualValue;
                            }
                        }
                    }
                }
            }

            // Statistics
            HtmlNode statisticsRootNode = doc.GetElementbyId("collapseobj_stats");
            if(statisticsRootNode != null)
            {
                statisticsRootNode = statisticsRootNode.SelectSingleNode("div[1]");
                if (statisticsRootNode != null)
                {
                    var postGroupNode = statisticsRootNode.SelectSingleNode("fieldset[1]");
                    if(postGroupNode != null)
                    {
                        var postsNode = postGroupNode.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        Posts = (postsNode != null) ? (uint) Convert.ToDouble(postsNode.InnerText) : 0;

                        var postsPerDayNode = postGroupNode.SelectSingleNode("ul[1]/li[2]/text()[1]");
                        PostsPerDay = (postsPerDayNode != null) ? Convert.ToDouble(postsPerDayNode.InnerText) : 0;
                    }

                    if (session.User.Groups.Any(group => group == Usergroup.Moderator)) // for some reason, visitor messages are only visible for moderators+
                    {
                        HtmlNode visitorMessagesGroupNode = statisticsRootNode.SelectSingleNode("fieldset[2]");

                        if (visitorMessagesGroupNode != null)
                        {
                            var visitorMessagesNode = visitorMessagesGroupNode.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            VisitorMessages = (visitorMessagesNode != null) ? (uint)Convert.ToDouble(visitorMessagesNode.InnerText) : 0;

                            var lastVisitorMessageNode = visitorMessagesGroupNode.SelectSingleNode("ul[1]/li[2]/text()[1]");
                            DateTime lastVisitorMessage = new DateTime();
                            if (lastVisitorMessageNode != null)
                                DateTime.TryParseExact(lastVisitorMessageNode.InnerText, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastVisitorMessage);

                            LastVisitorMessage = lastVisitorMessage;
                        }


                        // usernotes are also only visible for moderators+
                        HtmlNode userNotesGroupNode = statisticsRootNode.SelectSingleNode("fieldset[3]");
                        if(userNotesGroupNode != null)
                        {
                            var userNotesNode = visitorMessagesGroupNode.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            UserNotes = (userNotesNode != null) ? (uint) Convert.ToDouble(userNotesNode.InnerText) : 0;

                            var lastNoteDateNode = visitorMessagesGroupNode.SelectSingleNode("ul[1]/li[2]/text()[1]");
                            var lastNoteTimeNode = visitorMessagesGroupNode.SelectSingleNode("ul[1]/li[2]/span[2]");

                            if(lastNoteDateNode != null && lastNoteTimeNode != null)
                            {
                                DateTime lastUserNote = new DateTime();
                                DateTime.TryParseExact(lastNoteDateNode.InnerText + " " + lastNoteTimeNode.InnerText, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUserNote);
                                LastUserNote = lastUserNote;
                            }
                        }
                    }

                    var thanksGroupNode = statisticsRootNode.SelectSingleNode("fieldset[4]");
                    var blogGroupNode = statisticsRootNode.SelectSingleNode("fieldset[5]");
                    var otherGroupNode = statisticsRootNode.SelectSingleNode("fieldset[6]");
                }
            }

        }


        /// <summary>
        /// Gets all private messages stored in the specified folder
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="folder"> 
        /// The folder where the private messages are stored. Either a pre-defined folder (such as <c>PrivateMessage.Folder.Received</c>
        /// or <c>PrivateMessage.Folder.Sent</c>) can be used or you can specify your own folder you've created by transmitting the folder ID 
        /// </param>
        /// <returns> All private messages that could be retrieved </returns>
        /// <remarks>
        /// Every page contains 100 messages - if available
        /// </remarks>
        public List<PrivateMessage> GetPrivateMessages(Session session, PrivateMessage.Folder folder)
        {
            return GetPrivateMessages(session, 1, 1, folder);
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
        public List<PrivateMessage> GetPrivateMessages(Session session, uint firstPage, uint pageCount, PrivateMessage.Folder folder)
        {
            List<PrivateMessage> fetchedMessages = new List<PrivateMessage>();

            for (int i = 0; i < pageCount; ++i)
            {
                // setting 'pp' to 100 will get you exactly 100 messages per page. This is the highest count that can be set.
                Response res = session.Get("http://www.elitepvpers.com/forum/private.php?folderid=" + folder.ID + "&pp=100&sort=date&page=" + i);
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

                        // Messages that were send are labeled with the user that received the message. If messages were received, they were labeled with the sender
                        // so we need to know wether the folder stores received or sent messages
                        if(folder.StorageType == PrivateMessage.Folder.Storage.Received)
                            fetchedMessages.Add(new PrivateMessage(pmID, String.Empty, new List<User>() { this }, sender, title, dateTime, messageUnread));
                        else
                            fetchedMessages.Add(new PrivateMessage(pmID, String.Empty, new List<User>() { sender }, this, title, dateTime, messageUnread));
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
