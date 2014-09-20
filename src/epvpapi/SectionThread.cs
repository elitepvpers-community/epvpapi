using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi
{
    /// <summary>
    ///     Represents a thread within a section
    /// </summary>
    public class SectionThread : Thread, IReasonableDeletable, IDefaultUpdatable, IUniquePageableWebObject
    {
        public SectionThread(Section section)
            : this(0, section)
        {
        }

        public SectionThread(uint id, Section section)
            : base(id)
        {
            Section = section;
            InitialPost = new SectionPost(0, this);
            Tags = new List<string>();
            PageCount = 1;
        }

        /// <summary>
        ///     Section under which the <c>SectionThread</c> is listed
        /// </summary>
        private Section Section { get; set; }

        /// <summary>
        ///     If true, the thread state is closed meaning that no one (except the staff) can answer to this thread
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        ///     If true, the thread was sticked from a moderator and will always stay at the top section of the threads
        /// </summary>
        public bool Sticked { get; set; }

        /// <summary>
        ///     Preview of the thread content being displayed when hovering over the link
        /// </summary>
        public string PreviewContent { get; set; }

        /// <summary>
        ///     Current average rating of the <c>SectionThread</c>
        /// </summary>
        private double Rating { get; set; }

        /// <summary>
        ///     Amount of views that have been recorded
        /// </summary>
        public uint ViewCount { get; set; }

        /// <summary>
        ///     The first post in the <c>SectionThread</c>, usually created by the thread starter
        /// </summary>
        public SectionPost InitialPost { get; private set; }

        /// <summary>
        ///     Tags that have been set for better search results when using the board's search function
        /// </summary>
        private List<string> Tags { get; set; }

        /// <summary>
        ///     Amount of pages
        /// </summary>
        private uint PageCount { get; set; }

        /// <summary>
        ///     Amount of posts within all pages
        /// </summary>
        public uint PostCount { get; set; }

        /// <summary>
        ///     Retrieves information about the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <remarks>
        ///     ID and the section represented by a <c>Section</c> object is required in order to update.
        /// </remarks>
        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            if (String.IsNullOrEmpty(Section.UrlName))
                throw new ArgumentException("The section needs to be addressable");

            Response res = session.Get(GetUrl());
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res.ToString());

            HtmlNode postsRootNode = htmlDocument.GetElementbyId("posts");
            if (postsRootNode == null) return;

            Closed = htmlDocument.DocumentNode.GetDescendentElementsByName("img")
                .Any(
                    node => node.Attributes.Contains("src") && node.Attributes["src"].Value.Contains("threadclosed.gif"));

            HtmlNode currentRatingNode = htmlDocument.GetElementbyId("threadrating_current");
            if (currentRatingNode != null)
            {
                currentRatingNode = currentRatingNode.SelectSingleNode("img[1]");
                if (currentRatingNode != null)
                {
                    if (currentRatingNode.Attributes.Contains("alt"))
                    {
                        Match ratingMatch =
                            new Regex(@",\s+([0-9](?:,|.)[0-9]{2})\s+\S+").Match(
                                currentRatingNode.Attributes["alt"].Value);
                        if (ratingMatch.Groups.Count > 1)
                            Rating = Convert.ToDouble(ratingMatch.Groups[1].Value);
                    }
                }
            }

            HtmlNode tagsRootNode = htmlDocument.GetElementbyId("tag_list_cell");
            if (tagsRootNode != null)
            {
                foreach (HtmlNode tagNode in tagsRootNode.GetElementsByTagName("a"))
                    Tags.Add(tagNode.InnerText);
            }

            HtmlNode pageCountRootNode = htmlDocument.GetElementbyId("poststop");
            if (pageCountRootNode == null) return;
            pageCountRootNode =
                pageCountRootNode.SelectSingleNode(
                    "following-sibling::table[1]/tr[1]/td[2]/div[1]/table[1]/tr[1]/td[1]");
            if (pageCountRootNode == null) return;
            Match countMatch =
                new Regex(@"\S+\s{1}[0-9]+\s{1}\S+\s{1}([0-9]+)").Match(pageCountRootNode.InnerText);
            if (countMatch.Groups.Count > 1)
                PageCount = Convert.ToUInt32(countMatch.Groups[1].Value);
        }

        /// <summary>
        ///     Deletes the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        /// <remarks>
        ///     Not tested yet!
        /// </remarks>
        public void Delete<T>(ProfileSession<T> session, string reason) where T : User
        {
            if (session.User.GetHighestRank() < User.Rank.GlobalModerator)
                throw new InsufficientAccessException("You don't have enough access rights to delete this thread");
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/postings.php",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("t", Id.ToString()),
                    new KeyValuePair<string, string>("do", "dodeletethread"),
                    new KeyValuePair<string, string>("deletetype", "1"),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("deletereason", reason),
                });

            Deleted = true;
        }

        public string GetUrl(uint pageIndex = 1)
        {
            return "http://www.elitepvpers.com/forum/" + Section.UrlName + "/"
                   + Id + "-" + InitialPost.Title.UrlEscape()
                   + ((pageIndex > 1) ? "-" + pageIndex : "")
                   + ".html";
        }

        /// <summary>
        ///     Creates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="section"> Section under which the <c>SectionThread</c> is listed </param>
        /// <param name="startPost"> Represents the content and title of the <c>SectionThread</c> </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <param name="closed">
        ///     If true, the thread state is closed meaning that no one (except the staff) can answer to this
        ///     thread
        /// </param>
        /// <returns> Freshly created <c>SectionThread</c> </returns>
        public static SectionThread Create(ProfileSession<User> session, Section section, SectionPost startPost,
            SectionPost.Settings settings = SectionPost.Settings.ParseUrl | SectionPost.Settings.ShowSignature,
            bool closed = false)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newthread.php?do=postthread&f=" + section.Id,
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("subject", startPost.Title),
                    new KeyValuePair<string, string>("message", startPost.Content),
                    new KeyValuePair<string, string>("wysiwyg", "0"),
                    new KeyValuePair<string, string>("taglist", String.Empty),
                    new KeyValuePair<string, string>("iconid", "0"),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("f", section.Id.ToString()),
                    new KeyValuePair<string, string>("do", "postthread"),
                    new KeyValuePair<string, string>("posthash", "74532335f4d3a9f352db6af1b1c257f7"),
                    new KeyValuePair<string, string>("poststarttime", "1389309192"),
                    new KeyValuePair<string, string>("loggedinuser", session.User.Id.ToString()),
                    new KeyValuePair<string, string>("sbutton", "Submit New Thread"),
                    new KeyValuePair<string, string>("signature",
                        Convert.ToInt32(settings.HasFlag(SectionPost.Settings.ShowSignature))
                            .ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("parseurl",
                        Convert.ToInt32(settings.HasFlag(SectionPost.Settings.ParseUrl))
                            .ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("parseame", "1"),
                    new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                    new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                    new KeyValuePair<string, string>("emailupdate", "9999"),
                    new KeyValuePair<string, string>("polloptions", "4")
                });

            return new SectionThread(0, section);
        }

        /// <summary>
        ///     Closes the <c>SectionThread</c> if it is open
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        ///     Switch function - If the thread is closed, it will be opened and vice versa when executing this function
        ///     Not tested yet!
        /// </remarks>
        private void Close(Session session)
        {
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/postings.php",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("t", Id.ToString()),
                    new KeyValuePair<string, string>("do", "openclosethread"),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("pollid", String.Empty),
                });

            Closed = true;
        }

        /// <summary>
        ///     Opens the <c>SectionThread</c> if it is closed
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        ///     Not tested yet!
        /// </remarks>
        public void Open(Session session)
        {
            Close(session);
            Closed = false;
        }


        /// <summary>
        ///     Rates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="rating"> Represents the rate value (0-5) </param>
        public void Rate(Session session, uint rating)
        {
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/threadrate.php",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ajax", "1"),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("vote", rating.ToString()),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("t", Id.ToString()),
                    new KeyValuePair<string, string>("pp", "10"),
                    new KeyValuePair<string, string>("page", "1")
                });
        }


        /// <summary>
        ///     Replies to the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="post"> Reply to post </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        ///     The ID of the thread has to be given in order to reply
        /// </remarks>
        public void Reply(ProfileSession<User> session, SectionPost post,
            SectionPost.Settings settings = SectionPost.Settings.ParseUrl | SectionPost.Settings.ShowSignature)
        {
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newreply.php?do=postreply&t=" + Id,
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("title", post.Title),
                    new KeyValuePair<string, string>("message", post.Content),
                    new KeyValuePair<string, string>("wysiwyg", "0"),
                    new KeyValuePair<string, string>("iconid", post.Icon.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "postreply"),
                    new KeyValuePair<string, string>("t", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("p", String.Empty),
                    new KeyValuePair<string, string>("specifiedpost", "0"),
                    new KeyValuePair<string, string>("posthash", "6fd3840e9b2ed6a8dcc9d9d0432abb14"),
                    new KeyValuePair<string, string>("poststarttime", String.Empty),
                    new KeyValuePair<string, string>("loggedinuser",
                        session.User.Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("multiquoteempty", String.Empty),
                    new KeyValuePair<string, string>("sbutton", "Submit Reply"),
                    new KeyValuePair<string, string>("signature",
                        (settings & SectionPost.Settings.ShowSignature).ToString()),
                    new KeyValuePair<string, string>("parseurl", (settings & SectionPost.Settings.ParseUrl).ToString()),
                    new KeyValuePair<string, string>("parseame", "1"),
                    new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                    new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                    new KeyValuePair<string, string>("emailupdate", "9999"),
                    new KeyValuePair<string, string>("rating", "0"),
                    new KeyValuePair<string, string>("openclose", "0")
                });
        }

        /// <summary>
        ///     Retrieves a list of all posts in the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="firstPage"> Index of the first page to fetch </param>
        /// <param name="pageCount">
        ///     Amount of pages to get replies from. The higher this count, the more data will be generated
        ///     and received
        /// </param>
        /// <returns> List of <c>SectionPost</c>s representing the replies </returns>
        public List<SectionPost> Replies(Session session, uint pageCount = 1, uint firstPage = 1)
        {
            session.ThrowIfInvalid();
            if (Id == 0) throw new ArgumentException("ID must not be empty");

            // in case the amount of pages to fetch is greater than the available page count, set it to the maximum available count
            pageCount = (pageCount > PageCount && PageCount != 0) ? PageCount : pageCount;

            var retrievedReplies = new List<SectionPost>();
            for (uint i = 0; i < pageCount; ++i)
            {
                Response res = session.Get(GetUrl(i));
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(res.ToString());

                HtmlNode postsRootNode = htmlDocument.GetElementbyId("posts");
                if (postsRootNode == null) continue;

                foreach (HtmlNode postContainerNode in postsRootNode.GetElementsByTagName("div"))
                {
                    var fetchedPost = new SectionPost(0, this);
                    HtmlNode dateTimeNode =
                        postContainerNode.SelectSingleNode("div[1]/div[1]/div[1]/table[1]/tr[1]/td[1]/text()[3]");
                    fetchedPost.Date = (dateTimeNode != null)
                        ? dateTimeNode.InnerText.ToElitepvpersDateTime()
                        : new DateTime();

                    HtmlNode postRootNode = postContainerNode.SelectSingleNode("div[1]/div[1]/div[1]/table[1]/tr[2]");
                    if (postRootNode == null) continue;

                    HtmlNode userPartNode = postRootNode.SelectSingleNode("td[1]");
                    if (userPartNode != null)
                        new ReplyCreatorParser(fetchedPost.Sender).Execute(userPartNode);

                    HtmlNode messagePartNode;
                    // due to the (optional) title nodes users can set, another div will be inserted sometimes before the actual content
                    HtmlNode titleNode = postRootNode.SelectSingleNode("td[2]/div[1]/strong[1]/text()[1]");
                    if (titleNode != null)
                    {
                        fetchedPost.Title = titleNode.InnerText;
                        messagePartNode = postRootNode.SelectSingleNode("td[2]/div[2]");
                    }
                    else
                        messagePartNode = postRootNode.SelectSingleNode("td[2]/div[1]");

                    if (messagePartNode != null)
                    {
                        Match idMatch = new Regex("post_message_([0-9]+)").Match(messagePartNode.Id);
                        if (idMatch.Groups.Count > 1)
                            fetchedPost.Id = Convert.ToUInt32(idMatch.Groups[1].Value);

                        fetchedPost.Content = String.Join(String.Empty,
                            messagePartNode.GetElementsByTagName("#text").Select(node => node.InnerText));
                    }

                    retrievedReplies.Add(fetchedPost);
                }

                if (i != 0 || retrievedReplies.Count == 0) continue;
                InitialPost = retrievedReplies.First();
                retrievedReplies.Remove(retrievedReplies.First());
            }

            return retrievedReplies;
        }


        /// <summary>
        ///     Retrieves the thread ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved thread ID </returns>
        public static uint FromUrl(string url)
        {
            Match match = Regex.Match(url, @"http://www.elitepvpers.com/forum/\S+/(\d+)-\S+.html");
            if (match.Groups.Count < 2)
                throw new ParsingFailedException("User could not be exported from the given URL");

            return Convert.ToUInt32(match.Groups[1].Value);
        }

        private class ReplyCreatorParser : TargetableParser<User>, INodeParser
        {
            public ReplyCreatorParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                HtmlNode postCreatorNode = coreNode.SelectSingleNode("div[1]/a[1]");
                if (postCreatorNode == null) return;
                Target.Id = postCreatorNode.Attributes.Contains("href")
                    ? Convert.ToUInt32(User.FromUrl(postCreatorNode.Attributes["href"].Value))
                    : 0;

                HtmlNode userNameNode = postCreatorNode.SelectSingleNode("span[1]") ??
                                        postCreatorNode.SelectSingleNode("text()[1]");
                Target.Name = (userNameNode != null) ? userNameNode.InnerText : "";

                HtmlNode userTitleNode = coreNode.SelectSingleNode("div[3]");
                Target.Title = (userTitleNode != null) ? userTitleNode.InnerText : "";

                new User.RankParser(Target).Execute(coreNode.SelectSingleNode("div[4]"));

                HtmlNode userAvatarNode = coreNode.SelectSingleNode("div[5]/a[1]/img[1]");
                if (userAvatarNode != null)
                    Target.AvatarUrl = userAvatarNode.Attributes.Contains("src")
                        ? userAvatarNode.Attributes["src"].Value
                        : "";

                HtmlNode additionalStatsNode = coreNode.SelectSingleNode("div[6]");
                // node that contains posts, thanks, elite*gold, the join date...                    
                if (additionalStatsNode == null) return;
                IEnumerable<HtmlNode> statsNodes = additionalStatsNode.GetElementsByTagName("div");

                if (statsNodes == null) return;
                // Loop through the nodes and check for their descriptors since the 
                // number of nodes depend on the user's rank and settings
                foreach (HtmlNode statsNode in statsNodes)
                    new ReplyCreatorStatisticParser(Target).Execute(statsNode);
            }
        }

        private class ReplyCreatorStatisticParser : TargetableParser<User>, INodeParser
        {
            public ReplyCreatorStatisticParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                string innerText = coreNode.InnerText;

                if (innerText.Contains("elite*gold"))
                {
                    HtmlNode elitegoldNode = coreNode.SelectSingleNode("text()[2]");
                    Target.EliteGold = (elitegoldNode != null)
                        ? Convert.ToInt32(new String(elitegoldNode.InnerText.Skip(2).ToArray()))
                        : 0;
                }
                else if (innerText.Contains("The Black Market"))
                {
                    HtmlNode tbmPositiveNode = coreNode.SelectSingleNode("span[1]");
                    Target.TbmProfile.Ratings.Positive = (tbmPositiveNode != null)
                        ? Convert.ToUInt32(tbmPositiveNode.InnerText)
                        : 0;

                    HtmlNode tbmNeutralNode = coreNode.SelectSingleNode("text()[3]");
                    Target.TbmProfile.Ratings.Neutral = (tbmNeutralNode != null)
                        ? Convert.ToUInt32(tbmNeutralNode.InnerText.TrimStart('/').TrimEnd('/'))
                        : 0;

                    HtmlNode tbmNegativeNode = coreNode.SelectSingleNode("span[2]");
                    Target.TbmProfile.Ratings.Negative = (tbmNegativeNode != null)
                        ? Convert.ToUInt32(tbmNegativeNode.InnerText)
                        : 0;
                }
                else if (innerText.Contains("Mediations"))
                {
                    HtmlNode positiveNode = coreNode.SelectSingleNode("span[1]");
                    Target.TbmProfile.Mediations.Positive = (positiveNode != null)
                        ? Convert.ToUInt32(positiveNode.InnerText)
                        : 0;

                    HtmlNode neutralNode = coreNode.SelectSingleNode("text()[2]");
                    Target.TbmProfile.Mediations.Neutral = (neutralNode != null)
                        ? Convert.ToUInt32(neutralNode.InnerText.TrimStart('/'))
                        : 0;
                }
                else if (innerText.Contains("Join Date"))
                {
                    // since the join date is formatted with the first 3 characters of the month + year, we'd need to use some regex here
                    // data may look as follows: Registriert seit: Jun 2007 (German)
                    Match match = new Regex(@"([a-zA-Z]{3})\s{1}([0-9]+)").Match(coreNode.InnerText);
                    if (match.Groups.Count != 3) return;
                    for (int j = 1; j <= 12; j++)
                    {
                        if (
                            CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(j)
                                .Contains(match.Groups[1].Value))
                            Target.JoinDate = new DateTime(Convert.ToInt32(match.Groups[2].Value), j, 1);
                    }
                }
                else if (innerText.Contains("Posts"))
                {
                    Match match = new Regex("(?:Posts): ([0-9]+(?:.|,)[0-9]+)").Match(coreNode.InnerText);
                    if (match.Groups.Count > 1)
                        Target.Posts = (uint) double.Parse(match.Groups[1].Value);
                }
                else if (innerText.Contains("Received Thanks"))
                {
                    Match match = new Regex("(?:Received Thanks): ([0-9]+(?:.|,)[0-9]+)").Match(coreNode.InnerText);
                    if (match.Groups.Count > 1)
                        Target.ThanksReceived = (uint) double.Parse(match.Groups[1].Value);
                }
            }
        }
    }
}