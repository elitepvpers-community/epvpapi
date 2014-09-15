using System;
using System.Collections.Generic;
using epvpapi.Connection;
using epvpapi.Evaluation;
using Newtonsoft.Json;

namespace epvpapi.TBM
{
    public class Profile
    {
        public Ratings Ratings { get; protected set; }
        public Mediations Mediations { get; protected set; }
        public SecretWord SecretWord { get; set; }

        public Profile()
        {
            Ratings = new Ratings();
            Mediations = new Mediations();
            SecretWord = new SecretWord();
        }

        /// <summary>
        /// Fetches all Transactions that have been both received and sent
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <returns> List of <c>Transaction</c> objects representing the Transactions </returns>
        public List<Transaction> Transactions<TUser>(ProfileSession<TUser> session) where TUser : User
        {
            var res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=all&secretword=" + SecretWord.Value);
            string responseContent = res.ToString();
            if(String.IsNullOrEmpty(responseContent)) throw new InvalidAuthenticationException("The provided Secret Word was invalid");

            try
            {
                var receivedTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(responseContent);
                foreach (dynamic jsonTransaction in transactions)
                {
                    var transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid))
                    {
                        Note = jsonTransaction.note,
                        Sender = new User(Convert.ToString(jsonTransaction.eg_fromusername),
                                          Convert.ToUInt32(jsonTransaction.eg_from)),
                        Receiver = new User(Convert.ToString(jsonTransaction.eg_tousername),
                                            Convert.ToUInt32(jsonTransaction.eg_to)),
                        EliteGold = Convert.ToInt32(jsonTransaction.amount),
                        Time = ((double) (Convert.ToDouble(jsonTransaction.dateline))).ToDateTime()
                    };

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
        /// <returns> List of <c>Transaction</c> objects representing the received Transactions </returns>
        public List<Transaction> ReceivedTransactions<TUser>(ProfileSession<TUser> session) where TUser : User
        {
            var res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=received&secretword=" + SecretWord.Value);
            string responseContent = res.ToString();
            if (String.IsNullOrEmpty(responseContent)) throw new InvalidAuthenticationException("The provided Secret Word was invalid");

            try
            {
                var receivedTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(responseContent);
                foreach (dynamic jsonTransaction in transactions)
                {
                    var transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid))
                    {
                        Note = jsonTransaction.note,
                        Sender = new User(Convert.ToString(jsonTransaction.eg_fromusername),
                                 Convert.ToUInt32(jsonTransaction.eg_from)),
                        Receiver = session.User,
                        EliteGold = Convert.ToInt32(jsonTransaction.amount),
                        Time = ((double)(Convert.ToDouble(jsonTransaction.dateline))).ToDateTime()
                    };

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
        /// Fetches all Transactions that have been sent
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <returns> List of <c>Transaction</c> objects representing the Transactions sent </returns>
        public List<Transaction> SentTransactions<TUser>(ProfileSession<TUser> session) where TUser : User
        {
            var res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID + "&type=sent&secretword=" + SecretWord.Value);
            string responseContent = res.ToString();
            if (String.IsNullOrEmpty(responseContent)) throw new InvalidAuthenticationException("The provided Secret Word was invalid");

            try
            {
                var sentTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(responseContent);
                foreach (var jsonTransaction in transactions)
                {
                    var transaction = new Transaction(Convert.ToUInt32(jsonTransaction.eg_transactionid))
                    {
                        Note = jsonTransaction.note,
                        Receiver = new User(Convert.ToString(jsonTransaction.eg_tousername),
                                            Convert.ToUInt32(jsonTransaction.eg_to)),
                        Sender = session.User,
                        EliteGold = Convert.ToInt32(jsonTransaction.amount),
                        Time = ((double)(Convert.ToDouble(jsonTransaction.dateline))).ToDateTime()
                    };

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
