using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            }

            public uint ID { get; set; }
            public string Name { get; set; }


            public Channel(uint id, string name)
            {
                ID = id;
                Name = name;
            }

            /// <summary>
            /// List of the most recent shouts available in the channel, updated on executing the <c>Update</c> function
            /// </summary>
            public static List<Shout> Shouts { get; set; }

            /// <summary>
            /// Sends a message to the channel
            /// </summary>
            /// <param name="message"> The message text to send </param>
            public static void Send(string message)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Updates the most recent shouts usually displayed when loading the main page 
            /// </summary>
            public static void Update()
            {
                throw new NotImplementedException();
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

        private static Channel _EnglishOnly = new Channel(0, "EnglishOnly");
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
