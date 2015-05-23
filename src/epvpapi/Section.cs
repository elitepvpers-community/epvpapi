using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

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
        [Obsolete("UrlName has been renamed to Shortname")]
        public string UrlName
        {
            get { return Shortname; }
            set { Shortname = value; }
        }

        /// <summary>
        /// Short name that is shown in the address bar of the browser, mostly needed for updating information
        /// </summary>
        public string Shortname { get; set; }

        /// <summary>
        /// Short description what the section is about 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Amount of pages in this section
        /// </summary>
        public uint PageCount { get; set; }

        /// <summary>
        /// List of all announcements available for this section
        /// </summary>
        public List<Announcement> Announcements { get; set; }

        public Section(int id, string shortname)
            : base(id)
        {
            Shortname = shortname;
        }

        /// <summary>
        /// Updates information about the section
        /// </summary>
        /// <param name="session"> Session used for storing personal shoutbox data into the session user field </param>
        public void Update(GuestSession session)
        {
            if (Shortname == String.Empty) throw new ArgumentException("Sections cannot be updated if no url-address-name is provided");

            var res = session.Get(String.Format("http://www.elitepvpers.com/forum/{0}/", Shortname));
            var doc = new HtmlDocument();
            doc.LoadHtml(res);

            PageCount = new Regex(@"<td.*?>Page \d+ of (\d+)</td>").Match(doc.DocumentNode.InnerHtml).Groups[1].Value.To<UInt32>();

            new SectionParser.AnnouncementsParser(this).Execute(doc.GetElementbyId("threadslist"));
        }

        /// <summary>
        /// Loops through the section pages and retrieves the <c>SectionThread</c>s within this section
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="pages"> Amount of pages to request </param>
        /// <param name="startIndex"> Index of the first page that will be requested </param>
        /// <returns> List of all <c>SectionThread</c>s that could be obtained through the requests </returns>
        public List<SectionThread> Threads<TUser>(AuthenticatedSession<TUser> session, uint pages = 1, uint startIndex = 1) where TUser : User
        {
            session.ThrowIfInvalid();
            if (Shortname == String.Empty) throw new ArgumentException("This section is not addressable, please specify the URLName property before using this function");

            var parsedThreads = new List<SectionThread>();
            for (uint i = startIndex; i < startIndex + pages; ++i)
            {
                var res = session.Get(String.Format("http://www.elitepvpers.com/forum/{0}/index{1}.html", Shortname, i));
                var doc = new HtmlDocument();
                doc.LoadHtml(res);

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

                    if (parsedThread.ID != 0)
                        parsedThreads.Add(parsedThread);
                }
            }

            return parsedThreads;
        }

        public static List<Section> GetSectionByShortname<TUser>(AuthenticatedSession<TUser> session, string shortname) where TUser : User
        {
            var sections = new List<Section>();

            var res = session.Get("http://www.elitepvpers.com/forum/main");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            var selectElements = doc.DocumentNode
                .Descendants()
                .GetElementsByTagName("select")
                .GetElementsByAttribute("name", "f")
                .ToList();

            if (selectElements.Count != 1)
                throw new ParsingFailedException("The goto selection dropbox could not be found");

            var selectElement = selectElements.First();
            var forumsNode = selectElement.SelectSingleNode("optgroup[2]");

            if (forumsNode == null)
                throw new ParsingFailedException("The root node of the listed forums wasn't found");

            foreach (var forum in forumsNode.ChildNodes.GetElementsByTagName("option"))
            {
                var forumName = forum.NextSibling.InnerText.Strip();
                forumName = Regex.Replace(forumName, "(&amp;)|(^ )|(')|((&nbsp; )+)", "");

                var forumShortname = Regex.Replace(forumName, @"(\bof\b)|(\bfor\b)|(\bthe\b)", "", RegexOptions.IgnoreCase);
                forumShortname = Regex.Replace(forumShortname, "[^a-zA-Z0-9']+", "-");
                forumShortname = Regex.Replace(forumShortname, "(-$)|(^-)", "");
                forumShortname = forumShortname.ToLower();

                if (forumShortname != shortname) continue;

                sections.Add(new Section(forum.Attributes["value"].Value.To<int>(), forumShortname) { Name = forumName });
            }

            return sections;
        }

        public static List<Section> GetTopLevelSections<TUser>(AuthenticatedSession<TUser> session) where TUser : User
        {
            var sections = new List<Section>();

            var res = session.Get("http://www.elitepvpers.com/forum/main");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            var selectElements = doc.DocumentNode
                .Descendants()
                .GetElementsByTagName("select")
                .GetElementsByAttribute("name", "f")
                .ToList();

            if (selectElements.Count != 1)
                throw new ParsingFailedException("The goto selection dropbox could not be found");

            var selectElement = selectElements.First();
            var forumsNode = selectElement.SelectSingleNode("optgroup[2]");

            if (forumsNode == null)
                throw new ParsingFailedException("The root node of the listed forums wasn't found");

            foreach (var forum in forumsNode.ChildNodes.GetElementsByTagName("option"))
            {
                var forumName = forum.NextSibling.InnerText.Strip();
                forumName = Regex.Replace(forumName, "(&amp;)|(^ )|(')|((&nbsp; )+)", "");

                var forumShortname = Regex.Replace(forumName, @"(\bof\b)|(\bfor\b)|(\bthe\b)", "", RegexOptions.IgnoreCase);
                forumShortname = Regex.Replace(forumShortname, "[^a-zA-Z0-9']+", "-");
                forumShortname = Regex.Replace(forumShortname, "(-$)|(^-)", "");
                forumShortname = forumShortname.ToLower();

                sections.Add(new Section(forum.Attributes["value"].Value.To<int>(), forumShortname) { Name = forumName });
            }

            return sections;
        }

        static List<HtmlNode> getNodesWithStartingID(HtmlNode node, string id)
        {
            var nodes = new List<HtmlNode>();
            foreach (var childNode in node.ChildNodes)
                if (childNode.Id.StartsWith(id))
                {
                    nodes.Add(childNode);
                    var childNodes = getNodesWithStartingID(childNode, id);
                    if (childNodes.Count > 0)
                        nodes.AddRange(childNodes);
                }
            return nodes;
        }

        public string GetUrl()
        {
            return String.Format("http://www.elitepvpers.com/forum/{0}/", Shortname);
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
