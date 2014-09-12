using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace epvpapi
{
    /// <summary>
    /// Represents a subforum
    /// </summary>
    public class Section : UniqueWebObject, IDefaultUpdatable
    {
        /// <summary>
        /// Name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL name that is shown in the address bar of the browser, mostly needed for updating information
        /// </summary>
        public string URLName { get; set; }

        /// <summary>
        /// Short description what the section is about 
        /// </summary>
        public string Description { get; set; }

        public class Announcement : Post
        {
            public DateTime Begins { get; set; }
            public DateTime Ends { get; set; }
            public uint Hits { get; set; }
            public Section Section { get; set; }

            public override string URL
            {
                get { return "http://www.elitepvpers.com/forum/" + Section.URLName + "/announcement-" + Title.URLEscape() + ".html"; }
            }

            public Announcement(Section section, uint id = 0)
                : base(id)
            {
                Section = section;
                Begins = new DateTime();
                Ends = new DateTime();
            }
        }

        /// <summary>
        /// List of all announcements available for this section
        /// </summary>
        public List<Announcement> Announcements { get; set; }

        public override string URL
        {
            get { return "http://www.elitepvpers.com/forum/" + URLName + "/"; }
        }

        public Section(uint id, string urlName)
            : base(id)
        {
            URLName = urlName;
        }

        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (URLName == String.Empty) throw new ArgumentException("Sections cannot be updated if no url-address-name is provided");

            Response res = session.Get("http://www.elitepvpers.com/forum/" + URLName + "/");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            ParseAnnouncements(doc);
        }

        protected void ParseAnnouncements(HtmlDocument doc)
        {
            Announcements = new List<Announcement>();

            var threadListNode = doc.GetElementbyId("threadslist"); 
            if (threadListNode != null)
            {
                threadListNode = threadListNode.SelectSingleNode("tbody");
                var sectionNodes = new List<HtmlNode>(threadListNode.GetElementsByTagName("tr"));

                foreach (var announcementNode in sectionNodes.Take(sectionNodes.Count - 1)) // ignore the last node since that is no actual announcement
                {
                    Announcement announcement = new Announcement(this);

                    var firstLine = announcementNode.SelectSingleNode("td[2]/div[1]");
                    if (firstLine != null)
                    {
                        var hitsNode = firstLine.SelectSingleNode("span[1]/strong[1]");
                        announcement.Hits = (hitsNode != null) ? (uint) Convert.ToDouble(hitsNode.InnerText) : 0;

                        var titleNode = firstLine.SelectSingleNode("a[1]");
                        announcement.Title = (titleNode != null) ? titleNode.InnerText : "";
                    }

                    var secondLine = announcementNode.SelectSingleNode("td[2]/div[2]");
                    if(secondLine != null)
                    {
                        var beginNode = secondLine.SelectSingleNode("span[1]/span[1]");
                        if(beginNode != null)
                        {
                            DateTime beginDate = new DateTime();
                            DateTime.TryParseExact(beginNode.InnerText, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);
                            announcement.Begins = beginDate;
                        }

                        var creatorNode = secondLine.SelectSingleNode("span[2]/a[1]");
                        announcement.Sender.Name = (creatorNode != null) ? creatorNode.InnerText : "";
                        announcement.Sender.Title = secondLine.SelectSingleNode("span[2]/text()[2]").InnerText.Strip();
                        announcement.Sender.Title = announcement.Sender.Title.Remove(0, 1); // remove the brackets
                        announcement.Sender.Title = announcement.Sender.Title.Remove(announcement.Sender.Title.Length - 1, 1); // remove the brackets
                        announcement.Sender.ID = creatorNode.Attributes.Contains("href") ? User.FromURL(creatorNode.Attributes["href"].Value) : 0;   
                    }

                    Announcements.Add(announcement);
                }
            }
        }

        /// <summary>
        /// Loops through the section pages and retrieves the <c>SectionThread</c>s within this section
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="pages"> Amount of pages to request </param>
        /// <param name="startIndex"> Index of the first page that will be requested </param>
        /// <returns> List of all <c>SectionThread</c>s that could be obtained through the requests </returns>
        public List<SectionThread> Threads(Session session, uint pages = 1, uint startIndex = 1)
        {
            session.ThrowIfInvalid();
            if (URLName == String.Empty) throw new ArgumentException("This section is not addressable, please specify the URLName property before using this function");

            List<SectionThread> parsedThreads = new List<SectionThread>();
            for (uint i = startIndex; i <= pages; ++i)
            {
                Response res = session.Get("http://www.elitepvpers.com/forum/" + URLName + "/index" + i + ".html");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res.ToString());

                var threadFrameNode = doc.GetElementbyId("threadbits_forum_" + ID);
                if (threadFrameNode == null) continue;
                
                var threadNodes = new List<HtmlNode>(threadFrameNode.GetElementsByTagName("tr"));
                var normalThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]") != null) ? node.SelectSingleNode("td[1]").InnerText : "") == "Normal Threads");
                var stickyThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]/strong[1]") != null) ? node.SelectSingleNode("td[1]/strong[1]").InnerText : "") == "Sticky Threads");
                List<HtmlNode> normalThreadNodes = new List<HtmlNode>();
                List<HtmlNode> stickyThreadNodes = new List<HtmlNode>();
                List<HtmlNode> totalThreadNodes = new List<HtmlNode>();

                if (stickyThreadsBeginNode != null && normalThreadsBeginNode != null) // if there are any sticky threads present
                {
                    // extract the productive threads into their own sublists, ignore the leading identifiers (= +1) that are displayed as section divider ('Sticky Threads', 'Normal Threads' ...)
                    stickyThreadNodes = threadNodes.GetRange(threadNodes.IndexOf(stickyThreadsBeginNode) + 1, threadNodes.IndexOf(normalThreadsBeginNode) - threadNodes.IndexOf(stickyThreadsBeginNode) - 1);
                    normalThreadNodes = threadNodes.GetRange(threadNodes.IndexOf(normalThreadsBeginNode) + 1, threadNodes.Count - stickyThreadNodes.Count - 2); // -2 since we have 2 dividers

                    totalThreadNodes.InsertRange(totalThreadNodes.Count, normalThreadNodes);
                    totalThreadNodes.InsertRange((totalThreadNodes.Count != 0) ? totalThreadNodes.Count - 1 : 0, stickyThreadNodes);
                }
                else
                {
                    totalThreadNodes = threadNodes;
                }               

                foreach (var threadNode in totalThreadNodes)
                {
                    SectionThread parsedThread = new SectionThread(0, this);
                    parsedThread.Posts.Add(new SectionPost(0, parsedThread));

                    var previewContentNode = threadNode.SelectSingleNode("td[3]");
                    parsedThread.PreviewContent = (previewContentNode != null) ? (previewContentNode.Attributes.Contains("title")) ? previewContentNode.Attributes["title"].Value : "" : "";

                    var titleNode = threadNode.SelectSingleNode("td[3]/div[1]/a[1]");
                    if (titleNode.Id.Contains("thread_gotonew")) // new threads got an additional image displayed (left from the title) wrapped in an 'a' element for quick access to the new reply function
                        titleNode = threadNode.SelectSingleNode("td[3]/div[1]/a[2]");
                    parsedThread.Posts.First().Title = (titleNode != null) ? titleNode.InnerText : "";
                    parsedThread.ID = (titleNode != null) ? (titleNode.Attributes.Contains("href")) ? SectionThread.FromURL(titleNode.Attributes["href"].Value) : 0 : 0;

                    var threadStatusIconNode = threadNode.SelectSingleNode("td[1]/img[1]");
                    parsedThread.Closed = (threadStatusIconNode != null) ? (threadStatusIconNode.Attributes.Contains("src")) ? threadStatusIconNode.Attributes["src"].Value.Contains("lock") : false : false;

                    var creatorNode = threadNode.SelectSingleNode("td[3]/div[2]/span[1]");
                    if (creatorNode != null)
                    {
                        // if the thread has been rated, the element with the yellow stars shows up and is targeted as the first span element
                        // then, the actual node where the information about the creator is stored is located one element below the rating element
                        if (!creatorNode.Attributes.Contains("onclick"))
                            creatorNode = threadNode.SelectSingleNode("td[3]/div[2]/span[2]");

                        parsedThread.Creator = new User(creatorNode.InnerText, creatorNode.Attributes.Contains("onclick") ? User.FromURL(creatorNode.Attributes["onclick"].Value) : 0);
                    }

                    var repliesNode = threadNode.SelectSingleNode("td[5]/a[1]");
                    parsedThread.Replies = (repliesNode != null) ? (uint)Convert.ToDouble(repliesNode.InnerText) : 0;

                    var viewsNode = threadNode.SelectSingleNode("td[6]");
                    parsedThread.Views = (viewsNode != null) ? (uint)Convert.ToDouble(viewsNode.InnerText) : 0;

                    if (stickyThreadNodes.Any(stickyThreadNode => stickyThreadNode == threadNode))
                        parsedThread.Sticked = true;

                    parsedThreads.Add(parsedThread);
                }
            }

            return parsedThreads;
        }

        private static Section _Main = new Section(206, "main"); 
        public static Section Main
        {
            get { return _Main; }
        }

        private static Section _Suggestions = new Section(749, "suggestions");
        public static Section Suggestions
        {
            get { return _Suggestions; }
        }

        private static Section _JoiningElitepvpers = new Section(210, "joining-e-pvp");
        public static Section JoiningElitepvpers
        {
            get { return _JoiningElitepvpers; }
        }

        private static Section _ContentTeamApplications = new Section(564, "content-team-applications");
        public static Section ContentTeamApplications
        {
            get { return _ContentTeamApplications; }
        }

        private static Section _ComplaintArea = new Section(466, "complaint-area");
        public static Section ComplaintArea
        {
            get { return _ComplaintArea; }
        }

        private static Section _TBMRatingSupport = new Section(770, "tbm-rating-support");
        public static Section TBMRatingSupport
        {
            get { return _TBMRatingSupport; }
        }

        private static Section _EliteGoldSupport = new Section(614, "elite-gold-support");
        public static Section EliteGoldSupport
        {
            get { return _EliteGoldSupport; }
        }
    }
}
