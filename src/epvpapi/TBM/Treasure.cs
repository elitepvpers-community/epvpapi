using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using epvpapi.Connection;

namespace epvpapi.TBM
{
    /// <summary>
    /// Represents a treasure containing user-defined content that can be purchased with elite*gold
    /// </summary>
    public class Treasure : UniqueObject, IUniqueWebObject, IDefaultUpdatable
    {
        /// <summary>
        /// Title of the treasure, visible for everyone
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// User-defined content that will be visible once the <c>Treasure</c> has been bought
        /// <remarks>
        /// The content must be at least 4 characters long to be accepted by the system
        /// </remarks>
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// <c>User</c> that sells/sold the <c>Treasure</c>
        /// </summary>
        public User Seller { get; set; }

        /// <summary>
        /// <c>User</c> that bought the <c>Treasure</c>
        /// </summary>
        public User Buyer { get; set; }

        /// <summary>
        /// Treasure purchase cost given in elite*gold
        /// </summary>
        public uint Cost { get; set; }

        /// <summary>
        /// Date and time when the <c>Treasure</c> was created
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Date and time when the <c>Treasure</c> was bought
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        public Treasure(string title, string content, uint cost):
            this(0, title, content, cost)
        { }


        public Treasure(uint id, string title = null, string content = null, uint cost = 0):
            base(id)
        {
            Title = title;
            Content = content;
            Cost = cost;
            Seller = new User();
            Buyer = new User();
            CreationDate = new DateTime();
            PurchaseDate = new DateTime();
        }

        /// <summary>
        /// Creates the <c>Treasure</c> and makes it public 
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Create<TUser>(ProfileSession<TUser> session) where TUser : User
        {
            if (Content.Length < 4) throw new ArgumentException("The content is too short (4 characters minimum)");

            session.Post("http://www.elitepvpers.com/theblackmarket/treasures/",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("title", Title),
                            new KeyValuePair<string, string>("content", Content),
                            new KeyValuePair<string, string>("cost", Cost.ToString()),
                            new KeyValuePair<string, string>("createtreasure", "Submit")
                        });

            CreationDate = DateTime.Now;
            Seller = session.User;
        }

        /// <summary>
        /// Updates the <c>Treasure</c> by requesting the treasure page
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(Session session)
        {
            var res = session.Get(GetUrl());
            throw new NotImplementedException();
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/theblackmarket/treasures/" + ID;
        }
    }
}
