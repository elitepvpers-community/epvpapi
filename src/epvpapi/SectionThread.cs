using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// List of all posts in the thread
        /// </summary>
        public List<SectionPost> Posts { get; set; }

        /// <summary>
        /// Current average rating of the <c>SectionThread</c>
        /// </summary>
        public uint Rating { get; set; }


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
        /// <param name="closed"> If true, the thread state is closed meaning that no one (except the staff) can answer to this thread </param>
        /// <returns> Freshly created <c>SectionThread</c> </returns>
        public static SectionThread Create(Session session, Section section, SectionPost startPost, bool closed)
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
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("sbutton", "Submit New Thread"),
                            new KeyValuePair<string, string>("signature", Convert.ToInt32(startPost.Settings.HasFlag(Message.Options.ShowSignature)).ToString()),
                            new KeyValuePair<string, string>("parseurl", Convert.ToInt32(startPost.Settings.HasFlag(Message.Options.ParseURL)).ToString()),
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
        public void Delete(Session session, string reason)
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");

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

            session.Post("http://www.elitepvpers.com/forum/postings.php",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("t", ID.ToString()),
                            new KeyValuePair<string, string>("do", "openclosethread"),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("pollid", String.Empty),
                        });
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
        }


        /// <summary>
        /// Rates a <c>SectionThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="rating"> Represents the rate value (0-5) </param>
        public void Rate(Session session, uint rating)
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");

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
        /// <remarks>
        /// The ID of the thread has to be given in order to reply
        /// </remarks>
        public void Reply(Session session, SectionPost post)
        {
            if (ID == 0) throw new ArgumentException("ID must not be empty");

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
                             new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                             new KeyValuePair<string, string>("multiquoteempty", String.Empty),
                             new KeyValuePair<string, string>("sbutton", "Submit Reply"),
                             new KeyValuePair<string, string>("signature", (post.Settings & SectionPost.Options.ShowSignature).ToString()),
                             new KeyValuePair<string, string>("parseurl", (post.Settings & SectionPost.Options.ParseURL).ToString()),
                             new KeyValuePair<string, string>("parseame", "1"),
                             new KeyValuePair<string, string>("vbseo_retrtitle", "1"),
                             new KeyValuePair<string, string>("vbseo_is_retrtitle", "1"),
                             new KeyValuePair<string, string>("emailupdate", "9999"),
                             new KeyValuePair<string, string>("rating", "0"),
                             new KeyValuePair<string, string>("openclose", "0")
                         });
        }
    }
}
