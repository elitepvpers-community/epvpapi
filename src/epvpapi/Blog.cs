using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi
{
    /// <summary>
    /// Every user got an own blog that is accessible for everyone and writeable for the owner
    /// </summary>
    public class Blog : UniqueObject, IUniqueWebObject
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

            public Entry(uint id, Content content, string title = null)
                : base(id, content, title)
            {
                Tags = new List<string>();
            }

            public Entry(Content content, string title = null)
                : this(0, content, title)
            { }

            public Entry(uint id)
                : this(id, new Content())
            { }

            public string GetUrl()
            {
                return "http://www.elitepvpers.com/forum/blogs/" + Blog.Owner.ID + "-" + Blog.Owner.Name.UrlEscape() + "/" + ID + "-" + Title.UrlEscape() + ".html";
            }
        }

        public List<Entry> Entries { get; set; }
        public DateTime LastEntry { get; set; }
        public User Owner { get; set; }

        public Blog(User owner) :
            base(owner.ID)
        {
            Entries = new List<Entry>();
            Owner = owner;
        }

        /// <summary>
        /// Gets the url of the blog
        /// </summary>
        /// <returns> The url of the blog </returns>
        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/blogs/" + Owner.ID + "-" + Owner.Name.UrlEscape() + ".html";
        }   
    }
}
