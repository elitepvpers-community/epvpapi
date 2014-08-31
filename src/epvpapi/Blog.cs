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

            public void Publish<T>(UserSession<T> session, DateTime publishDate) where T : User
            {
                session.ThrowIfInvalid();

                string tags = "";
                foreach(var tag in Tags)
                {
                    tags += tag;
                    if(Tags.Last() != tag)
                        tags += ",";
                }

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
                                new KeyValuePair<string, string>("allowcomments", "1"),
                                new KeyValuePair<string, string>("moderatecomments", "1"),
                                new KeyValuePair<string, string>("private", "1"),
                                new KeyValuePair<string, string>("status", "publish_now"),
                                new KeyValuePair<string, string>("publish[month]", publishDate.Month.ToString()),
                                new KeyValuePair<string, string>("publish[day]", publishDate.Day.ToString()),
                                new KeyValuePair<string, string>("publish[year]", publishDate.Year.ToString()),
                                new KeyValuePair<string, string>("publish[hour]", publishDate.Hour.ToString()),
                                new KeyValuePair<string, string>("publish[minute]", publishDate.Minute.ToString()),
                                new KeyValuePair<string, string>("parseurl", "1"),
                                new KeyValuePair<string, string>("parseame", "1"),
                                new KeyValuePair<string, string>("emailupdate", "none"),
                                new KeyValuePair<string, string>("sbutton", "Submit")
                            });

                Date = publishDate;
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
