using epvpapi.Connection;

namespace epvpapi
{
    public interface IUpdatable
    {
        void Update<TUser>(Session<TUser> session) where TUser : User;
    }
}
