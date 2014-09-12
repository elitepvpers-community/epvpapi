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

        public Post(uint id, User sender, string content, string title = null)
            : base(content)
        {
            Title = title;
            Sender = sender;
        }

        public Post(uint id, string content, string title = null)
            : this(id, new User(), content, title)
        { }

        public Post(string content, string title = null)
            : this(0, content, title)
        { }

        public Post(uint id)
            : this(id, null, null)
        { }
    }
}
