using epvpapi.Connection;

namespace epvpapi
{
    public interface ISpecializedUpdatable
    {
        void Update<T>(ProfileSession<T> session) where T : User;
    }
}