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
        /// A single shout send by an user
        /// </summary>
        public class Shout
        {
            public User User { get; set; }
            public string Message { get; set; }
            public DateTime Time { get; set; }
        }

        /// <summary>
        /// List of all Shouts available, updated on executing the <c>Update</c> function
        /// </summary>
        public static List<Shout> Shouts { get; set; }

        /// <summary>
        /// Contains the Top 10 chatter
        /// </summary>
        public static List<User> TopChatter { get; set; }

        /// <summary>
        /// Amount of messages stored in the shoutbox
        /// </summary>
        public static uint MessageCount { get; set; }

        /// <summary>
        /// Amount of messages stored within the last 24 hours
        /// </summary>
        public static uint MessageCountCurrentDay { get; set; }

        public static void Send(string message)
        {

        }

        public static void Update(uint pages = 10)
        {

        }
    }
}
