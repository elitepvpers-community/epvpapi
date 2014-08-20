using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short description what the section is about 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of all <c>SectionThread</c> objects representing the threads in the section
        /// </summary>
        public List<SectionThread> Threads { get; set; }
        

        public Section(uint id)
            : base(id)
        {
            Threads = new List<SectionThread>();
        }
    }
}
