using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class Blog : UniqueObject
    {
        public class Entry : Post
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
                ParseURL = 1,

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

            public Entry(uint id, string content, string title = null)
                : base(id, content, title)
            {
                Tags = new List<string>();
            }

            public Entry(string content, string title = null)
                : this(0, content, title)
            { }

            public Entry(uint id)
                : this(id, null)
            { }

            public void Publish<T>(UserSession<T> session, 
                                   Settings settings = Settings.ParseURL | Settings.AllowComments) where T : User
            {
                session.ThrowIfInvalid();

                string tags = "";
                foreach(var tag in Tags)
                {
                    tags += tag;
                    if(Tags.Last() != tag)
                        tags += ",";
                }

                Date = DateTime.Now;

                session.Post("http://www.elitepvpers.com/forum/blog_post.php?do=updateblog&blogid=",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("title", Title),
                                new KeyValuePair<string, string>("message", Content),
                                new KeyValuePair<string, string>("wysiwyg", "0"),
                                new KeyValuePair<string, string>("s", String.Empty),
                                new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                                new KeyValuePair<string, string>("do", "updateblog"),
                                new KeyValuePair<string, string>("b", String.Empty),
                                new KeyValuePair<string, string>("posthash", String.Empty),
                                new KeyValuePair<string, string>("poststarttime", DateTime.Now.UNIXTimestamp().ToString()),
                                new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                                new KeyValuePair<string, string>("u", String.Empty),
                                new KeyValuePair<string, string>("taglist", tags),
                                new KeyValuePair<string, string>("allowcomments", (settings & Settings.AllowComments).ToString()),
                                new KeyValuePair<string, string>("moderatecomments", (settings & Settings.ModerateComments).ToString()),
                                new KeyValuePair<string, string>("private", (settings & Settings.Private).ToString()),
                                new KeyValuePair<string, string>("status", "publish_now"),
                                new KeyValuePair<string, string>("publish[month]", Date.Month.ToString()),
                                new KeyValuePair<string, string>("publish[day]", Date.Day.ToString()),
                                new KeyValuePair<string, string>("publish[year]", Date.Year.ToString()),
                                new KeyValuePair<string, string>("publish[hour]", Date.Hour.ToString()),
                                new KeyValuePair<string, string>("publish[minute]", Date.Minute.ToString()),
                                new KeyValuePair<string, string>("parseurl", (settings & Settings.ParseURL).ToString()),
                                new KeyValuePair<string, string>("parseame", "1"),
                                new KeyValuePair<string, string>("emailupdate", "none"),
                                new KeyValuePair<string, string>("sbutton", "Submit")
                            });
            }
        }

        public List<Entry> Entries { get; set; }
        public DateTime LastEntry { get; set; }

        public Blog(uint id = 0):
            base(id)
        {
            Entries = new List<Entry>();
        }
    }
}
