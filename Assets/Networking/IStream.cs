
using RiptideNetworking;

namespace Net
{

    /// <summary>
    /// An entity networked streamed component
    /// </summary>
    public interface IStream
    {

        void OnReceive(Message message);

        void OnSend(Message message);
    }
}