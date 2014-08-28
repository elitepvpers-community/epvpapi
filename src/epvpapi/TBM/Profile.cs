using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.TBM
{
    public class Profile
    {
        public uint Positive { get; set; }
        public uint Neutral { get; set; }
        public uint Negative { get; set; }

        public Profile(uint positive = 0, uint neutral = 0, uint negative = 0)
        {
            Positive = positive;
            Neutral = neutral;
            Negative = negative;
        }
    }
}
