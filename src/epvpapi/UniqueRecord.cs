using System;

namespace epvpapi
{
    /// <summary>
    /// Represents an unique record provided with an identifier and a record date
    /// </summary>
    public abstract class UniqueRecord : UniqueObject
    {
        /// <summary>
        /// Date and time of the record
        /// </summary>
        public DateTime Date { get; set; }

        public UniqueRecord(int id = 0) :
            base(id)
        {
            Date = new DateTime();
        }
    }
}
