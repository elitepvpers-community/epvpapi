using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using epvpapi.Connection;

namespace epvpapi
{
    public class BlogEntry : Post
    {
        /// <summary>
        /// Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public new enum Settings
        {
            /// <summary>
            /// If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,

            /// <summary>
            /// Users may reply to the entry using the built-in comment feature if this flag has been set
            /// </summary>
            AllowComments = 3,

            /// <summary>
            /// If set, comments need to be approved first before they are shown to the public. 
            /// While they are not approved, only the staff and you, the owner, can see the comments
            /// </summary>
            ModerateComments = 4,
                
            /// <summary>
            /// If set, the blog entry is only visible to the staff and yourself
            /// </summary>
            Private = 5
        }

        public List<string> Tags { get; set; }
        public Blog Blog { get; set; }

        public BlogEntry(int id, Content content, string title = null)
            : base(id, content, title)
        {
            Tags = new List<string>();
        }

        public BlogEntry(Content content, string title = null)
            : this(0, content, title)
        { }

        public BlogEntry(int id)
            : this(id, new Content())
        { }


        /// <summary>
        /// Publishes the <c>Blog.Entry</c> in the logged-in user's blog
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        public void Publish<TUser>(AuthenticatedSession<TUser> session, Settings settings = Settings.ParseUrl | Settings.AllowComments)
                                    where TUser : User
        {
            Publish(session, DateTime.Now, settings);
        }

        /// <summary>
        /// Publishes the <c>Blog.Entry</c> in the logged-in user's blog at the given time (automatically)
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="publishDate"> Date and time when the entry will go live </param>
        /// <param name="settings"> Additional options that can be set </param>
        public void Publish<TUser>(AuthenticatedSession<TUser> session, DateTime publishDate, Settings settings = Settings.ParseUrl |Settings.AllowComments)
                                    where TUser : User
        {
            session.ThrowIfInvalid();

            Date = publishDate;
            var tags = Tags.Aggregate("", (current, tag) => current + (tag + (Tags.Last() != tag ? "," : "")));

            session.Post("http://www.elitepvpers.com/forum/blog_post.php?do=updateblog&blogid=",
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("title", Title),
                    new KeyValuePair<string, string>("message", Content.ToString()),
                    new KeyValuePair<string, string>("wysiwyg", "0"),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "updateblog"),
                    new KeyValuePair<string, string>("b", String.Empty),
                    new KeyValuePair<string, string>("posthash", String.Empty),
                    new KeyValuePair<string, string>("poststarttime", Extensions.UnixTimestamp().ToString()),
                    new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                    new KeyValuePair<string, string>("u", String.Empty),
                    new KeyValuePair<string, string>("taglist", tags),
                    new KeyValuePair<string, string>("allowcomments", Convert.ToUInt32(settings.HasFlag(Settings.AllowComments)).ToString()),
                    new KeyValuePair<string, string>("moderatecomments", Convert.ToUInt32(settings.HasFlag(Settings.ModerateComments)).ToString()),
                    new KeyValuePair<string, string>("private", Convert.ToUInt32(settings.HasFlag(Settings.Private)).ToString()),
                    new KeyValuePair<string, string>("status", (publishDate.Compare(DateTime.Now)) ? "publish_now" : "publish_on"),
                    new KeyValuePair<string, string>("publish[month]", Date.Month.ToString()),
                    new KeyValuePair<string, string>("publish[day]", Date.Day.ToString()),
                    new KeyValuePair<string, string>("publish[year]", Date.Year.ToString()),
                    new KeyValuePair<string, string>("publish[hour]", Date.Hour.ToString()),
                    new KeyValuePair<string, string>("publish[minute]", Date.Minute.ToString()),
                    new KeyValuePair<string, string>("parseurl", Convert.ToUInt32(settings.HasFlag(Settings.ParseUrl)).ToString()),
                    new KeyValuePair<string, string>("parseame", "1"),
                    new KeyValuePair<string, string>("emailupdate", "none"),
                    new KeyValuePair<string, string>("sbutton", "Submit")
                });
        }


        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/blogs/" + Blog.Owner.ID + "-" + Blog.Owner.Name.UrlEscape() + "/" + ID + "-" + Title.UrlEscape() + ".html";
        }
    }
}
