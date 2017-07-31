using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using epvpapi.Connection;
using HtmlAgilityPack;
using System.Linq;
using System.Net.Http;

namespace epvpapi.TBM
{
    /// <summary>
    /// Represents a <c>Trade</c>
    /// </summary>
    public class Trade : UniqueObject, IUniqueWebObject
    {
        public enum Query
        {
            /// <summary>
            /// If the <c>Trade</c> has been received
            /// </summary>
            Received,

            /// <summary>
            /// If the <c>Trade</c> has been requested
            /// </summary>
            Requested
        }

        /// <summary>
        /// Thread link of the trade
        /// </summary>
        public string Thread { get; private set; }

        /// <summary>
        /// Note that will be visible as public information of the <c>Trade</c> created by the requester
        /// </summary>
        public string Note { get; private set; }

        /// <summary>
        /// <c>User</c> that requested the <c>Trade</c>
        /// </summary>
        public User Requester { get; private set; }

        /// <summary>
        /// <c>User</c> that received the <c>Trade</c>
        /// </summary>
        public User Receiver { get; private set; }

        /// <summary>
        /// Date and time when the <c>Trade</c> was created
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Date and time when the <c>Trade</c> was accepted
        /// </summary>
        public DateTime AcceptedDate { get; private set; }

        public Trade(int id) :
            base(id)
        {
            Thread = "";
            Note = "";
            Requester = new User();
            Receiver = new User();
            CreationDate = new DateTime();
            AcceptedDate = new DateTime();
        }

        public string GetUrl()
        {
            return "https://www.elitepvpers.com/theblackmarket/trade/" + ID;
        }

        public static Trade fromId<TUser>(AuthenticatedSession<TUser> session, int id) where TUser : User
        {
            session.ThrowIfInvalid();
            Trade result = new Trade(id);
            // fetch some trade details
            var res = session.Get(result.GetUrl());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(res);

            var rootFormNode = htmlDocument.GetElementbyId("contentbg");
            if (rootFormNode == null)
            {
                return null;
            }

            var historyNode = rootFormNode.SelectSingleNode("table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]/div[5]/div[3]/table[1]");
            if (historyNode == null)
            {
                return null;
            }

            // get the trade history and the trades note
            // we can assume that classname == gray, because we are searching the open trades only
            var requestorHistoryNodes = historyNode.ChildNodes.GetElementsByTagName("tr").GetElementsByClassName("gray");
            if (requestorHistoryNodes.Count() > 0)
            {
                var creatorNode = requestorHistoryNodes.First();
                if (creatorNode != null)
                {
                    var noteNode = creatorNode.SelectSingleNode("td[3]");
                    if (noteNode != null)
                    {
                        result.Note = noteNode.InnerText;
                    }

                    var creatorDateNode = creatorNode.SelectSingleNode("td[5]");
                    if (creatorDateNode != null)
                    {
                        var creationDate = creatorDateNode.InnerText;
                        result.CreationDate = DateTime.ParseExact(creationDate, "MMM dd, yyyy - HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            //check if trade got accepted
            var acceptorHistoryNodes = historyNode.ChildNodes.GetElementsByTagName("tr").GetElementsByClassName("green");
            if (acceptorHistoryNodes.Count() > 0)
            {
                var acceptorNode = acceptorHistoryNodes.First();
                if (acceptorNode != null)
                {
                    var acceptorDateNode = acceptorNode.SelectSingleNode("td[5]");
                    if (acceptorDateNode != null)
                    {
                        var creationDate = acceptorDateNode.InnerText;
                        result.AcceptedDate = DateTime.ParseExact(creationDate, "MMM dd, yyyy - HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            var detailTableNode = rootFormNode.SelectSingleNode("table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]/div[6]/div[3]/table[2]");
            if (detailTableNode == null)
            {
                return null;
            }

            var requestorNode = detailTableNode.SelectSingleNode("tr[2]").SelectSingleNode("td[1]");
            if (requestorNode != null)
            {
                requestorNode = requestorNode.SelectSingleNode("a[1]");
                if (requestorNode != null)
                {
                    result.Requester = requestorNode.Attributes.Contains("href")
                        ? new User(requestorNode.InnerText,
                            epvpapi.User.FromUrl(requestorNode.Attributes["href"].Value))
                        : new User();
                    result.Requester.Update(session);
                }
            }

            var receiverNode = detailTableNode.SelectSingleNode("tr[2]").SelectSingleNode("td[3]");
            if (receiverNode != null)
            {
                receiverNode = receiverNode.SelectSingleNode("a[1]");
                if (receiverNode != null)
                {
                    result.Receiver = receiverNode.Attributes.Contains("href")
                        ? new User(receiverNode.InnerText,
                            epvpapi.User.FromUrl(receiverNode.Attributes["href"].Value))
                        : new User();
                    result.Receiver.Update(session);
                }
            }

            // get the trade's details
            detailTableNode = rootFormNode.SelectSingleNode("table[1]/tr[1]/td[1]/table[1]/tr[2]/td[1]/div[6]/div[3]/table[1]");
            if (detailTableNode == null)
            {
                return null;
            }

            var tradeThreadNode = detailTableNode.ChildNodes.GetElementsByTagName("tr").First();
            if (tradeThreadNode != null)
            {
                var threadNode = tradeThreadNode.SelectSingleNode("td[2]");
                if (threadNode != null)
                {
                    threadNode = threadNode.SelectSingleNode("a[1]");
                    if (threadNode != null)
                    {
                        result.Thread = threadNode.Attributes.Contains("href")
                            ? threadNode.Attributes["href"].Value
                            : threadNode.InnerText;
                    }
                }
            }

            return result;
        }
    }
}
