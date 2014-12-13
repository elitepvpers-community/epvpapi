using System;
using System.Collections.Generic;

namespace epvpapi
{
    public abstract class Post : Message
    {
        /// <summary>
        /// Title of the <c>Post</c>
        /// </summary>
        public string Title { get; set; }

        public Post(int id = 0)
            : this(id, new Content())
        { }
        
        public Post(Content content)
            : this(0, content)
        { }

        public Post(int id, Content content, string title = null)
            : base(id, content)
        {
            Title = title;
        }
    }
}
