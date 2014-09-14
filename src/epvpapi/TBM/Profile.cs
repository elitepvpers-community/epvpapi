using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.TBM
{
    public class Profile
    {
        public Ratings Ratings { get; set; }
        public Mediations Mediations { get; set; }

        public Profile()
        {
            Ratings = new Ratings();
            Mediations = new Mediations();
        }
    }
}
