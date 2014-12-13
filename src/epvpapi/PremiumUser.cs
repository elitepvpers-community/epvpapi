namespace epvpapi
{
    public class PremiumUser : User
    {
        public uint ShoutboxMessages { get; set; }

        public PremiumUser(int id = 0)
            : base(id)
        { }

        /// <param name="name"> Name of the premium user </param>
        /// <param name="id"> ID of the premium user (profile ID)</param>
        public PremiumUser(string name, int id = 0)
            : base(name, id)
        { }
    }
}
