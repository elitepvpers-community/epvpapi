using epvpapi.Connection;

namespace epvpapi
{
    public interface IUpdatable
    {
        void Update<TUser>(AuthenticatedSession<TUser> authenticatedSession) where TUser : User;
    }
}
