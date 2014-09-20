using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using epvpapi.Evaluation;
using epvpapi.TBM;
using HtmlAgilityPack;

namespace epvpapi
{
    /// <summary>
    ///     Represents an user in elitepvpers
    /// </summary>
    public class User : UniqueObject, ISpecializedUpdatable, IUniqueWebObject
    {
        public enum Status
        {
            Online,
            Offline,
            Invisible
        }

        public User(uint id = 0)
            : this(null, id)
        {
        }

        /// <param name="name"> Name of the user </param>
        /// <param name="id"> ID of the user (profile ID)</param>
        public User(string name, uint id = 0)
            : base(id)
        {
            Name = name;
            Blog = new Blog(this);
            // the blog id is equal to the user id since every user can have just one blog which is bound to the user's profile
            LastActivity = new DateTime();
            Ranks = new List<Rank>();
            Namecolor = "black";
            LastVisitorMessage = new DateTime();
            JoinDate = new DateTime();
            TbmProfile = new Profile(id);
        }

        /// <summary>
        ///     Name of the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Color of the name
        /// </summary>
        private string Namecolor { get; set; }

        /// <summary>
        ///     Custom user title displayed beneath the name
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The current status of the user
        /// </summary>
        private Status CurrentStatus { get; set; }

        /// <summary>
        ///     The last activity of the user given is <c>DateTime</c> format
        /// </summary>
        private DateTime LastActivity { get; set; }

        /// <summary>
        ///     Date the user joined elitepvpers
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        ///     Amount of elite*gold the user got
        /// </summary>
        public int EliteGold { private get; set; }

        /// <summary>
        ///     Represents the TBM profile including positive, neutral and negative ratings
        /// </summary>
        public Profile TbmProfile { get; private set; }

        /// <summary>
        ///     Amount of thanks the user has given
        /// </summary>
        private uint ThanksGiven { get; set; }

        /// <summary>
        ///     Amount of thanks the user has received
        /// </summary>
        public uint ThanksReceived { get; set; }

        /// <summary>
        ///     Total amount of posts the user has written
        /// </summary>
        public uint Posts { get; set; }

        /// <summary>
        ///     Average amount of posts written per day
        /// </summary>
        private double PostsPerDay { get; set; }

        /// <summary>
        ///     Total amount of <c>VisitorMessage</c>s the user has received
        /// </summary>
        private uint VisitorMessages { get; set; }

        /// <summary>
        ///     Date and time of the last <c>VisitorMessage</c> that was received
        /// </summary>
        private DateTime LastVisitorMessage { get; set; }

        /// <summary>
        ///     Total amount of user notes the user has
        /// </summary>
        private uint UserNotes { get; set; }

        /// <summary>
        ///     Date and time of the last user note entry
        /// </summary>
        private DateTime LastUserNote { get; set; }

        /// <summary>
        ///     Amount of users entered the user's name into the recommandations field on registering
        /// </summary>
        private uint Recommendations { get; set; }

        /// <summary>
        ///     Last (visible) users that visited the users profile
        /// </summary>
        private List<User> LastVisitors { get; set; }

        /// <summary>
        ///     The associated user blog
        /// </summary>
        private Blog Blog { get; set; }

        /// <summary>
        ///     List of ranks the user got
        /// </summary>
        private List<Rank> Ranks { get; set; }

        /// <summary>
        ///     Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        private string Biography { get; set; }

        /// <summary>
        ///     Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        private string Location { get; set; }

        /// <summary>
        ///     Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        private string Interests { get; set; }

        /// <summary>
        ///     Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        private string Occupation { get; set; }

        /// <summary>
        ///     Additional information that can be entered by the user. Listed under tab 'About'
        /// </summary>
        private string SteamId { get; set; }

