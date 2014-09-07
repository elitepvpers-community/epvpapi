using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public abstract class Thread : UniqueWebObject 
    {
        public User Creator { get; set; }
        public bool Deleted { get; set; }
        public uint Replies { get; set; }

        public Thread(uint id)
            : base(id)
        {
            Creator = new User();
        }

        public Thread()
            : this(0)
        { }
    }
}
    