using RiptideNetworking;
using UnityEngine;

namespace Net
{
    public abstract class NetworkComponent : MonoBehaviour, IStream
    {

        public abstract void OnReceive(Message message);

        public abstract void OnSend(Message message);
        //:)
    }
}