using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Object derived from classes that are unique and accessable through a location in the web
    /// </summary>
    public class UniqueObject
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Web URL
        /// </summary>
        public string Link { get; set; }

        /// <param name="id"> Unique identifier </param>
        public UniqueObject(uint id)
        {
            ID = id;
        }

        public UniqueObject()
        { }
    }
}
