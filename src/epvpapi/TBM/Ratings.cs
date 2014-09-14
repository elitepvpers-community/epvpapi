namespace epvpapi.TBM
{
    public class Ratings : Mediations
    {
        public uint Negative { get; set; }

        public Ratings(uint positive = 0, uint neutral = 0, uint negative = 0): 
            base(positive, neutral)
        {
            Negative = negative;
        }
    }
}
