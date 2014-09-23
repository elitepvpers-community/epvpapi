using System.Collections.Generic;

namespace epvpapi
{
    public abstract class Post : Message
    {
        /// <summary>
        /// Title of the <c>Post</c>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Creator of the <c>Post</c>
        /// </summary>
        public User Sender { get; set; }

        public Post(uint id, User sender, List<VBContent> contents, string title = null)
            : base(id, contents)
        {
            Title = title;
            Sender = sender;
        }

        public Post(uint id, List<VBContent> contents, string title = null)
            : this(id, new User(), contents, title)
        { }

        public Post(List<VBContent> contents, string title = null)
            : this(0, contents, title)
        { }

        public Post(uint id)
            : this(id, null, null)
        { }
    }
}
