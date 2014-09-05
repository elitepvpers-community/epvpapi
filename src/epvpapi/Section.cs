using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents a subforum
    /// </summary>
    public class Section : UniqueWebObject
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

        /// <summary>
        /// List of all <c>SectionThread</c> objects representing the threads in the section
        /// </summary>
        public List<SectionThread> Threads { get; set; }

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
            Threads = new List<SectionThread>();
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
            ParseThreads(doc);
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

        protected void ParseThreads(HtmlDocument doc)
        {
            var threadFrameNode = doc.GetElementbyId("threadbits_forum_" + ID);
            if(threadFrameNode != null)
            {
                var threadNodes = new List<HtmlNode>(threadFrameNode.GetElementsByTagName("tr"));
                var stickyThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]/strong[1]") != null) ? node.SelectSingleNode("td[1]/strong[1]").InnerText : "") == "Sticky Threads");
                List<HtmlNode> normalThreadNodes = new List<HtmlNode>();
                List<HtmlNode> stickyThreadNodes = new List<HtmlNode>();

                if(stickyThreadsBeginNode != null) // if there are any sticky threads present
                {
                    var normalThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]") != null) ? node.SelectSingleNode("td[1]").InnerText : "") == "Normal Threads");
                    if(normalThreadsBeginNode != null)
                    {
                        // extract the productive threads into their own sublists, ignore the leading identifiers (= +1) that are displayed as section divider ('Sticky Threads', 'Normal Threads' ...)
                        stickyThreadNodes = threadNodes.GetRange(threadNodes.IndexOf(stickyThreadsBeginNode) + 1, threadNodes.IndexOf(normalThreadsBeginNode) - threadNodes.IndexOf(stickyThreadsBeginNode) - 1);
                        normalThreadNodes = threadNodes.GetRange(threadNodes.IndexOf(normalThreadsBeginNode) + 1, threadNodes.Count - stickyThreadNodes.Count - 2); // -2 since we have 2 dividers
                    }
                }

                List<HtmlNode> totalThreadNodes = new List<HtmlNode>();
                totalThreadNodes.InsertRange(totalThreadNodes.Count, normalThreadNodes);
                totalThreadNodes.InsertRange(totalThreadNodes.Count - 1, stickyThreadNodes);

                foreach(var threadNode in totalThreadNodes)
                {
                    SectionThread parsedThread = new SectionThread(0, this);
                    parsedThread.Posts.Add(new SectionPost(0, parsedThread));

                    var previewContentNode = threadNode.SelectSingleNode("td[3]");
                    parsedThread.PreviewContent = (previewContentNode != null) ? (previewContentNode.Attributes.Contains("title")) ? previewContentNode.Attributes["title"].Value : "" : "";

                    var titleNode = threadNode.SelectSingleNode("td[3]/div[1]/a[1]");
                    if(titleNode.Id.Contains("thread_gotonew")) // new threads got an additional image displayed (left from the title) wrapped in an 'a' element for quick access to the new reply function
                        titleNode = threadNode.SelectSingleNode("td[3]/div[1]/a[2]");
                    parsedThread.Posts.First().Title = (titleNode != null) ? titleNode.InnerText : "";

                    if (stickyThreadNodes.Any(stickyThreadNode => stickyThreadNode == threadNode))
                        parsedThread.Sticked = true;

                    Threads.Add(parsedThread);
                }
            }
        }

        public static Section Main
        {
            get { return new Section(206, "main"); }
        }

        public static Section Suggestions
        {
            get { return new Section(749, "suggestions"); }
        }

        public static Section JoiningElitepvpers
        {
            get { return new Section(210, "joining-e-pvp"); }
        }

        public static Section ContentTeamApplications
        {
            get { return new Section(564, "content-team-applications"); }
        }

        public static Section ComplaintArea
        {
            get { return new Section(466, "complaint-area"); }
        }

        public static Section TBMRatingSupport
        {
            get { return new Section(770, "tbm-rating-support"); }
        }

        public static Section EliteGoldSupport
        {
            get { return new Section(614, "elite-gold-support"); }
        }
    }
}
