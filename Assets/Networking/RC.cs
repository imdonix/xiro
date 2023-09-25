
using RiptideNetworking;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Net
{
    public class RC
    {

        private readonly IPeer _provider;
        internal readonly int call;
        internal readonly ushort target;

        public readonly Message message;

        internal RC(int call, ushort target, IPeer provider)
        {
            this.message = Message.Create(MessageSendMode.reliable, provider.GetRCFlag());
            this.target = target;
            this.call = call;

            _provider = provider;

            this.message.AddUShort(target);
            this.message.AddInt(call);
        }

        internal RC(string call, ushort target, IPeer provider) : this(call.GetHashCode(), target, provider) { }

        public void Send()
        {
            _provider.SendRC(this);
        }

        internal static void RegisterRCHandlers(object obj, Dictionary<int, MethodInfo> rpcs)
        {
            foreach (var info in obj.GetType().GetMethods())
            {
                RemoteCall rc = info.GetCustomAttribute<RemoteCall>();
                if (!ReferenceEquals(rc, null))
                {
                    int id = rc.tag.GetHashCode();
                    rpcs.Add(id, info);
                }
            }
        }
    }
}
