using System;
using System.Collections.Generic;

namespace epvpapi
{
    /// <summary>
    /// Every user got an own blog that is accessible for everyone and writeable for the owner
    /// </summary>
    public class Blog : UniqueObject, IUniqueWebObject
    {
        public List<BlogEntry> Entries { get; set; }
        public DateTime LastEntry { get; set; }
        public User Owner { get; set; }

        public Blog(User owner) :
            base(owner.ID)
        {
            Entries = new List<BlogEntry>();
            Owner = owner;
        }

        /// <summary>
        /// Gets the url of the blog
        /// </summary>
        /// <returns> The url of the blog </returns>
        public string GetUrl()
        {
            return "https://www.elitepvpers.com/forum/blogs/" + Owner.ID + "-" + Owner.Name.UrlEscape() + ".html";
        }
    }
}
