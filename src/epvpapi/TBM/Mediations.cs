using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.TBM
{
    public class Mediations
    {
        public uint Positive { get; set; }
        public uint Neutral { get; set; }

        public Mediations(uint positive = 0, uint neutral = 0)
        {
            Positive = positive;
            Neutral = neutral;
        }
    }
}
