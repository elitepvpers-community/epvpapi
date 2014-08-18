using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class EliteGold
    {
        public int Amount { get; set; }

        public EliteGold()
            : this(0)
        { }

        public EliteGold(int amount)
        {
            Amount = amount;
        }
    }
}