        /// <summary>
        ///     Holds the URL to the avatar the user has set
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        ///     Updates the user by requesting the profile
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update<T>(ProfileSession<T> session) where T : User
        {
            session.ThrowIfInvalid();
            Response res = session.Get("http://www.elitepvpers.com/forum/members/" + Id + "--.html");

            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            new GeneralInfoParser(this).Execute(doc.GetElementbyId("username_box"));
            new LastActivityParser(this).Execute(doc.GetElementbyId("last_online"));

            // In case the user is the logged in user, all fields are editable and therefore got his own ids. 
            if (this == session.User)
                new SessionUserAboutParser(session.User).Execute(doc);
            else // otherwise, fields are not owning an id
                new AboutParser(session.User).Execute(doc.GetElementbyId("collapseobj_aboutme"));

            new RankParser(this).Execute(doc.GetElementbyId("rank"));
            new StatisticsParser(this, (this == session.User)).Execute(doc.GetElementbyId("collapseobj_stats"));
            new MiniStatsParser(this).Execute(doc.GetElementbyId("collapseobj_stats_mini"));
            new LastVisitorsParser(this).Execute(doc.GetElementbyId("collapseobj_visitors"));
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/members/" + Id + "-" + Name.UrlEscape() + ".html";
        }


        public bool HasRank(Rank rank)
        {
            return Ranks.Any(userRank => userRank == rank);
        }

        public Rank GetHighestRank()
        {
            Rank[] highestRank = {new Rank()};
            foreach (Rank rank in Ranks.Where(rank => rank > highestRank[0]))
            {
                highestRank[0] = rank;
            }

            return highestRank[0];
        }

        /// <summary>
        ///     Retrieves the profile ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved profile ID </returns>
        public static uint FromUrl(string url)
        {
            Match match = Regex.Match(url,
                @"(?:http://www.elitepvpers.com/(?:forum/)*)*(?:members|theblackmarket/profile)/([0-9]+)(?:-[a-zA-Z]+.html)*");
            if (match.Groups.Count < 2)
                throw new ParsingFailedException("User could not be exported from the given URL");

            return Convert.ToUInt32(match.Groups[1].Value);
        }

