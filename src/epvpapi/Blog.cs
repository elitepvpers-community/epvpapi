using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class Blog : UniqueObject
    {
        public uint Entries { get; set; }
        public DateTime LastEntry { get; set; }

        public Blog(uint id = 0):
            base(id)
        { }
    }
}
