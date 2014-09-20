using System;

namespace epvpapi.TBM
{
    /// <summary>
    ///     Represents a <c>Transaction</c> made using The Black Market
    /// </summary>
    public class Transaction : UniqueObject, IUniqueWebObject
    {
        public Transaction(uint id)
            : base(id)
        {
            Sender = new User();
            Receiver = new User();
            Time = new DateTime();
        }

        /// <summary>
        ///     User that sent the <c>EliteGold</c>
        /// </summary>
        public User Sender { get; set; }

        /// <summary>
        ///     User that received the <c>EliteGold</c>
        /// </summary>
        public User Receiver { get; set; }

        /// <summary>
        ///     Amount of elite*gold that was sent
        /// </summary>
        public int EliteGold { get; set; }

        /// <summary>
        ///     Optional note describing the <c>Transaction</c>
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        ///     Date and Time indicating when the <c>Transaction</c> was made
        /// </summary>
        public DateTime Time { get; set; }


        public string GetUrl()
        {
            return "http://www.elitepvpers.com/theblackmarket/transaction/" + Id;
        }
    }
}