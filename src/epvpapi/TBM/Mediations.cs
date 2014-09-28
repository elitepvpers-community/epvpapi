namespace epvpapi.TBM
{
    /// <summary>
    /// Rating system, represents the rating of middleman services.
    /// </summary>
    public class Mediations
    {
        /// <summary>
        /// Amount of positive ratings
        /// </summary>
        public uint Positive { get; set; }

        /// <summary>
        /// Amount of neutral ratings
        /// </summary>
        public uint Neutral { get; set; }

        public Mediations(uint positive = 0, uint neutral = 0)
        {
            Positive = positive;
            Neutral = neutral;
        }
    }
}
