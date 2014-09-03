using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public interface ISpecializedUpdatable
    {
        void Update<T>(UserSession<T> session) where T : User;
    }
}
