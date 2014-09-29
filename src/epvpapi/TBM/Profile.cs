using System;
using System.Collections.Generic;
using epvpapi.Connection;
using epvpapi.Evaluation;
using Newtonsoft.Json;

namespace epvpapi.TBM
{
    public class Profile : UniqueObject, IUniqueWebObject
    {
        public Ratings Ratings { get; protected set; }
        public Mediations Mediations { get; protected set; }

        /// <summary>
        /// Represents the Secret word required for the official TBM API in order to query Transactions
        /// </summary>
        public string SecretWord { get; set; }

        public Profile(uint id = 0):
            base(id)
        {
            Ratings = new Ratings();
            Mediations = new Mediations();
        }

        /// <summary>
        /// Fetches all recorded transactions of the profile using the TBM API
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="query"> 
        /// Indicates whether to retrieve all transactions, only the sent transactions or only the received transactions.
        /// Use the <c>Transaction.Query.Received</c> or <c>Transaction.Query.Sent</c> constant. You can also concatenate both constants to get all transactions
        /// </param>
        /// <returns> List of <c>Transaction</c> objects representing the Transactions </returns>
        public List<Transaction> GetTransactions<TUser>(Session<TUser> session, 
            Transaction.Query query = Transaction.Query.Sent | Transaction.Query.Received)  where TUser : User
        {
            var typeParameter = "all";
            if (query.HasFlag(Transaction.Query.Received) && !query.HasFlag(Transaction.Query.Sent))
                typeParameter = "received";
            else if (query.HasFlag(Transaction.Query.Sent) && !query.HasFlag(Transaction.Query.Received))
                typeParameter = "sent";

            var res = session.Get("http://www.elitepvpers.com/theblackmarket/api/transactions.php?u=" + session.User.ID +
                                    "&type=" + typeParameter + "&secretword=" + SecretWord);

            var responseContent = res.ToString();
            if(String.IsNullOrEmpty(responseContent))
                throw new InvalidAuthenticationException("The provided Secret Word was invalid");

            try
            {
                var receivedTransactions = new List<Transaction>();
                dynamic transactions = JsonConvert.DeserializeObject(responseContent);
                foreach (var jsonTransaction in transactions)
                {
                    var transaction = new Transaction(jsonTransaction.eg_transactionid.To<uint>())
                    {
                        Note = jsonTransaction.note,
                        EliteGold = jsonTransaction.amount.To<int>(),
                        Time = jsonTransaction.dateline.To<double>().ToDateTime()
                    };

                    if (query.HasFlag(Transaction.Query.Received))
                    {
                        transaction.Receiver = session.User;
                        transaction.Sender = new User(jsonTransaction.eg_fromusername, jsonTransaction.eg_from.To<uint>());
                    }

                    if (query.HasFlag(Transaction.Query.Sent))
                    {
                        transaction.Sender = session.User;
                        transaction.Receiver = new User(jsonTransaction.eg_tousername, jsonTransaction.eg_to.To<uint>());
                    }

                    receivedTransactions.Add(transaction);
                }

                return receivedTransactions;
            }
            catch (JsonException exception)
            {
                throw new ParsingFailedException("Could not parse received Transactions", exception);
            }
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/theblackmarket/profile/" + ID;
        }
    }
}
