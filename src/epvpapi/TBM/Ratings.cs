namespace epvpapi.TBM
{
    /// <summary>
    /// Based on the mediation rating system, the general rating system is used for 
    /// rating trades, transactions and treasures. 
    /// </summary>
    public class Ratings : Mediations
    {
        /// <summary>
        /// Amount of negative ratings
        /// </summary>
        public uint Negative { get; set; }

        public Ratings(uint positive = 0, uint neutral = 0, uint negative = 0): 
            base(positive, neutral)
        {
            Negative = negative;
        }
    }
}
