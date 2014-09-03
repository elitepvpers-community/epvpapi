using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Represents a category in elitepvpers
    /// </summary>
    public class Section : UniqueObject
    {
        public class Announcement : Post
        {
            public DateTime Begins { get; set; }
            public DateTime Ends { get; set; }
            public uint Hits { get; set; }
            public User Creator { get; set; }

            public Announcement(uint id = 0)
                : base(id)
            {
                Begins = new DateTime();
                Ends = new DateTime();
                Creator = new User();
            }
        }

        /// <summary>
        /// Name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL name that is shown in the address bar of the browser, mostly needed for updating information
        /// </summary>
        public string URLName { get; set; }

        /// <summary>
        /// Short description what the section is about 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of all <c>SectionThread</c> objects representing the threads in the section
        /// </summary>
        public List<SectionThread> Threads { get; set; }

        /// <summary>
        /// List of all announcements available for this section
        /// </summary>
        public List<Announcement> Announcements { get; set; }

        /// <summary>
        /// Some sections may have subsections that are located within the owning section
        /// </summary>
        public List<Section> Subsections { get; set; }

        public Section(uint id)
            : base(id)
        {
            Threads = new List<SectionThread>();
            Subsections = new List<Section>();
        }

        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (URLName == String.Empty) throw new ArgumentException("Sections cannot be updated if no url-address-name is provided");

            Response res = session.Get("http://www.elitepvpers.com/forum/" + URLName + "/");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            UpdateAnnouncements(doc);
        }

        protected void UpdateAnnouncements(HtmlDocument doc)
        {
            Announcements = new List<Announcement>();

            var threadListNode = doc.GetElementbyId("threadslist"); 
            if (threadListNode != null)
            {
                threadListNode = threadListNode.SelectSingleNode("tbody");
                var sectionNodes = new List<HtmlNode>(threadListNode.GetElementsByTagName("tr"));

                foreach (var announcementNode in sectionNodes.Take(sectionNodes.Count - 1)) // ignore the last node since that is no actual announcement
                {
                    Announcement announcement = new Announcement();

                    var firstLine = announcementNode.SelectSingleNode("td[2]/div[1]");
                    if (firstLine != null)
                    {
                        var hitsNode = firstLine.SelectSingleNode("span[1]/strong[1]");
                        announcement.Hits = (hitsNode != null) ? (uint) Convert.ToDouble(hitsNode.InnerText) : 0;

                        var titleNode = firstLine.SelectSingleNode("a[1]");
                        announcement.Title = (titleNode != null) ? titleNode.InnerText : "";
                    }

                    var secondLine = announcementNode.SelectSingleNode("td[2]/div[2]");
                    if(secondLine != null)
                    {
                        var beginNode = secondLine.SelectSingleNode("span[1]/span[1]");
                        if(beginNode != null)
                        {
                            DateTime beginDate = new DateTime();
                            DateTime.TryParseExact(beginNode.InnerText, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);
                            announcement.Begins = beginDate;
                        }

                        var creatorNode = secondLine.SelectSingleNode("span[2]/a[1]");
                        announcement.Creator.Name = (creatorNode != null) ? creatorNode.InnerText : "";
                        announcement.Creator.Title = secondLine.SelectSingleNode("span[2]/text()[2]").InnerText.Strip();
                        announcement.Creator.Title = announcement.Creator.Title.Remove(0, 1); // remove the brackets
                        announcement.Creator.Title = announcement.Creator.Title.Remove(announcement.Creator.Title.Length - 1, 1); // remove the brackets
                        announcement.Creator.ID = creatorNode.Attributes.Contains("href") ? User.FromURL(creatorNode.Attributes["href"].Value) : 0;   
                    }

                    Announcements.Add(announcement);
                }
            }
        }
    }
}
