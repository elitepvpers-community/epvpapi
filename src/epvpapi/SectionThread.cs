using System.Globalization;
using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using epvpapi.Evaluation;

namespace epvpapi
{
    /// <summary>
    /// Represents a thread within a section
    /// </summary>
    public class SectionThread : Thread, IReasonableDeletable, IUpdatable, IUniquePageableWebObject
    {
        /// <summary>
        /// Section under which the <c>SectionThread</c> is listed
        /// </summary>
        public Section Section { get; set; }
        
        /// <summary>
        /// If true, the thread state is closed meaning that no one (except the staff) can answer to this thread
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// If true, the thread was sticked from a moderator and will always stay at the top section of the threads
        /// </summary>
        public bool Sticked { get; set; }

        /// <summary>
        /// Preview of the thread content being displayed when hovering over the link
        /// </summary>
        public string PreviewContent { get; set; }

        /// <summary>
        /// Current average rating of the <c>SectionThread</c>
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Amount of views that have been recorded
        /// </summary>
        public uint ViewCount { get; set; }

        /// <summary>
        /// The first post in the <c>SectionThread</c>, usually created by the thread starter
        /// </summary>
        public SectionPost InitialPost { get; set; }
        
        /// <summary>
        /// Tags that have been set for better search results when using the board's search function
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Amount of pages 
        /// </summary>
        public uint PageCount { get; set; }

        /// <summary>
        /// Amount of posts within all pages
        /// </summary>
        public uint PostCount { get; set; }

        public SectionThread(Section section)
            : this(0, section)
        { }

        public SectionThread(int id, Section section)
            : base(id)
        {
            Section = section;
            InitialPost = new SectionPost(0, this);
            Tags = new List<string>();
            PageCount = 1;
        }

        /// <summary>
        /// Creates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="section"> Section under which the <c>SectionThread</c> is listed </param>
        /// <param name="startPost"> Represents the content and title of the <c>SectionThread</c> </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <param name="closed"> If true, the thread state is closed meaning that no one (except the staff) can answer to this thread </param>
        /// <returns> Freshly created <c>SectionThread</c> </returns>
        public static SectionThread Create<TUser>(AuthenticatedSession<TUser> session, Section section, SectionPost startPost,
                                                   SectionPost.Settings settings = SectionPost.Settings.ParseUrl | SectionPost.Settings.ShowSignature,
                                                   bool closed = false)
                                                   where TUser : User
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newthread.php?do=postthread&f=" + section.ID,
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("subject", String.IsNullOrEmpty(startPost.Title) ? "-" : startPost.Title),
                            new KeyValuePair<string, string>("message", startPost.Content.ToString()),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("taglist", String.Empty),
                            new KeyValuePair<string, string>("iconid", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("f", section.ID.ToString()),
                            new KeyValuePair<string, string>("do", "postthread"),
                            new KeyValuePair<string, string>("posthash", "74532335f4d3a9f352db6af1b1c257f7"),
                            new KeyValuePair<string, string>("poststarttime", "1389309192"),
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("sbutton", "Submit New Thread"),
                            new KeyValuePair<string, string>("signature", settings.HasFlag(SectionPost.Settings.ShowSignature) ? "1" : "0"),
                            new KeyValuePair<string, string>("parseurl",  settings.HasFlag(SectionPost.Settings.ParseUrl) ? "1" : "0"),
                            new KeyValuePair<string, string>("parseame", "1"),
                            new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                            new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                            new KeyValuePair<string, string>("emailupdate", "9999"),
                            new KeyValuePair<string, string>("polloptions", "4")
                        });

