using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Net
{

    /// <summary>
    /// A service describe a server or client system
    /// </summary>
    public abstract class Service : IRemoteCallHandler
    {
        private readonly Dictionary<int, MethodInfo> _rcs;

        protected IPeer _provider;
        protected bool _inited;

        protected Service()
        {
            _rcs = new Dictionary<int, MethodInfo>();
            RC.RegisterRCHandlers(this, _rcs);
        }

        public abstract void Init();

        public abstract void Update();

        internal void Tick()
        {
            if (_inited)
            {
                Update();
            }
            else
            {
                Init();
                _inited = true;
            }
        }

        public abstract void OnUserLeave(User user);

        public abstract void OnUserJoin(User user);

        internal void InjectPeer(IPeer peer)
        {
            _provider = peer;
        }

        public IEnumerable<User> GetPlayers()
        {
            return _provider.GetPlayers();
        }

        public RC PrepareRC(string call)
        {
            return _provider.PrepareRC(call, 0);
        }

        public bool HandleRC(Message message)
        {
            int rc = message.GetInt();

            if (_rcs.ContainsKey(rc))
            {
                _rcs[rc].Invoke(this, new object[] { message });
                return true;
            }

            throw new Exception($"No RC handler created for {rc} id");
        }

    }
}
