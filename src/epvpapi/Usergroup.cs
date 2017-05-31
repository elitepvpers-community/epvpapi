using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace epvpapi
{
    /// <summary>
    /// Available usergroups an user can have 
    /// </summary>
    public class Usergroup
    {
        /// <summary>
        /// Name of the Usergroup
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

        public static Usergroup Premium
        {
            get { return new Usergroup("Premium", "premium.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Level2
        {
            get { return new Usergroup("Level 2", "level2.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Level3
        {
            get { return new Usergroup("Level 3", "level3.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Moderator
        {
            get { return new Usergroup("Moderator", "moderator.png", Rights.Moderation | Rights.PrivateForumAccess); }
        }

        public static Usergroup GlobalModerator
        {
            get { return new Usergroup("Global Moderator", "globalmod.png", Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess); }
        }

        public static Usergroup Administrator
        {
            get { return new Usergroup("Administrator", "coadmin.png", Rights.Moderation | Rights.GlobalModeration | Rights.PrivateForumAccess | Rights.ForumManagement); }
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
            get { return new Usergroup("Translator", "translator.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Editor
        {
            get { return new Usergroup("Editor", "editor.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup EventPlanner
        {
            get { return new Usergroup("Event Planner", "eventplanner.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Podcaster
        {
            get { return new Usergroup("Podcaster", "podcaster.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup Broadcaster
        {
            get { return new Usergroup("Broadcaster", "broadcaster.png", Rights.PrivateForumAccess); }
        }

        public static Usergroup IDVerified
        {
            get { return new Usergroup("ID Verified", "idverified.png"); }
        }

        public static Usergroup Founder
        {
            get { return new Usergroup("Founder", "founder.png"); }
        }

        private const string _DefaultDirectory = "https://cdn.elitepvpers.org/forum/images/teamicons/relaunch/";

        public static string DefaultDirectory
        {
            get { return _DefaultDirectory; }
        }

        public Usergroup(string name = null, string file = null, Rights accessRights = 0) :
            this(name, file, DefaultDirectory + file, accessRights)
        { }

        public Usergroup(string name, string file, string path, Rights accessRights)
        {
            Name = name;
            File = file;
            Path = path;
            AccessRights = accessRights;
        }

        /// <summary>
        /// Parses an url to a given badge file and returns the associated <c>Usergroup</c> object matching the file
        /// </summary>
        /// <param name="url"> URL to parse </param>
        /// <param name="group"> <c>Usergroup</c> object where the parsed result will be stored </param>
        /// <returns> true on success, false on failure </returns>
        public static bool FromUrl(string url, out Usergroup group)
        {
            group = new Usergroup();
            var match = Regex.Match(url, @"(?:http|https)+:\/\/(?:cdn|www)+\.elitepvpers\.(?:org|com)+\/forum\/images\/teamicons\/relaunch\/(\S+)");
            if (match.Groups.Count < 2) return false;

            var availableUsergroups = typeof(Usergroup).GetProperties().Where(property => property.PropertyType == typeof(Usergroup));

            foreach (var reflectedUsergroup in availableUsergroups)
            {
                var usergroup = (reflectedUsergroup.GetValue(null, null) as Usergroup);
                if (usergroup.File == match.Groups[1].Value)
                {
                    group = usergroup;
                    return true;
                }
            }

            return false;
        }

        public static bool operator >(Usergroup lhs, Usergroup rhs)
        {
            return lhs.AccessRights > rhs.AccessRights;
        }

        public static bool operator <(Usergroup lhs, Usergroup rhs)
        {
            return lhs.AccessRights < rhs.AccessRights;
        }

        public static bool operator ==(Usergroup lhs, Usergroup rhs)
        {
            return (lhs.Name == rhs.Name) && (lhs.AccessRights == rhs.AccessRights) && (lhs.File == rhs.File) && (lhs.Path == rhs.Path);
        }

        public static bool operator !=(Usergroup lhs, Usergroup rhs)
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
}
