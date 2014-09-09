using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents a thread within a section
    /// </summary>
    public class SectionThread : Thread, IReasonableDeletable
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
        /// List of all posts in the thread
        /// </summary>
        public List<SectionPost> Posts { get; set; }

        /// <summary>
        /// Current average rating of the <c>SectionThread</c>
        /// </summary>
        public uint Rating { get; set; }

        /// <summary>
        /// Amount of views that have been recorded
        /// </summary>
        public uint Views { get; set; }

        public string Title
        {
            get { return (Posts.Count > 0) ? Posts.First().Title : ""; }
            set { Posts.First().Title = (Posts.Count > 0) ? value : ""; }
        }

        public override string URL
        {
            get { return "http://www.elitepvpers.com/forum/" + Section.URLName + "/" 
                                                             + ID + "-" + Title.URLEscape() 
                                                             + ".html"; }
        }

        public SectionThread(Section section)
            : this(0, section)
        { }

        public SectionThread(uint id, Section section)
            : base(id)
        {
            Section = section;
            Posts = new List<SectionPost>();
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
        public static SectionThread Create(ProfileSession<User> session, Section section, SectionPost startPost,
                                           SectionPost.Settings settings = SectionPost.Settings.ParseURL | SectionPost.Settings.ShowSignature,
                                           bool closed = false)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newthread.php?do=postthread&f=" + section.ID,
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("subject", startPost.Title),
                            new KeyValuePair<string, string>("message", startPost.Content),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("taglist", String.Empty),
                            new KeyValuePair<string, string>("iconid", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("f", section.ID.ToString()),
                            new KeyValuePair<string, string>("do", "postthread"),
                            new KeyValuePair<string, string>("posthash", "74532335f4d3a9f352db6af1b1c257f7"),
                            new KeyValuePair<string, string>("poststarttime", "1389309192"),
                            new KeyValuePair<string, string>("loggedinuser", session.ConnectedProfile.User.ID.ToString()),
                            new KeyValuePair<string, string>("sbutton", "Submit New Thread"),
                            new KeyValuePair<string, string>("signature", Convert.ToInt32(settings.HasFlag(SectionPost.Settings.ShowSignature)).ToString()),
                            new KeyValuePair<string, string>("parseurl", Convert.ToInt32(settings.HasFlag(SectionPost.Settings.ParseURL)).ToString()),
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
        public void Delete<T>(ProfileSession<T> session, string reason) where T : User
        {
            if (session.ConnectedProfile.User.GetHighestRank() < User.Rank.GlobalModerator) throw new InsufficientAccessException("You don't have enough access rights to delete this thread");
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
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
        /// Closes the <c>SectionThread</c> if it is open
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        /// Switch function - If the thread is closed, it will be opened and vice versa when executing this function
        /// Not tested yet!
        /// </remarks>
        public void Close(Session session)
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
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
        /// Opens the <c>SectionThread</c> if it is closed
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        /// Not tested yet!
        /// </remarks>
        public void Open(Session session)
        {
            Close(session);
            Closed = false;
        }


        /// <summary>
        /// Rates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="rating"> Represents the rate value (0-5) </param>
        public void Rate(Session session, uint rating)
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
        public void Reply(ProfileSession<User> session, SectionPost post,
                          SectionPost.Settings settings = SectionPost.Settings.ParseURL | SectionPost.Settings.ShowSignature)
        {
            if (ID == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/newreply.php?do=postreply&t=" + ID,
                         new List<KeyValuePair<string, string>>() 
                         { 
                             new KeyValuePair<string, string>("title", post.Title),
                             new KeyValuePair<string, string>("message", post.Content),
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
                             new KeyValuePair<string, string>("loggedinuser", session.ConnectedProfile.User.ID.ToString()),
                             new KeyValuePair<string, string>("multiquoteempty", String.Empty),
                             new KeyValuePair<string, string>("sbutton", "Submit Reply"),
                             new KeyValuePair<string, string>("signature", (settings & SectionPost.Settings.ShowSignature).ToString()),
                             new KeyValuePair<string, string>("parseurl", (settings & SectionPost.Settings.ParseURL).ToString()),
                             new KeyValuePair<string, string>("parseame", "1"),
                             new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                             new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                             new KeyValuePair<string, string>("emailupdate", "9999"),
                             new KeyValuePair<string, string>("rating", "0"),
                             new KeyValuePair<string, string>("openclose", "0")
                         });

            Posts.Add(post);
        }

        /// <summary>
        /// Retrieves the thread ID of the given URL
        /// </summary>
        /// <param name="url"> URL being parsed </param>
        /// <returns> Retrieved thread ID </returns>
        public static uint FromURL(string url)
        {
            var match = Regex.Match(url, @"http://www.elitepvpers.com/forum/\S+/(\d+)-\S+.html");
            if (match.Groups.Count < 2) throw new ParsingFailedException("User could not be exported from the given URL");

            return Convert.ToUInt32(match.Groups[1].Value);
        }
    }
}
