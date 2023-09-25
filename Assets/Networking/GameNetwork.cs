using RiptideNetworking;
using System;
using System.Reflection;
using UnityEngine;
using Utils;

namespace Net
{
    public class GameNetwork : MonoSingleton<GameNetwork>
    {

        private const float TPS = 1F / Settings.TICK_RATE;

        private ushort _me;
        private float _lastTick;

        private GameServer _server;
        private GameClient _client;

        protected override void Init() { }

        #region Unity

        private void Update()
        {
            _lastTick += Time.deltaTime;

            if (!ReferenceEquals(_server, null)) _server.Update();
            if (!ReferenceEquals(_client, null)) _client.Update();

            if (_lastTick > TPS)
            {
                Entity.SyncAll();
                _lastTick = 0;
            }
        }

        private void OnApplicationQuit()
        {
            if (!ReferenceEquals(_server, null)) _server.Quit();
            if (!ReferenceEquals(_client, null)) _client.Quit();
        }

        #endregion

        #region API
        public bool IsServerOnly()
        {
            return !ReferenceEquals(_server, null) && ReferenceEquals(_client, null);
        }

        public bool IsClientOnly()
        {
            return !ReferenceEquals(_client, null) && ReferenceEquals(_server, null);
        }

        public bool IsMine(ushort id)
        {
            if (IsClientOnly())
            {
                return Me() == id;
            }
            else
            {
                return Me() == id || id == (ushort)0;
            }
        }

        public ushort Me()
        {
            return _me;
        }

        public void Connect(INetworkHandler handler, Assembly services, string host, ushort port)
        {
            ClientService.Init(services);

            Message welcome = Message.Create();
            welcome.AddString(Settings.Username);

            _client = new GameClient();

            _client.ConnectionFailed += ((obj, args) => handler.OnConnectionFailed());
            _client.Connected += ((obj, args) => handler.OnConnected());

            _client.Connect($"{host}:{port}", 0, welcome);
        }

        public void Host<T>(INetworkHandler handler, ushort port) where T : ServerService
        {
            _server = GameServer.Create<T>();

            try
            {
                _server.ExtendedStart(port, Settings.MAX_PLAYER);
                handler.OnHost();
            }
            catch (Exception)
            {
                handler.OnHostFailed();
            }
        }

        public void Disconnect()
        {
            OnApplicationQuit();
            _client = null;
            _server = null;
            _me = 0;
        }

        #endregion


        internal void BindMe(ushort id)
        {
            _me = id;
        }

    }
}
