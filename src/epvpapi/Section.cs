using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi
{
    /// <summary>
    /// Represents a subforum
    /// </summary>
    public class Section : UniqueObject, IUpdatable, IUniqueWebObject
    {
        /// <summary>
        /// Name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL name that is shown in the address bar of the browser, mostly needed for updating information
        /// </summary>
        public string UrlName { get; set; }

        /// <summary>
        /// Short description what the section is about 
        /// </summary>
        public string Description { get; set; }

        public class Announcement : Post, IUniqueWebObject
        {
            public DateTime Begins { get; set; }
            public DateTime Ends { get; set; }
            public uint Hits { get; set; }
            public Section Section { get; set; }

            public Announcement(Section section, uint id = 0)
                : base(id)
            {
                Section = section;
                Begins = new DateTime();
                Ends = new DateTime();
            }

            public string GetUrl()
            {
                return "http://www.elitepvpers.com/forum/" + Section.UrlName + "/announcement-" + Title.UrlEscape() + ".html";
            } 

        }

        /// <summary>
        /// List of all announcements available for this section
        /// </summary>
        public List<Announcement> Announcements { get; set; } 

        public Section(uint id, string urlName)
            : base(id)
        {
            UrlName = urlName;
        }

        /// <summary>
        /// Updates information about the section
        /// </summary>
        /// <param name="authenticatedSession"> Session used for storing personal shoutbox data into the session user field </param>
        public void Update<TUser>(AuthenticatedSession<TUser> authenticatedSession) where TUser : User
        {
            authenticatedSession.ThrowIfInvalid();
            if (UrlName == String.Empty) throw new ArgumentException("Sections cannot be updated if no url-address-name is provided");

            var res = authenticatedSession.Get("http://www.elitepvpers.com/forum/" + UrlName + "/");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            new SectionParser.AnnouncementsParser(this).Execute(doc.GetElementbyId("threadslist"));
        }

        /// <summary>
        /// Loops through the section pages and retrieves the <c>SectionThread</c>s within this section
        /// </summary>
        /// <param name="authenticatedSession"> Session that is used for sending the request </param>
        /// <param name="pages"> Amount of pages to request </param>
        /// <param name="startIndex"> Index of the first page that will be requested </param>
        /// <returns> List of all <c>SectionThread</c>s that could be obtained through the requests </returns>
        public List<SectionThread> Threads<TUser>(AuthenticatedSession<TUser> authenticatedSession, uint pages = 1, uint startIndex = 1) where TUser : User
        {
            authenticatedSession.ThrowIfInvalid();
            if (UrlName == String.Empty) throw new ArgumentException("This section is not addressable, please specify the URLName property before using this function");

            var parsedThreads = new List<SectionThread>();
            for (uint i = startIndex; i <= pages; ++i)
            {
                var res = authenticatedSession.Get("http://www.elitepvpers.com/forum/" + UrlName + "/index" + i + ".html");
                var doc = new HtmlDocument();
                doc.LoadHtml(res.ToString());

                var threadFrameNode = doc.GetElementbyId("threadbits_forum_" + ID);
                if (threadFrameNode == null) continue;

                var threadNodes = new List<HtmlNode>(threadFrameNode.ChildNodes.GetElementsByTagName("tr"));
                var normalThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]") != null) ? node.SelectSingleNode("td[1]").InnerText : "") == "Normal Threads");
                var stickyThreadsBeginNode = threadNodes.Find(node => ((node.SelectSingleNode("td[1]/strong[1]") != null) ? node.SelectSingleNode("td[1]/strong[1]").InnerText : "") == "Sticky Threads");
                var normalThreadNodes = new List<HtmlNode>();
                var stickyThreadNodes = new List<HtmlNode>();
                var totalThreadNodes = new List<HtmlNode>();

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
                    var parsedThread = new SectionThread(0, this);
                    new SectionParser.ThreadListingParser(parsedThread).Execute(threadNode);

                    if (stickyThreadNodes.Any(stickyThreadNode => stickyThreadNode == threadNode))
                        parsedThread.Sticked = true;

                    if(parsedThread.ID != 0)
                        parsedThreads.Add(parsedThread);
                }
            }

            return parsedThreads;
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/" + UrlName + "/";
        } 

        private static readonly Section _Main = new Section(206, "main");
        public static Section Main
        {
            get { return _Main; }
        }

        private static readonly Section _Suggestions = new Section(749, "suggestions");
        public static Section Suggestions
        {
            get { return _Suggestions; }
        }

        private static readonly Section _JoiningElitepvpers = new Section(210, "joining-e-pvp");
        public static Section JoiningElitepvpers
        {
            get { return _JoiningElitepvpers; }
        }

        private static readonly Section _ContentTeamApplications = new Section(564, "content-team-applications");
        public static Section ContentTeamApplications
        {
            get { return _ContentTeamApplications; }
        }

        private static readonly Section _ComplaintArea = new Section(466, "complaint-area");
        public static Section ComplaintArea
        {
            get { return _ComplaintArea; }
        }

        private static readonly Section _TBMRatingSupport = new Section(770, "tbm-rating-support");
        public static Section TBMRatingSupport
        {
            get { return _TBMRatingSupport; }
        }

        private static readonly Section _EliteGoldSupport = new Section(614, "elite-gold-support");
        public static Section EliteGoldSupport
        {
            get { return _EliteGoldSupport; }
        }

        private static readonly Section _EliteGoldTrading = new Section(580, "elite-gold-trading");
        public static Section EliteGoldTrading
        {
            get { return _EliteGoldTrading; }
        }

        private static readonly Section _Trading = new Section(368, "trading");
        public static Section Trading
        {
            get { return _Trading; }
        }

        public static List<Section> Sections = new List<Section>()
        {
            Main,
            Suggestions,
            JoiningElitepvpers,
            ContentTeamApplications,
            ComplaintArea,
            TBMRatingSupport,
            EliteGoldSupport,
            EliteGoldTrading,
            Trading
        };
    }
}
