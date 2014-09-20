namespace epvpapi
{
    public abstract class Post : Message
    {
        private Post(uint id, User sender, string content, string title = null)
            : base(id, content)
        {
            Title = title;
            Sender = sender;
        }

        protected Post(uint id, string content, string title = null)
            : this(id, new User(), content, title)
        {
        }

        protected Post(string content, string title = null)
            : this(0, content, title)
        {
        }

        protected Post(uint id)
            : this(id, null, null)
        {
        }

        /// <summary>
        ///     Title of the <c>Post</c>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Creator of the <c>Post</c>
        /// </summary>
        public User Sender { get; set; }
    }
}