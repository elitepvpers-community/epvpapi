using epvpapi.Connection;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace epvpapi
{
    public class Statistic : IUpdatable
    {
        /// <summary>
        /// Count of threads created
        /// </summary>
        public uint Threads { get; private set; }

        /// <summary>
        /// Number of posts done
        /// </summary>
        public uint Posts { get; private set; }

        /// <summary>
        /// Total amount of members
        /// </summary>
        public uint Members { get; private set; }

        /// <summary>
        /// Creates an stastic object which allows you to retrieve some elitepvpers statistics
        /// </summary>
        public Statistic()
        {
            Threads = 0;
            Posts = 0;
            Members = 0;
        }

        /// <summary>
        /// Updates statistic
        /// </summary>
        /// <param name="session"> Session which will be used to retrieve the statistic </param>
        public void Update(GuestSession session)
        {
            var res = session.Get("http://www.elitepvpers.com/forum/");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            var statsNode = doc.GetElementbyId("collapseobj_forumhome_stats");
            if (statsNode == null) return;

            statsNode = statsNode.SelectSingleNode("tr[1]/td[2]/div[1]/div[1]");
            if (statsNode == null) return;

            var match = Regex.Matches(statsNode.InnerText, "([0-9,]+)");
            if (match.Count == 3)
            {
                Threads = match[0].Groups[1].Value.Replace(",", "").To<uint>();
                Posts = match[1].Groups[1].Value.Replace(",", "").To<uint>();
                Members = match[2].Groups[1].Value.Replace(",", "").To<uint>();
            }
        }
    }
}
