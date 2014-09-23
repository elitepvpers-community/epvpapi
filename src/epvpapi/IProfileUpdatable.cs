using epvpapi.Connection;

namespace epvpapi
{
    public interface IProfileUpdatable
    {
        void Update<T>(ProfileSession<T> session) where T : User;
    }
}
