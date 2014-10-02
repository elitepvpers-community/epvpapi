using epvpapi.Connection;

namespace epvpapi
{
    public interface IUpdatable
    {
        void Update(GuestSession session);
    }
}