        /// <summary>
        ///     Performs an user lookup request and searches for users with the specified name
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="name"> User name to search for </param>
        /// <returns> List of <c>User</c>s that were found </returns>
        public static List<User> Search(Session session, string name)
        {
            Response res = session.Post("http://www.elitepvpers.com/forum/ajax.php?do=usersearch",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "usersearch"),
                    new KeyValuePair<string, string>("fragment", name)
                });

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res.ToString());
            HtmlNode rootNode = htmlDocument.DocumentNode.SelectSingleNode("users");

            return (rootNode != null)
                ? (from userNode in rootNode.GetElementsByTagName("user")
                    where userNode.Attributes.Contains("userid")
                    select new User(userNode.InnerText, Convert.ToUInt32(userNode.Attributes["userid"].Value))).ToList()
                : new List<User>();
        }


        private class AboutParser : TargetableParser<User>, INodeParser
        {
            public AboutParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                HtmlNode profilefieldlistNode = coreNode.SelectSingleNode("div[1]/ul[1]/li[1]/dl[1]");
                if (profilefieldlistNode == null) return;

                var fieldNodes = new List<HtmlNode>(profilefieldlistNode.GetElementsByTagName("dt"));
                var valueNodes = new List<HtmlNode>(profilefieldlistNode.GetElementsByTagName("dd"));

                if (fieldNodes.Count != valueNodes.Count) return;
                foreach (HtmlNode fieldNode in fieldNodes)
                {
                    string actualValue = valueNodes.ElementAt(fieldNodes.IndexOf(fieldNode)).InnerText;

                    switch (fieldNode.InnerText)
                    {
                        case "Biography":
                            Target.Biography = actualValue;
                            break;
                        case "Location":
                            Target.Location = actualValue;
                            break;
                        case "Interests":
                            Target.Interests = actualValue;
                            break;
                        case "Occupation":
                            Target.Occupation = actualValue;
                            break;
                        case "Steam ID":
                            Target.SteamId = actualValue;
                            break;
                    }
                }
            }
        }


        private class GeneralInfoParser : TargetableParser<User>, INodeParser
        {
            public GeneralInfoParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                HtmlNode userNameNode = coreNode.SelectSingleNode("h1[1]/span[1]");
                if (userNameNode != null)
                {
                    Target.Name = userNameNode.InnerText.Strip();
                }
                else
                {
                    // In case the user has no special color, the <span> element will be missing and no attributes are used
                    userNameNode = coreNode.SelectSingleNode("h1[1]");
                    Target.Name = (userNameNode != null) ? userNameNode.InnerText.Strip() : String.Empty;
                }

                HtmlNode userTitleNode = coreNode.SelectSingleNode("h2[1]");
                Target.Title = (userTitleNode != null) ? userTitleNode.InnerText : String.Empty;

                if (userNameNode.Attributes.Contains("style"))
                {
                    Match match = Regex.Match(userNameNode.Attributes["style"].Value, @"color:(\S+)");
                    if (match.Groups.Count > 1)
                        Target.Namecolor = match.Groups[1].Value;
                }

                HtmlNode userStatusNode = coreNode.SelectSingleNode("h1[1]/img[1]");
                if (userStatusNode == null) return;
                if (!userStatusNode.Attributes.Contains("src")) return;
                string userStatusLink = userStatusNode.Attributes["src"].Value;
                if (userStatusLink.Contains("invisible"))
                    Target.CurrentStatus = Status.Invisible;
                else if (userStatusLink.Contains("offline"))
                    Target.CurrentStatus = Status.Offline;
                else if (userStatusLink.Contains("online"))
                    Target.CurrentStatus = Status.Online;
            }
        }

        private class LastActivityParser : TargetableParser<User>, INodeParser
        {
            public LastActivityParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode != null)
                {
                    HtmlNode lastActivityDateNode = coreNode.SelectSingleNode("text()[2]");
                    string date = (lastActivityDateNode != null) ? lastActivityDateNode.InnerText.Strip() : String.Empty;

                    HtmlNode lastActivityTimeNode = coreNode.SelectSingleNode("span[2]");
                    string time = (lastActivityTimeNode != null) ? lastActivityTimeNode.InnerText.Strip() : String.Empty;

                    Target.LastActivity = (date + " " + time).ToElitepvpersDateTime();
                }
                else
                {
                    Target.CurrentStatus = Status.Invisible;
                }
            }
        }

        private class LastVisitorsParser : TargetableParser<User>, INodeParser
        {
            public LastVisitorsParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]/ol[1]");
                if (coreNode == null) return;

                Target.LastVisitors = new List<User>();
                foreach (HtmlNode visitorNode in coreNode.GetElementsByTagName("li"))
                {
                    HtmlNode profileLinkNode = visitorNode.SelectSingleNode("a[1]");
                    if (profileLinkNode == null) continue;
                    string profileLink = (profileLinkNode.Attributes.Contains("href"))
                        ? profileLinkNode.Attributes["href"].Value
                        : "";

                    HtmlNode userNameNode = profileLinkNode.SelectSingleNode("span[1]") ?? profileLinkNode;

                    Target.LastVisitors.Add(new User(userNameNode.InnerText, FromUrl(profileLink)));
                }
            }
        }

        private class MiniStatsParser : TargetableParser<User>, INodeParser
        {
            public MiniStatsParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]/table[1]/tr[1]");
                if (coreNode == null) return;

                HtmlNode fieldsRootNode = coreNode.SelectSingleNode("td[1]/dl[1]");
                if (fieldsRootNode == null) return;

                var miniStatsNodes = new List<HtmlNode>(fieldsRootNode.GetElementsByTagName("dt"));
                var miniStatsValueNodes = new List<HtmlNode>(fieldsRootNode.GetElementsByTagName("dd"));

                if (miniStatsNodes.Count != miniStatsValueNodes.Count) return;

                // loop through the key nodes since they can also occur occasionally (depends on what the user selects to be shown in the profile and/or the rank)
                foreach (HtmlNode keyNode in miniStatsNodes)
                {
                    HtmlNode valueNode = miniStatsValueNodes[miniStatsNodes.IndexOf(keyNode)];
                    if (valueNode == null) continue;

                    if (keyNode.InnerText == "Join Date")
                    {
                        Target.JoinDate = valueNode.InnerText.ToElitepvpersDateTime();
                    }
                    else if (keyNode.InnerText.Contains("elite*gold"))
                    {
                        HtmlNode eliteGoldValueNode = valueNode.SelectSingleNode("text()[1]");
                        Target.EliteGold = (eliteGoldValueNode != null)
                            ? Convert.ToInt32(eliteGoldValueNode.InnerText)
                            : Target.EliteGold;
                    }
                    else if (keyNode.InnerText.Contains("The Black Market"))
                    {
                        HtmlNode positiveRatingsNode = valueNode.SelectSingleNode("span[1]");
                        Target.TbmProfile.Ratings.Positive = (positiveRatingsNode != null)
                            ? Convert.ToUInt32(positiveRatingsNode.InnerText)
                            : Target.TbmProfile.Ratings.Positive;

                        HtmlNode neutralRatingsNode = valueNode.SelectSingleNode("text()[1]");
                        Target.TbmProfile.Ratings.Neutral = (neutralRatingsNode != null)
                            ? Convert.ToUInt32(new string(neutralRatingsNode.InnerText.Skip(1).Take(1).ToArray()))
                            : Target.TbmProfile.Ratings.Neutral;

                        HtmlNode negativeRatingsNode = valueNode.SelectSingleNode("span[2]");
                        Target.TbmProfile.Ratings.Negative = (negativeRatingsNode != null)
                            ? Convert.ToUInt32(negativeRatingsNode.InnerText)
                            : Target.TbmProfile.Ratings.Negative;
                    }
                    else if (keyNode.InnerText.Contains("Mediations"))
                    {
                        HtmlNode positiveNode = valueNode.SelectSingleNode("span[1]");
                        Target.TbmProfile.Mediations.Positive = (positiveNode != null)
                            ? Convert.ToUInt32(positiveNode.InnerText)
                            : 0;

                        HtmlNode neutralNode = valueNode.SelectSingleNode("text()[1]");
                        Target.TbmProfile.Mediations.Neutral = (neutralNode != null)
                            ? Convert.ToUInt32(neutralNode.InnerText.TrimStart('/'))
                            : 0;
                    }
                }

                HtmlNode avatarNode = coreNode.SelectSingleNode("td[2]/img[1]");
                Target.AvatarUrl = (avatarNode != null)
                    ? avatarNode.Attributes.Contains("src") ? avatarNode.Attributes["src"].Value : ""
                    : "";
            }
        }

        /// <summary>
        ///     Available usergroups an user can have
        /// </summary>
        public class Rank
        {
            [Flags]
            public enum Rights
            {
                /// <summary>
                ///     Access to private forums which are not accessible for normal users
                /// </summary>
                PrivateForumAccess = 1,

                /// <summary>
                ///     Partial moderation rights for moderating one or more sections
                /// </summary>
                Moderation = 2,

                /// <summary>
                ///     Global moderation rights for moderating all sections
                /// </summary>
                GlobalModeration = 4,

                /// <summary>
                ///     Highest privilege, maintaining the forum
                /// </summary>
                ForumManagement = 8
            }

            private const string _DefaultDirectory = "http://cdn.elitepvpers.org/forum/images/teamicons/relaunch/";

            public Rank(string name = null, string file = null, Rights accessRights = 0) :
                this(name, file, DefaultDirectory + file, accessRights)
            {
            }

            private Rank(string name, string file, string path, Rights accessRights)
            {
                Name = name;
                File = file;
                Path = path;
                AccessRights = accessRights;
            }

            /// <summary>
            ///     Name of the Rank
            /// </summary>
            private string Name { get; set; }

            /// <summary>
            ///     Filename and extension of the rank badge
            /// </summary>
            private string File { get; set; }

            /// <summary>
            ///     Absolute path to the rank badge file
            /// </summary>
            private string Path { get; set; }

            /// <summary>
            ///     Rights representing privileges that users owning the rank are obtaining
            /// </summary>
            private Rights AccessRights { get; set; }

            public static Rank Premium
            {
                get { return new Rank("Premium", "premium.png", Rights.PrivateForumAccess); }
            }

            public static Rank Level2
            {
                get { return new Rank("Level 2", "level2.png", Rights.PrivateForumAccess); }
            }

            public static Rank Level3
            {
                get { return new Rank("Level 3", "level3.png", Rights.PrivateForumAccess); }
            }

            public static Rank Moderator
            {
                get { return new Rank("Moderator", "moderator.png", Rights.Moderation | Rights.PrivateForumAccess); }
            }

            public static Rank GlobalModerator
            {
                get
                {
                    return new Rank("Global Moderator", "globalmod.png",
                        Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess);
                }
            }

            public static Rank Administrator
            {
                get
                {
                    return new Rank("Administrator", "coadmin.png",
                        Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess | Rights.ForumManagement);
                }
            }

            public static Rank EliteGoldTrader
            {
                get { return new Rank("elite*gold Trader", "egtrader.png"); }
            }

            public static Rank FormerVolunteer
            {
                get { return new Rank("Former Volunteer", "formervolunteer.png"); }
            }

            public static Rank FormerStaff
            {
                get { return new Rank("Former Staff", "formerstaff.png"); }
            }

            public static Rank Guardian
            {
                get { return new Rank("Guardian", "guard.png"); }
            }

            public static Rank Translator
            {
                get { return new Rank("Translator", "translator.png", Rights.PrivateForumAccess); }
            }

            public static Rank Editor
            {
                get { return new Rank("Editor", "editor.png", Rights.PrivateForumAccess); }
            }

            public static Rank EventPlanner
            {
                get { return new Rank("Event Planner", "eventplanner.png", Rights.PrivateForumAccess); }
            }

            public static Rank Podcaster
            {
                get { return new Rank("Podcaster", "podcaster.png", Rights.PrivateForumAccess); }
            }

            public static Rank Broadcaster
            {
                get { return new Rank("Broadcaster", "broadcaster.png", Rights.PrivateForumAccess); }
            }

            public static Rank IdVerified
            {
                get { return new Rank("ID Verified", "idverified.png"); }
            }

            public static Rank Founder
            {
                get { return new Rank("Founder", "founder.png"); }
            }

            private static string DefaultDirectory
            {
                get { return _DefaultDirectory; }
            }

            private bool Equals(Rank other)
            {
                return string.Equals(Name, other.Name) && string.Equals(File, other.File) &&
                       string.Equals(Path, other.Path) && AccessRights == other.AccessRights;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Rank) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (File != null ? File.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Path != null ? Path.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (int) AccessRights;
                    return hashCode;
                }
            }

            /// <summary>
            ///     Parses an url to a given badge file and returns the associated <c>Rank</c> object matching the file
            /// </summary>
            /// <param name="url"> URL to parse </param>
            /// <param name="group"> <c>Rank</c> object where the parsed result will be stored </param>
            /// <returns> true on success, false on failure </returns>
            public static bool FromURL(string url, out Rank group)
            {
                group = new Rank();
                Match match = Regex.Match(url, @"http://cdn.elitepvpers.org/forum/images/teamicons/relaunch/(\S+)");
                if (match.Groups.Count < 2) return false;

                IEnumerable<PropertyInfo> availableUsergroups =
                    typeof (Rank).GetProperties().Where(property => property.PropertyType == typeof (Rank));

                foreach (
                    Rank usergroup in
                        availableUsergroups.Select(reflectedUsergroup => (reflectedUsergroup.GetValue(null) as Rank))
                            .Where(usergroup => usergroup.File == match.Groups[1].Value))
                {
                    @group = usergroup;
                    return true;
                }

                return false;
            }

            public static bool operator >(Rank lhs, Rank rhs)
            {
                return lhs.AccessRights > rhs.AccessRights;
            }

            public static bool operator <(Rank lhs, Rank rhs)
            {
                return lhs.AccessRights < rhs.AccessRights;
            }

            public static bool operator ==(Rank lhs, Rank rhs)
            {
                return (lhs.Name == rhs.Name) && (lhs.AccessRights == rhs.AccessRights) && (lhs.File == rhs.File) &&
                       (lhs.Path == rhs.Path);
            }

            public static bool operator !=(Rank lhs, Rank rhs)
            {
                return (lhs.Name != rhs.Name) || (lhs.AccessRights != rhs.AccessRights) || (lhs.File != rhs.File) ||
                       (lhs.Path != rhs.Path);
            }
        }

        public class RankParser : TargetableParser<User>, INodeParser
        {
            public RankParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                // Fetch the user title badges. User which do not belong to any group or do not have got any badges will be lacking of the 'rank' element in their profile page
                if (coreNode == null) return;

                var rankNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("img"));
                // every rank badge got his very own 'img' element

                foreach (HtmlNode node in rankNodes)
                {
                    if (!node.Attributes.Contains("src")) continue;
                    var parsedRank = new Rank();
                    if (Rank.FromURL(node.Attributes["src"].Value, out parsedRank)) Target.Ranks.Add(parsedRank);
                }
            }
        }

        private class SessionUserAboutParser : TargetableParser<User>, IDocumentParser
        {
            public SessionUserAboutParser(User target) : base(target)
            {
            }

            public void Execute(HtmlDocument document)
            {
                HtmlNode biographyNode = document.GetElementbyId("profilefield_value_1");
                Target.Biography = (biographyNode != null)
                    ? biographyNode.SelectSingleNode("text()[1]").InnerText.Strip()
                    : "";

                HtmlNode locationNode = document.GetElementbyId("profilefield_value_2");
                Target.Location = (locationNode != null)
                    ? locationNode.SelectSingleNode("text()[1]").InnerText.Strip()
                    : "";

                HtmlNode interestsNode = document.GetElementbyId("profilefield_value_3");
                Target.Interests = (interestsNode != null)
                    ? interestsNode.SelectSingleNode("text()[1]").InnerText.Strip()
                    : "";

                HtmlNode occupationNode = document.GetElementbyId("profilefield_value_4");
                Target.Occupation = (occupationNode != null)
                    ? occupationNode.SelectSingleNode("text()[1]").InnerText.Strip()
                    : "";

                HtmlNode steamIdNode = document.GetElementbyId("profilefield_value_8");
                Target.SteamId = (steamIdNode != null)
                    ? steamIdNode.SelectSingleNode("text()[1]").InnerText.Strip()
                    : "";
            }
        }

        private class StatisticsParser : TargetableParser<User>, INodeParser
        {
            public StatisticsParser(User target, bool isSessionUser = false) : base(target)
            {
                IsSessionUser = isSessionUser;
            }

            private bool IsSessionUser { get; set; }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]");

                // Loop through the fields since vBulletin sorts them dynamically according to rank and certain user settings
                foreach (HtmlNode statisticsGroup in coreNode.GetElementsByTagName("fieldset"))
                {
                    string legendCaption = statisticsGroup.SelectSingleNode("legend[1]").InnerText;

                    switch (legendCaption)
                    {
                        case "Total Posts":
                        {
                            HtmlNode postsNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            Target.Posts = (postsNode != null) ? (uint) Convert.ToDouble(postsNode.InnerText) : 0;

                            HtmlNode postsPerDayNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                            Target.PostsPerDay = (postsPerDayNode != null)
                                ? Convert.ToDouble(postsPerDayNode.InnerText)
                                : 0;
                        }
                            break;
                        case "Visitor Messages":
                        {
                            HtmlNode visitorMessagesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            Target.VisitorMessages = (visitorMessagesNode != null)
                                ? (uint) Convert.ToDouble(visitorMessagesNode.InnerText)
                                : 0;

                            HtmlNode lastVisitorMessageNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                            Target.LastVisitorMessage = (lastVisitorMessageNode != null)
                                ? lastVisitorMessageNode.InnerText.ToElitepvpersDateTime()
                                : new DateTime();
                        }
                            break;
                        case "Thanks Given":
                        {
                            HtmlNode givenThanksNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            Target.ThanksGiven = (givenThanksNode != null)
                                ? (uint) Convert.ToDouble(givenThanksNode.InnerText)
                                : 0;

                            // The received thanks count is stored within the span element and is trailed after the language dependent definition.
                            // Unlike other elements, the count is not seperated and therefore needs some regex in order to extract the count
                            HtmlNode thanksReceivedNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[1]");
                            if (thanksReceivedNode != null)
                            {
                                Match match = Regex.Match(thanksReceivedNode.InnerText, @"\S+\s*([0-9.]+)");
                                // language independent
                                if (match.Groups.Count > 1)
                                    Target.ThanksReceived = (uint) Convert.ToDouble(match.Groups[1].Value);
                            }
                        }
                            break;
                        case "General Information":
                        {
                            HtmlNode recommendationsNode;
                            if (Target.CurrentStatus != Status.Invisible || IsSessionUser)
                                recommendationsNode = statisticsGroup.SelectSingleNode("ul[1]/li[3]/text()[1]");
                            else
                                recommendationsNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");

                            Target.Recommendations = (recommendationsNode != null)
                                ? Convert.ToUInt32(recommendationsNode.InnerText)
                                : 0;
                        }
                            break;
                        case "User Notes":
                        {
                            HtmlNode userNotesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                            Target.UserNotes = (userNotesNode != null)
                                ? (uint) Convert.ToDouble(userNotesNode.InnerText)
                                : 0;

                            HtmlNode lastNoteDateNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                            HtmlNode lastNoteTimeNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[2]");

                            if (lastNoteDateNode != null && lastNoteTimeNode != null)
                                Target.LastUserNote =
                                    (lastNoteDateNode.InnerText + " " + lastNoteTimeNode.InnerText)
                                        .ToElitepvpersDateTime();
                        }
                            break;
                        default:
                            if (legendCaption.Contains("Blog -"))
                                // users can specify their own blog name that is trailed behind the 'Blog -' string
                            {
                                HtmlNode blogEntriesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                                // skip the first 2 characters since the value always contains a leading ':' and whitespace 
                                Target.Blog.Entries =
                                    new List<Blog.Entry>((blogEntriesNode != null)
                                        ? Convert.ToInt32(new string(blogEntriesNode.InnerText.Skip(2).ToArray()))
                                        : 0);

                                HtmlNode lastEntryDateNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[2]");
                                string date = (lastEntryDateNode != null) ? lastEntryDateNode.InnerText.Strip() : "";

                                HtmlNode lastEntryTimeNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[2]");
                                string time = (lastEntryTimeNode != null) ? lastEntryTimeNode.InnerText.Strip() : "";

                                Target.Blog.LastEntry = (date + " " + time).ToElitepvpersDateTime();
                            }
                            break;
                    }
                }
            }
        }
    }
}