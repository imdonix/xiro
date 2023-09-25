
namespace Net
{
    public interface INetworkHandler
    {

        void OnConnected();

        void OnConnectionFailed();

        void OnHost();

        void OnHostFailed();

    }
}