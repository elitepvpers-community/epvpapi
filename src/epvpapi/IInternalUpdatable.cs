using epvpapi.Connection;

namespace epvpapi
{
    public interface IInternalUpdatable
    {
        void Update<TUser>(AuthenticatedSession<TUser> session) where TUser : User;
    }
}
