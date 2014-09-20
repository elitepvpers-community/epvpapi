namespace epvpapi
{
    public class PremiumUser : User
    {
        public PremiumUser(uint id = 0)
            : base(id)
        {
        }

        /// <param name="name"> Name of the premium user </param>
        /// <param name="id"> ID of the premium user (profile ID)</param>
        public PremiumUser(string name, uint id = 0)
            : base(name, id)
        {
        }

        public uint ShoutboxMessages { get; set; }
    }
}