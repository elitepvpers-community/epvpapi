using epvpapi.Connection;
using epvpapi.Evaluation;
using epvpapi.TBM;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace epvpapi
{
    /// <summary>
    /// Represents an user in elitepvpers
    /// </summary>
    public class User : UniqueObject, IUpdatable, IInternalUpdatable, IUniqueWebObject
    {
        /// <summary>
        /// Available usergroups an user can have 
        /// </summary>
        public class Rank
        {
            /// <summary>
            /// Name of the Rank
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Filename and extension of the rank badge
            /// </summary>
            public string File { get; set; }

            /// <summary>
            /// Absolute path to the rank badge file
            /// </summary>
            public string Path { get; set; }
            
            /// <summary>
            /// Rights representing privileges that users owning the rank are obtaining
            /// </summary>
            public Rights AccessRights { get; set; }

            [Flags]
            public enum Rights
            {
                /// <summary>
                /// Access to private forums which are not accessible for normal users
                /// </summary>
                PrivateForumAccess = 1,

                /// <summary>
                /// Partial moderation rights for moderating one or more sections
                /// </summary>
                Moderation = 2,
                
                /// <summary>
                /// Global moderation rights for moderating all sections
                /// </summary>
                GlobalModeration = 4,

                /// <summary>
                /// Highest privilege, maintaining the forum 
                /// </summary>
                ForumManagement = 8
            }

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
                get { return new Rank("Global Moderator", "globalmod.png", Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess); }
            }

            public static Rank Administrator
            {
                get { return new Rank("Administrator", "coadmin.png", Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess | Rights.ForumManagement); }
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

            public static Rank IDVerified
            {
                get { return new Rank("ID Verified", "idverified.png"); }
            }

            public static Rank Founder
            {
                get { return new Rank("Founder", "founder.png"); }
            }

            private static string _DefaultDirectory = "http://cdn.elitepvpers.org/forum/images/teamicons/relaunch/";
            public static string DefaultDirectory
            {
                get { return _DefaultDirectory; }
            }

            public Rank(string name = null, string file = null, Rights accessRights = 0) :
                this(name, file, DefaultDirectory + file, accessRights)
            { }

            public Rank(string name, string file, string path, Rights accessRights)
            {
                Name = name;
                File = file;
                Path = path;
                AccessRights = accessRights;
            }

            /// <summary>
            /// Parses an url to a given badge file and returns the associated <c>Rank</c> object matching the file
            /// </summary>
            /// <param name="url"> URL to parse </param>
            /// <param name="group"> <c>Rank</c> object where the parsed result will be stored </param>
            /// <returns> true on success, false on failure </returns>
            public static bool FromUrl(string url, out Rank group)
            {
                group = new Rank();
                var match = Regex.Match(url, @"(?:http|https)+:\/\/(?:cdn|www)+\.elitepvpers\.(?:org|com)+\/forum\/images\/teamicons\/relaunch\/(\S+)");
                if (match.Groups.Count < 2) return false;

                var availableUsergroups = typeof(Rank).GetProperties().Where(property => property.PropertyType == typeof(Rank));
                
                foreach(var reflectedUsergroup in availableUsergroups)
                {
                    var usergroup = (reflectedUsergroup.GetValue(null, null) as Rank);
                    if (usergroup.File == match.Groups[1].Value)
                    {
                        group = usergroup;
                        return true;
                    }
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

            public static bool operator==(Rank lhs, Rank rhs)
            {
                return (lhs.Name == rhs.Name) && (lhs.AccessRights == rhs.AccessRights) && (lhs.File == rhs.File) && (lhs.Path == rhs.Path);
            }

            public static bool operator !=(Rank lhs, Rank rhs)
            {
                return (lhs.Name != rhs.Name) || (lhs.AccessRights != rhs.AccessRights) || (lhs.File != rhs.File) || (lhs.Path != rhs.Path);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            } 

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
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
        /// Date the user joined elitepvpers
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Amount of elite*gold the user got
        /// </summary>
        public int EliteGold { get; set; }

        /// <summary>
        /// Represents the TBM profile including positive, neutral and negative ratings
        /// </summary>
        public Profile TBMProfile { get; set; }

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
        /// Amount of users entered the user's name into the recommandations field on registering
        /// </summary>
        public uint Recommendations { get; set; }

        /// <summary>
        /// Last (visible) users that visited the users profile
        /// </summary>
        public List<User> LastVisitors { get; set; }

        /// <summary>
        /// The associated user blog
        /// </summary>
        public Blog Blog { get; set; }

        /// <summary>
        /// List of ranks the user got
        /// </summary>
        public List<Rank> Ranks { get; set; }

        /// <summary>
        /// Indicates whether the user is banned or not
        /// </summary>
        public bool Banned { get; internal set; }

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
        /// Holds the URL to the avatar the user has set
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Represents the custom signature that the user has set
        /// </summary>
        public Content Signature { get; set; }

        /// <summary>
        /// Amount of friends the user has added
        /// </summary>
        public uint Friends { get; set; }

        /// <summary>
        /// The system user appears in transactions with the id -1
        /// </summary>
        public static User System
        {
            get { return new User(-1); }
        }

        public User(int id = 0)
            : this(null, id)
        { }

        /// <param name="name"> Name of the user </param>
        /// <param name="id"> ID of the user (profile ID)</param>
        public User(string name, int id = 0)
            : base(id)
        {
            Name = name;
            Blog = new Blog(this); // the blog id is equal to the user id since every user can have just one blog which is bound to the user's profile
            LastActivity = new DateTime();
            Ranks = new List<Rank>();
            Namecolor = "black";
            LastVisitorMessage = new DateTime();
            JoinDate = new DateTime();
            TBMProfile = new Profile(id);
            Signature = new Content();
        }

        /// <summary>
        /// Checks if a user got a specific rank
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public bool HasRank(Rank rank)
        {
            return Ranks.Any(userRank => userRank == rank);
        }

        /// <summary>
        /// Gets highest rank or a user
        /// </summary>
        /// <returns> The highest rank object </returns>
        public Rank GetHighestRank()
        {
            var highestRank = new Rank();
            foreach(var rank in Ranks)
                if (rank > highestRank)
                    highestRank = rank;

            return highestRank;
        }

        /// <summary>
        /// Updates the user by requesting the profile
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update<TUser>(AuthenticatedSession<TUser> session) where TUser : User
        {
            if(ID == 0) throw new ArgumentException("User ID must not be 0");
            session.ThrowIfInvalid();
            var res = session.Get(GetUrl());

            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());
            new UserParser(this, (this == session.User)).Execute(doc);
        }

        /// <summary>
        /// Updates the user by requesting the profile
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(GuestSession session)
        {
            if (ID == 0) throw new ArgumentException("User ID must not be 0");
            var res = session.Get(GetUrl());

            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());
            new UserParser(this, false).Execute(doc);
        }

        /// <summary>
        /// Gets the profile url
        /// </summary>
        /// <returns> The profile url of the user </returns>
        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/members/" + ID + "-" + Name.UrlEscape() + ".html";
        } 

        /// <summary>
        /// Retrieves the profile ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved profile ID </returns>
        /// <exception cref="ParsingFailedException"> Thrown if the specified link does not contain the profile ID and/or was malformatted </exception>
        public static int FromUrl(string url)
        {
            var match = Regex.Match(url, @"(?:http://www.elitepvpers.com/(?:forum/)*)*(?:members|theblackmarket/profile)/([0-9]+)(?:-[a-zA-Z]+.html)*");
            if(match.Groups.Count < 2) throw new ParsingFailedException(String.Format("User could not be exported from the given url: {0}", url));

            return match.Groups[1].Value.To<int>();
        }

        /// <summary>
        /// Performs an user lookup request and searches for users with the specified name
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="name"> User name to search for </param>
        /// <returns> List of <c>User</c>s that were found </returns>
        public static IEnumerable<User> Search<TUser>(AuthenticatedSession<TUser> session, string name) where TUser : User
        {
            session.ThrowIfInvalid();
            var res = session.Post("http://www.elitepvpers.com/forum/ajax.php?do=usersearch",
                                    new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                        new KeyValuePair<string, string>("do", "usersearch"),
                                        new KeyValuePair<string, string>("fragment", name)
                                    });

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res.ToString());
            var rootNode = htmlDocument.DocumentNode.SelectSingleNode("users");

            return (rootNode != null)
                    ? (from userNode in rootNode.ChildNodes.GetElementsByTagName("user")
                        where userNode.Attributes.Contains("userid")
                       select new User(userNode.InnerText, userNode.Attributes["userid"].Value.To<int>())).ToList()
                    : new List<User>();
        }

        /// <summary>
        /// Gets an <c>User</c> by the username 
        /// </summary>
        /// <param name="name"> The username of the user that will be looked up </param>
        /// <returns> The user that was found </returns>
        /// <exception cref="EpvpapiException"> Thrown if no user was found matching the specified username </exception>
        public static User ByName<TUser>(AuthenticatedSession<TUser> session, string name) where TUser : User
        {
            var results = epvpapi.User.Search(session, name)
                         .Where(x => x.Name == name)
                         .ToList();

            if (results.Count == 0)
                throw new EpvpapiException(String.Format("No user with the given name '{0}' was found", name));

            return results[0];
        }
    }
}
