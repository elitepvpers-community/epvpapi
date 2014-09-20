using System;
using System.Collections.Generic;
using System.Linq;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi
{
    /// <summary>
    ///     Represents a subforum
    /// </summary>
    public class Section : UniqueObject, IDefaultUpdatable, IUniqueWebObject
    {
        private static readonly Section _Main = new Section(206, "main");
        private static readonly Section _Suggestions = new Section(749, "suggestions");
        private static readonly Section _JoiningElitepvpers = new Section(210, "joining-e-pvp");
        private static readonly Section _ContentTeamApplications = new Section(564, "content-team-applications");
        private static readonly Section _ComplaintArea = new Section(466, "complaint-area");
        private static readonly Section _TBMRatingSupport = new Section(770, "tbm-rating-support");
        private static readonly Section _EliteGoldSupport = new Section(614, "elite-gold-support");

        public Section(uint id, string urlName)
            : base(id)
        {
            UrlName = urlName;
        }

        /// <summary>
        ///     Name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     URL name that is shown in the address bar of the browser, mostly needed for updating information
        /// </summary>
        public string UrlName { get; private set; }

        /// <summary>
        ///     Short description what the section is about
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     List of all announcements available for this section
        /// </summary>
        private List<Announcement> Announcements { get; set; }

        public static Section Main
        {
            get { return _Main; }
        }

        public static Section Suggestions
        {
            get { return _Suggestions; }
        }

        public static Section JoiningElitepvpers
        {
            get { return _JoiningElitepvpers; }
        }

        public static Section ContentTeamApplications
        {
            get { return _ContentTeamApplications; }
        }

        public static Section ComplaintArea
        {
            get { return _ComplaintArea; }
        }

        public static Section TbmRatingSupport
        {
            get { return _TBMRatingSupport; }
        }

        public static Section EliteGoldSupport
        {
            get { return _EliteGoldSupport; }
        }

        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (UrlName == String.Empty)
                throw new ArgumentException("Sections cannot be updated if no url-address-name is provided");

            Response res = session.Get("http://www.elitepvpers.com/forum/" + UrlName + "/");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            new AnnouncementsParser(this).Execute(doc.GetElementbyId("threadslist"));
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/" + UrlName + "/";
        }

        /// <summary>
        ///     Loops through the section pages and retrieves the <c>SectionThread</c>s within this section
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="pages"> Amount of pages to request </param>
        /// <param name="startIndex"> Index of the first page that will be requested </param>
        /// <returns> List of all <c>SectionThread</c>s that could be obtained through the requests </returns>
        public List<SectionThread> Threads(Session session, uint pages = 1, uint startIndex = 1)
        {
            session.ThrowIfInvalid();
            if (UrlName == String.Empty)
                throw new ArgumentException(
                    "This section is not addressable, please specify the URLName property before using this function");

            var parsedThreads = new List<SectionThread>();
            for (uint i = startIndex; i <= pages; ++i)
            {
                Response res = session.Get("http://www.elitepvpers.com/forum/" + UrlName + "/index" + i + ".html");
                var doc = new HtmlDocument();
                doc.LoadHtml(res.ToString());

                HtmlNode threadFrameNode = doc.GetElementbyId("threadbits_forum_" + Id);
                if (threadFrameNode == null) continue;

                var threadNodes = new List<HtmlNode>(threadFrameNode.GetElementsByTagName("tr"));
                HtmlNode normalThreadsBeginNode =
                    threadNodes.Find(
                        node =>
                            ((node.SelectSingleNode("td[1]") != null) ? node.SelectSingleNode("td[1]").InnerText : "") ==
                            "Normal Threads");
                HtmlNode stickyThreadsBeginNode =
                    threadNodes.Find(
                        node =>
                            ((node.SelectSingleNode("td[1]/strong[1]") != null)
                                ? node.SelectSingleNode("td[1]/strong[1]").InnerText
                                : "") == "Sticky Threads");
                var stickyThreadNodes = new List<HtmlNode>();
                var totalThreadNodes = new List<HtmlNode>();

                if (stickyThreadsBeginNode != null && normalThreadsBeginNode != null)
                    // if there are any sticky threads present
                {
                    // extract the productive threads into their own sublists, ignore the leading identifiers (= +1) that are displayed as section divider ('Sticky Threads', 'Normal Threads' ...)
                    stickyThreadNodes = threadNodes.GetRange(threadNodes.IndexOf(stickyThreadsBeginNode) + 1,
                        threadNodes.IndexOf(normalThreadsBeginNode) - threadNodes.IndexOf(stickyThreadsBeginNode) - 1);
                    List<HtmlNode> normalThreadNodes =
                        threadNodes.GetRange(threadNodes.IndexOf(normalThreadsBeginNode) + 1,
                            threadNodes.Count - stickyThreadNodes.Count - 2);

                    totalThreadNodes.InsertRange(totalThreadNodes.Count, normalThreadNodes);
                    totalThreadNodes.InsertRange((totalThreadNodes.Count != 0) ? totalThreadNodes.Count - 1 : 0,
                        stickyThreadNodes);
                }
                else
                {
                    totalThreadNodes = threadNodes;
                }

                foreach (HtmlNode threadNode in totalThreadNodes)
                {
                    var parsedThread = new SectionThread(0, this);
                    new ThreadListingParser(parsedThread).Execute(threadNode);

                    if (stickyThreadNodes.Any(stickyThreadNode => stickyThreadNode == threadNode))
                        parsedThread.Sticked = true;

                    if (parsedThread.Id != 0)
                        parsedThreads.Add(parsedThread);
                }
            }

            return parsedThreads;
        }

        private class Announcement : Post, IUniqueWebObject
        {
            public Announcement(Section section, uint id = 0)
                : base(id)
            {
                Section = section;
                Begins = new DateTime();
                Ends = new DateTime();
            }

            public DateTime Begins { get; set; }
            private DateTime Ends { get; set; }
            public uint Hits { get; set; }
            private Section Section { get; set; }

            public string GetUrl()
            {
                return "http://www.elitepvpers.com/forum/" + Section.UrlName + "/announcement-" + Title.UrlEscape() +
                       ".html";
            }
        }

        private class AnnouncementsParser : TargetableParser<Section>, INodeParser
        {
            public AnnouncementsParser(Section target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                Target.Announcements = new List<Announcement>();

                if (coreNode == null) return;
                coreNode = coreNode.SelectSingleNode("tbody");
                var sectionNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("tr"));

                foreach (HtmlNode announcementNode in sectionNodes.Take(sectionNodes.Count - 1))
                    // ignore the last node since that is no actual announcement
                {
                    var announcement = new Announcement(Target);

                    HtmlNode firstLine = announcementNode.SelectSingleNode("td[2]/div[1]");
                    if (firstLine != null)
                    {
                        HtmlNode hitsNode = firstLine.SelectSingleNode("span[1]/strong[1]");
                        announcement.Hits = (hitsNode != null) ? (uint) Convert.ToDouble(hitsNode.InnerText) : 0;

                        HtmlNode titleNode = firstLine.SelectSingleNode("a[1]");
                        announcement.Title = (titleNode != null) ? titleNode.InnerText : "";
                    }

                    HtmlNode secondLine = announcementNode.SelectSingleNode("td[2]/div[2]");
                    if (secondLine != null)
                    {
                        HtmlNode beginNode = secondLine.SelectSingleNode("span[1]/span[1]");
                        if (beginNode != null)
                            announcement.Begins = beginNode.InnerText.ToElitepvpersDateTime();

                        HtmlNode creatorNode = secondLine.SelectSingleNode("span[2]/a[1]");
                        announcement.Sender.Name = (creatorNode != null) ? creatorNode.InnerText : "";
                        announcement.Sender.Title =
                            secondLine.SelectSingleNode("span[2]/text()[2]").InnerText.Strip();
                        announcement.Sender.Title = announcement.Sender.Title.Remove(0, 1); // remove the brackets
                        announcement.Sender.Title =
                            announcement.Sender.Title.Remove(announcement.Sender.Title.Length - 1, 1);
                        // remove the brackets
                        announcement.Sender.Id = creatorNode.Attributes.Contains("href")
                            ? User.FromUrl(creatorNode.Attributes["href"].Value)
                            : 0;
                    }

                    Target.Announcements.Add(announcement);
                }
            }
        }

        private class ThreadListingParser : TargetableParser<SectionThread>, INodeParser
        {
            public ThreadListingParser(SectionThread target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                HtmlNode previewContentNode = coreNode.SelectSingleNode("td[3]");
                if (previewContentNode != null)
                {
                    if (previewContentNode.InnerText.Contains("Moved"))
                        return; // moved threads do not contain any data to parse
                    Target.PreviewContent = (previewContentNode.Attributes.Contains("title"))
                        ? previewContentNode.Attributes["title"].Value
                        : "";
                }

                HtmlNode titleNode = coreNode.SelectSingleNode("td[3]/div[1]/a[1]");
                if (titleNode.Id.Contains("thread_gotonew"))
                    // new threads got an additional image displayed (left from the title) wrapped in an 'a' element for quick access to the new reply function
                    titleNode = coreNode.SelectSingleNode("td[3]/div[1]/a[2]");
                Target.InitialPost.Title = (titleNode != null) ? titleNode.InnerText : "";
                Target.Id = (titleNode != null)
                    ? (titleNode.Attributes.Contains("href"))
                        ? SectionThread.FromUrl(titleNode.Attributes["href"].Value)
                        : 0
                    : 0;

                HtmlNode threadStatusIconNode = coreNode.SelectSingleNode("td[1]/img[1]");
                Target.Closed = (threadStatusIconNode != null) &&
                                ((threadStatusIconNode.Attributes.Contains("src")) &&
                                 threadStatusIconNode.Attributes["src"].Value.Contains("lock"));

                HtmlNode creatorNode = coreNode.SelectSingleNode("td[3]/div[2]/span[1]");
                if (creatorNode != null)
                {
                    // if the thread has been rated, the element with the yellow stars shows up and is targeted as the first span element
                    // then, the actual node where the information about the creator is stored is located one element below the rating element
                    if (!creatorNode.Attributes.Contains("onclick"))
                        creatorNode = coreNode.SelectSingleNode("td[3]/div[2]/span[2]");

                    Target.Creator = new User(creatorNode.InnerText,
                        creatorNode.Attributes.Contains("onclick")
                            ? User.FromUrl(creatorNode.Attributes["onclick"].Value)
                            : 0);
                }

                HtmlNode repliesNode = coreNode.SelectSingleNode("td[5]/a[1]");
                Target.ReplyCount = (repliesNode != null) ? (uint) Convert.ToDouble(repliesNode.InnerText) : 0;

                HtmlNode viewsNode = coreNode.SelectSingleNode("td[6]");
                Target.ViewCount = (viewsNode != null) ? (uint) Convert.ToDouble(viewsNode.InnerText) : 0;
            }
        }
    }
}