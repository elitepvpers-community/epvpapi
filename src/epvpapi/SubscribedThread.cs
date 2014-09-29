using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{

    /// <summary>
    /// Represents a subscribed thread
    /// </summary>
    public class SubscribedThread : Thread
    {

        /// <summary>
        /// The id of the subscribed thread
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The title of the subscribed thread
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The <c>SectionThread</c> of the subscribed thread
        /// </summary>
        public SectionThread SectionThread;

        public SubscribedThread(uint id, Section section) : 
            base(id)
        {
            Id = id;
            SectionThread = new SectionThread(id, section);
        }

    }

}
