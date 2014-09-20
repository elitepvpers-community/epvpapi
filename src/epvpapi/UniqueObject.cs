namespace epvpapi
{
    /// <summary>
    ///     Object derived from classes that are unique and accessable through a location in the web
    /// </summary>
    public abstract class UniqueObject
    {
        /// <param name="id"> Unique identifier </param>
        protected UniqueObject(uint id = 0)
        {
            Id = id;
        }

        /// <summary>
        ///     Unique identifier
        /// </summary>
        public uint Id { get; set; }
    }
}