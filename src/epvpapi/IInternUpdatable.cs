using epvpapi.Connection;

namespace epvpapi
{
    public interface IInternUpdatable
    {
        void Update<TUser>(AuthenticatedSession<TUser> session) where TUser : User;
    }
}