            return new SectionThread(0, section);
        }

        /// <summary>
        /// Deletes the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        /// <remarks>
        /// Not tested yet!
        /// </remarks>
        public void Delete<TUser>(AuthenticatedSession<TUser> session, string reason) where TUser : User
        {
            if (session.User.GetHighestRank() < Usergroup.GlobalModerator) throw new InsufficientAccessException("You don't have enough access rights to delete this thread");
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/postings.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("t", ID.ToString()),
                            new KeyValuePair<string, string>("do", "dodeletethread"),
                            new KeyValuePair<string, string>("deletetype", "1"),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("deletereason", reason),

                        });

            Deleted = true;
        }

        /// <summary>
        /// Opens or closes the <c>SectionThread</c>, depends on the current status. 
        /// If the thread is opened, it will be closed. If it is closed, it will be opened
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        public void ToggleStatus<TUser>(AuthenticatedSession<TUser> session) where TUser : User
        {
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/postings.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("t", ID.ToString()),
                            new KeyValuePair<string, string>("do", "openclosethread"),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("pollid", String.Empty),
                        });

            Closed = true;
        }


        /// <summary>
        /// Rates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="rating"> Represents the rate value (0-5) </param>
        public void Rate<TUser>(AuthenticatedSession<TUser> session, uint rating) where TUser : User
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/threadrate.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("ajax", "1"),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("vote", rating.ToString()),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("t", ID.ToString()),
                            new KeyValuePair<string, string>("pp", "10"),
                            new KeyValuePair<string, string>("page", "1")
                        });
        }

        /// <summary>
        /// Replies to the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="post"> Reply to post </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        /// The ID of the thread has to be given in order to reply
        /// </remarks>
        public void Reply<TUser>(AuthenticatedSession<TUser> session, SectionPost post,
                                SectionPost.Settings settings = SectionPost.Settings.ParseUrl | SectionPost.Settings.ShowSignature)
                                where TUser : User
        {
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newreply.php?do=postreply&t=" + ID,
                         new List<KeyValuePair<string, string>>() 
                         { 
                             new KeyValuePair<string, string>("title", String.IsNullOrEmpty(post.Title) ? "-" : post.Title),
                             new KeyValuePair<string, string>("message", post.Content.ToString()),
                             new KeyValuePair<string, string>("wysiwyg", "0"),
                             new KeyValuePair<string, string>("iconid", post.Icon.ToString()),
                             new KeyValuePair<string, string>("s", String.Empty),
                             new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                             new KeyValuePair<string, string>("do", "postreply"),
                             new KeyValuePair<string, string>("t", ID.ToString()),
                             new KeyValuePair<string, string>("p", String.Empty),
                             new KeyValuePair<string, string>("specifiedpost", "0"),
                             new KeyValuePair<string, string>("posthash", "6fd3840e9b2ed6a8dcc9d9d0432abb14"),
                             new KeyValuePair<string, string>("poststarttime", String.Empty),
                             new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                             new KeyValuePair<string, string>("multiquoteempty", String.Empty),
                             new KeyValuePair<string, string>("sbutton", "Submit Reply"),
                             new KeyValuePair<string, string>("signature", settings.HasFlag(SectionPost.Settings.ShowSignature) ? "1" : "0"),
                             new KeyValuePair<string, string>("parseurl", settings.HasFlag(SectionPost.Settings.ParseUrl) ? "1" : "0"),
                             new KeyValuePair<string, string>("parseame", "1"),
                             new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                             new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                             new KeyValuePair<string, string>("emailupdate", "9999"),
                             new KeyValuePair<string, string>("rating", "0"),
                             new KeyValuePair<string, string>("openclose", "0")
                         });
        }

        /// <summary>
        /// Retrieves information about the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <remarks>
        /// ID and the section represented by a <c>Section</c> object is required in order to update.
        /// </remarks>
        public void Update(GuestSession session)
        {
            if(ID == 0) throw new ArgumentException("ID must not be empty");
            if(String.IsNullOrEmpty(Section.UrlName)) throw new ArgumentException("The section needs to be addressable");

            var res = session.Get(GetUrl());
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res.ToString());

            var postsRootNode = htmlDocument.GetElementbyId("posts");
            if (postsRootNode == null) return;

            Closed = htmlDocument.DocumentNode.Descendants().GetElementsByName("img")
                    .Any(node => node.Attributes.Contains("src")
                                ? node.Attributes["src"].Value.Contains("threadclosed.gif")
                                : false);

            var currentRatingNode = htmlDocument.GetElementbyId("threadrating_current");
            if (currentRatingNode != null)
            {
                currentRatingNode = currentRatingNode.SelectSingleNode("img[1]");
                if (currentRatingNode != null)
                {
                    if (currentRatingNode.Attributes.Contains("alt"))
                    {
                        var ratingMatch = new Regex(@",\s+([0-9](?:,|.)[0-9]{2})\s+\S+").Match(currentRatingNode.Attributes["alt"].Value);
                        if (ratingMatch.Groups.Count > 1)
                            Rating = ratingMatch.Groups[1].Value.To<double>();
                    }
                }
            }

            var tagsRootNode = htmlDocument.GetElementbyId("tag_list_cell");
            if (tagsRootNode != null)
            {
                foreach (var tagNode in tagsRootNode.ChildNodes.GetElementsByTagName("a"))
                    Tags.Add(tagNode.InnerText);
            }

            var pageCountRootNode = htmlDocument.GetElementbyId("poststop");
            if (pageCountRootNode != null)
            {
                pageCountRootNode = pageCountRootNode.SelectSingleNode("following-sibling::table[1]/tr[1]/td[2]/div[1]/table[1]/tr[1]/td[1]");
                if (pageCountRootNode != null)
                {
                    var countMatch = new Regex(@"\S+\s{1}[0-9]+\s{1}\S+\s{1}([0-9]+)").Match(pageCountRootNode.InnerText);
                    if (countMatch.Groups.Count > 1)
                        PageCount = countMatch.Groups[1].Value.To<uint>();
                }
            }
        }

        /// <summary>
        /// Retrieves a list of all posts in the <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="firstPage"> Index of the first page to fetch </param>
        /// <param name="pageCount"> Amount of pages to get replies from. The higher this count, the more data will be generated and received </param>
        /// <returns> List of <c>SectionPost</c>s representing the replies </returns>
        public List<SectionPost> Replies<TUser>(AuthenticatedSession<TUser> session, uint pageCount = 1, uint firstPage = 1) where TUser : User
        {
            session.ThrowIfInvalid();
            if(ID == 0) throw new ArgumentException("ID must not be empty");

            // in case the amount of pages to fetch is greater than the available page count, set it to the maximum available count
            pageCount = (pageCount > PageCount && PageCount != 0) ? PageCount : pageCount;

            var retrievedReplies = new List<SectionPost>();
            for (uint i = firstPage; i <= (firstPage + pageCount); ++i)
            {
                var res = session.Get(GetUrl(i));
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(res.ToString());

                var postsRootNode = htmlDocument.GetElementbyId("posts");
                if (postsRootNode == null) continue;

                // for some reason, the lastpost container contains nothing and needs to be filtered out 
                foreach (var postContainerNode in postsRootNode.ChildNodes.GetElementsByTagName("div").Where(element => element.Id != "lastpost"))
                {
                    var parsedPost = new SectionPost(0, this);
                    new SectionPostParser(parsedPost).Execute(postContainerNode);

                    retrievedReplies.Add(parsedPost);
                }

                // store the starting post and 
                // remove it after storing since it is no reply
                if (i == 1 && retrievedReplies.Count != 0)
                {
                    InitialPost = retrievedReplies.First();
                    retrievedReplies.Remove(retrievedReplies.First());
                }
            }

            return retrievedReplies;
        }

        public string GetUrl(uint pageIndex = 1)
        {
            return "http://www.elitepvpers.com/forum/" + Section.UrlName + "/"
                + ID + "-" + InitialPost.Title.UrlEscape()
                + ((pageIndex > 1) ? "-" + pageIndex : "")
                + ".html";
        }

        /// <summary>
        /// Retrieves the thread ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved thread ID </returns>
        public static int FromUrl(string url)
        {
            var match = Regex.Match(url, @"http://www.elitepvpers.com/forum/\S+/(\d+)-\S+.html");
            if (match.Groups.Count < 2) throw new ParsingFailedException("User could not be exported from the given URL");

            return match.Groups[1].Value.To<int>();
        }
    }
}
