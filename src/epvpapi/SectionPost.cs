using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class SectionPost : Post, IReportable
    {
        /// <summary>
        /// Icon associated with the post
        /// </summary>
        public short Icon { get; set; }

        /// <summary>
        /// Thread that contains the post
        /// </summary>
        public SectionThread Thread { get; set; }


        public SectionPost(uint id, SectionThread thread)
            : base(id)
        {
            Thread = thread;
        }


        /// <summary>
        /// Reports the <c>SectionPost</c> using the built-in report function
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason of the report </param>
        /// <remarks>
        /// The ID of the <c>SectionPost</c> has to be given in order to report the post
        /// </remarks>
        public void Report(Session session, string reason)
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/report.php?do=sendemail",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("reason", reason),
                            new KeyValuePair<string, string>("postid", ID.ToString()),
                            new KeyValuePair<string, string>("do", "sendemail"),
                            new KeyValuePair<string, string>("url", "showthread.php?p=" + ID.ToString() + "#post" + ID.ToString())                
                        });
        }
    }
}
