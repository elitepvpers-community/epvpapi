using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents the shoutbox accessable by premium users, level2 + level3 users and the staff
    /// </summary>
    public static class Shoutbox
    {
        /// <summary>
        /// Themed chat-channel of the shoutbox where messages can be stored, send and received. 
        /// </summary>
        public class Channel
        {
            /// <summary>
            /// A single shout send by an user
            /// </summary>
            public class Shout
            {
                public User User { get; set; }
                public string Message { get; set; }
                public DateTime Time { get; set; }

                public Shout(User user, string message, DateTime time)
                {
                    User = user;
                    Message = message;
                    Time = time;
                }
            }

            public uint ID { get; set; }
            public string Name { get; set; }

            /// <summary>
            /// List of the most recent shouts available in the channel, updated on executing the <c>Update</c> function
            /// </summary>
            public List<Shout> Shouts { get; set; }

            public Channel(uint id, string name)
            {
                ID = id;
                Name = name;
            }

            /// <summary>
            /// Sends a message to the channel
            /// </summary>
            /// <param name="session"> Session used for sending the request </param>
            /// <param name="message"> The message text to send </param>
            public void Send(Session session, string message)
            {
                session.ThrowIfInvalid();

                Response res = session.Post("http://www.elitepvpers.com/forum/mgc_cb_evo_ajax.php",
                                            new List<KeyValuePair<string, string>>()
                                            {
                                                new KeyValuePair<string, string>("do", "ajax_chat"),
                                                new KeyValuePair<string, string>("channel_id", ID.ToString()),
                                                new KeyValuePair<string, string>("chat", message),
                                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                                new KeyValuePair<string, string>("s", String.Empty)
                                            });
            }

            /// <summary>
            /// Updates the most recent shouts usually displayed when loading the main page 
            /// </summary>
            /// <param name="session"> Session used for sending the request </param>
            public void Update(Session session)
            {
                Response res = session.Post("http://www.elitepvpers.com/forum/mgc_cb_evo_ajax.php",
                                            new List<KeyValuePair<string, string>>
                                            {
                                                new KeyValuePair<string, string>("do", "ajax_refresh_chat"),
                                                new KeyValuePair<string, string>("status", "open"),
                                                new KeyValuePair<string, string>("channel_id", ID.ToString()),
                                                new KeyValuePair<string, string>("location", "inc"),
                                                new KeyValuePair<string, string>("first_load", "0"),
                                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken), // for some reason, the security token is send twice
                                                new KeyValuePair<string, string>("s", String.Empty),
                                            });

                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(res.ToString());

                    List<HtmlNode> tdNodes = new List<HtmlNode>(doc.DocumentNode.GetElementsByTagName("td"));
                    List<HtmlNode> shoutboxNodes = new List<HtmlNode>(tdNodes.Where(node => node.Attributes.Any(attribute => attribute.Name == "valign" && attribute.Value == "top")));

                    List<List<HtmlNode>> shoutboxNodeGroups = shoutboxNodes.Split(3);

                    Shouts = new List<Shout>();
                    foreach(var shoutboxNodeGroup in shoutboxNodeGroups)
                    {
                        if (shoutboxNodeGroup.Count != 3) continue; // every node group needs to have exactly 3 nodes in order to be valid

                        DateTime time = new DateTime();
                        HtmlNode timetdNode = shoutboxNodeGroup.ElementAt(0);
                        if (timetdNode != null)
                            if (timetdNode.ChildNodes.ElementAt(1) != null)
                                if (timetdNode.ChildNodes.ElementAt(1).FirstChild != null)
                                    if (timetdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling != null)
                                        if (timetdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling.FirstChild != null)
                                        {
                                            Match match = new Regex(@"\s*(\S+)&nbsp;").Match(timetdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling.FirstChild.InnerText);
                                            string matchedTime = match.Groups.Count > 1 ? match.Groups[1].Value : String.Empty;
                                            DateTime.TryParse(matchedTime, out time);
                                        }

                        string username = "";
                        HtmlNode userNametdNode = shoutboxNodeGroup.ElementAt(1);
                        if(userNametdNode != null)
                            if(userNametdNode.ChildNodes.ElementAt(1) != null)
                                if(userNametdNode.ChildNodes.ElementAt(1).FirstChild != null)
                                    if(userNametdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling != null)
                                        if(userNametdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling.FirstChild != null)
                                            if(userNametdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling.FirstChild.NextSibling != null)
                                                username = userNametdNode.ChildNodes.ElementAt(1).FirstChild.NextSibling.FirstChild.NextSibling.InnerText;

                        string message = "";
                        HtmlNode messagetdNode = shoutboxNodeGroup.ElementAt(2);
                        if (messagetdNode != null)
                            if (messagetdNode.ChildNodes.ElementAt(1) != null)
                                message = messagetdNode.ChildNodes.ElementAt(1).InnerText;

                        Shouts.Add(new Shout(new User(username), message, time));
                    }
                }
                catch (HtmlWebException exception)
                {
                    throw new ParsingFailedException("Parsing recent shouts from response content failed", exception);
                }
            }

            /// <summary>
            /// Fetches the history of the specified shoutbox channel and returns all shouts that have been stored
            /// </summary>
            /// <param name="firstPage"> Index of the first page to fetch </param>
            /// <param name="pageCount"> Amount of pages to get. The higher this count, the more data will be generated and received </param>
            /// <returns></returns>
            public static List<Shout> History(uint pageCount = 10, uint firstPage = 1)
            {
                throw new NotImplementedException();
            }

        };


        /// <summary>
        /// Contains the Top 10 chatters of all channels
        /// </summary>
        public static List<User> TopChatter { get; set; }

        /// <summary>
        /// Amount of messages stored in all shoutbox channels
        /// </summary>
        public static uint MessageCount { get; set; }

        /// <summary>
        /// Amount of messages stored within the last 24 hours in all shoutbox channels
        /// </summary>
        public static uint MessageCountCurrentDay { get; set; }

        private static Channel _Global = new Channel(0, "General");
        public static Channel Global
        {
            get {  return _Global; }
            set { _Global = value; }
        }

        private static Channel _EnglishOnly = new Channel(1, "EnglishOnly");
        public static Channel EnglishOnly
        {
            get { return _EnglishOnly; }
            set { _EnglishOnly = value; }
        }

        /// <summary>
        /// Updates statistics and information about the shoutbox
        /// </summary>
        public static void Update()
        {
            throw new NotImplementedException();
        }
    }
}
