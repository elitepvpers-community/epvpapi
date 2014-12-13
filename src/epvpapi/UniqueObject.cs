namespace epvpapi
{
    /// <summary>
    /// Object derived from classes that are unique and accessable through a location in the web
    /// </summary>
    public abstract class UniqueObject
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int ID { get; set; }

        /// <param name="id"> Unique identifier </param>
        public UniqueObject(int id = 0)
        {
            ID = id;
        }
    }
}
