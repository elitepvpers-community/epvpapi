using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class Announcement : Post, IUniqueWebObject
    {
        public DateTime Begins { get; set; }
        public DateTime Ends { get; set; }
        public uint Hits { get; set; }
        public Section Section { get; set; }

        public Announcement(Section section, int id = 0)
            : base(id)
        {
            Section = section;
            Begins = new DateTime();
            Ends = new DateTime();
        }

        public string GetUrl()
        {
            return String.Format("http://www.elitepvpers.com/forum/{0}/announcement-{1}.html", Section.Shortname, Title.UrlEscape());
        }
    }
}
