﻿namespace epvpapi
{
    public abstract class Thread : UniqueObject
    {
        public User Creator { get; set; }
        public bool Deleted { get; set; }
        public uint ReplyCount { get; set; }

        public Thread(int id)
            : base(id)
        {
            Creator = new User();
        }

        public Thread()
            : this(0)
        { }
    }
}
