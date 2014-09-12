namespace epvpapi
{
    /// <summary>
    /// Object derived from classes that are unique and accessable through a location in the web
    /// </summary>
    public abstract class UniqueWebObject
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Web URL to the unique web object
        /// </summary>
        public abstract string URL { get; }

        /// <param name="id"> Unique identifier </param>
        public UniqueWebObject(uint id = 0)
        {
            ID = id;
        }
    }
}
