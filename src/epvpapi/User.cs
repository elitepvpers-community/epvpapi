using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// Indicates the last activity of the user
        /// </summary>
        public Activity LastActivity { get; set; }


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
            LastActivity = new Activity();
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
                LastActivity.Day = (lastActivityDateNode != null) ? lastActivityDateNode.InnerText.Strip() : String.Empty;

                HtmlNode lastActivityTimeNode = lastActivityNode.SelectSingleNode("span[2]");
                LastActivity.Time = (lastActivityTimeNode != null) ? lastActivityTimeNode.InnerText.Strip() : String.Empty;
            }
        }
    }
}
