namespace epvpapi
{
    public abstract class Thread : UniqueObject
    {
        protected Thread(uint id)
            : base(id)
        {
            Creator = new User();
        }

        protected Thread()
            : this(0)
        {
        }

        public User Creator { get; set; }
        protected bool Deleted { get; set; }
        public uint ReplyCount { get; set; }
    }
}