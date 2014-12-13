using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class UnicastMessage : Message
    {
        /// <summary>
        /// Recipient of the message
        /// </summary>
        public User Receiver { get; set; }

        public UnicastMessage(int id = 0)
            : this(id, new User(), new Content())
        { }

        public UnicastMessage(Content content)
            : this(0, new User(), content)
        { }

        public UnicastMessage(User receiver, Content content)
            : this(0, receiver, content)
        { }

        public UnicastMessage(int id, User receiver, Content content)
            : base(id)
        {
            Content = content;       
            Sender = new User();
            Receiver = receiver;
        }
    }
}
