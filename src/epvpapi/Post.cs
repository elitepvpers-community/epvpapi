using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class Post : Message
    {
        /// <summary>
        /// Title of the <c>Post</c>
        /// </summary>
        public string Title { get; set; }

        public Post(uint id, string content = null)
            : base(id, content)
        { }

        public Post(string content, string title = null)
            : base(content)
        {
            Title = title;
        }
    }
}
