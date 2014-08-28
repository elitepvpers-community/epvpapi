using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace epvpapi.TBM
{
    /// <summary>
    /// Represents a <c>Transaction</c> made using The Black Market
    /// </summary>
    public class Transaction : UniqueObject
    {
        /// <summary>
        /// User that sent the <c>EliteGold</c>
        /// </summary>
        User Sender { get; set; }
        
        /// <summary>
        /// User that received the <c>EliteGold</c>
        /// </summary>
        User Receiver { get; set; }

        /// <summary>
        /// Amount of elite*gold that was sent
        /// </summary>
        int EliteGold { get; set; }

        /// <summary>
        /// Optional note describing the <c>Transaction</c>
        /// </summary>
        string Note { get; set; }

        /// <summary>
        /// Date and Time indicating when the <c>Transaction</c> was made
        /// </summary>
        public DateTime Time { get; set; }

        public Transaction(uint id)
            : base(id)
        {
            Sender = new User();
            Receiver = new User();
            Time = new DateTime();
        }


         /// <summary>
        /// Fetches all Transactions that have been both received and sent
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="secretword"> Secret word used as authentication </param>
        /// <returns> List of <c>Transaction</c> objects representing the Transactions </returns>
        public static List<Transaction> All(UserSession<User> session, string secretword)
        {
            Response res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=all&secretword=" + secretword);

            try
            {
                List<Transaction> receivedTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(res.ToString());
                foreach (dynamic jsonTransaction in transactions)
                {
                    Transaction transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid));
                    transaction.Note = jsonTransaction.note;
                    transaction.Sender = new User(Convert.ToString(jsonTransaction.eg_fromusername), Convert.ToUInt32(jsonTransaction.eg_from));
                    transaction.Receiver = new User(Convert.ToString(jsonTransaction.eg_tousername), Convert.ToUInt32(jsonTransaction.eg_to));
                    transaction.EliteGold = Convert.ToInt32(jsonTransaction.amount);
                    transaction.Time = ((double)(Convert.ToDouble(jsonTransaction.dateline))).ToDateTime();

                    receivedTransactions.Add(transaction);
                }

                return receivedTransactions;
            }
            catch (JsonException exception)
            {
                throw new ParsingFailedException("Could not parse received Transactions", exception);
            }
        }


        /// <summary>
        /// Fetches all Transactions that have been received
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="secretword"> Secret word used as authentication </param>
        /// <returns> List of <c>Transaction</c> objects representing the received Transactions </returns>
        public static List<Transaction> Received(UserSession<User> session, string secretword)
        {
            Response res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=received&secretword=" + secretword);

            try
            {
                List<Transaction> receivedTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(res.ToString());
                foreach (dynamic jsonTransaction in transactions)
                {
                    Transaction transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid));
                    transaction.Note = jsonTransaction.note;
                    transaction.Sender = new User(Convert.ToString(jsonTransaction.eg_fromusername), Convert.ToUInt32(jsonTransaction.eg_from));
                    transaction.Receiver = session.User;
                    transaction.EliteGold = Convert.ToInt32(jsonTransaction.amount);
                    transaction.Time = ((double)(Convert.ToDouble(jsonTransaction.dateline))).ToDateTime();

                    receivedTransactions.Add(transaction);
                }

                return receivedTransactions;
            }
            catch(JsonException exception)
            {
                throw new ParsingFailedException("Could not parse received Transactions", exception);
            }
        }


        /// <summary>
        /// Fetches all Transactions that have been sent
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="secretword"> Secret word used as authentication </param>
        /// <returns> List of <c>Transaction</c> objects representing the Transactions sent </returns>
        public static List<Transaction> Sent(UserSession<User> session, string secretword)
        {
            Response res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=sent&secretword=" + secretword);

            try
            {
                List<Transaction> sentTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(res.ToString());
                foreach (dynamic jsonTransaction in transactions)
                {
                    Transaction transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid));
                    transaction.Note = jsonTransaction.note;
                    transaction.Receiver = new User(Convert.ToString(jsonTransaction.eg_tousername), Convert.ToUInt32(jsonTransaction.eg_to));
                    transaction.Sender = session.User;
                    transaction.EliteGold = Convert.ToInt32(jsonTransaction.amount);
                    transaction.Time = ((double)(Convert.ToDouble(jsonTransaction.dateline))).ToDateTime();

                    sentTransactions.Add(transaction);
                }

                return sentTransactions;
            }
            catch (JsonException exception)
            {
                throw new ParsingFailedException("Could not parse received Transactions", exception);
            }
        }
    }
}
