using RiptideNetworking;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Net
{
    internal class GameClient : Client, IPeer
    {
        private static readonly Action<object> _log = Loggers.Client;

        private readonly Dictionary<ushort, User> _players;

        private Service _service;

        public GameClient()
        {
            _players = new Dictionary<ushort, User>();

            Connected += OnConnected;
            ConnectionFailed += OnConnectionFailed;
            Disconnected += OnDisconnected;

            MessageReceived += OnMessageReceived;

            Entity.LoadAllEntity();
        }

        public void Update()
        {
            if (!ReferenceEquals(_service, null)) _service.Tick();
            this.Tick();
        }

        public void Quit()
        {
            Disconnect();

            Connected -= OnConnected;
            ConnectionFailed -= OnConnectionFailed;
            Disconnected -= OnDisconnected;

            MessageReceived -= OnMessageReceived;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            _log($"Connected [{this.Id}]");
            GameNetwork.Instance.BindMe(this.Id);
        }

        private void OnConnectionFailed(object sender, EventArgs e)
        {
            _log("Connecting to the server failed.");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            _log("Disconnected from the server");
        }

        public bool IsServer()
        {
            return false;
        }

        public Service GetService()
        {
            return _service;
        }

        public IEnumerable<User> GetPlayers()
        {
            return new List<User>(_players.Values);
        }

        public void Sync(Entity entity)
        {
            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)CTS.SyncEntity);
            msg.AddUShort(entity.GetNetworkID());

            foreach (var comp in entity._netcomps)
            {
                comp.OnSend(msg);
            }

            this.Send(msg);
        }

        #region RC

        public void SendRC(RC rc)
        {
            if (rc.target == (ushort)0)
            {

                //Read headers 3 byte
                rc.message.GetByte();
                rc.message.GetUShort();

                ushort _ = rc.message.GetUShort();

                try
                {
                    _service.HandleRC(rc.message);
                }
                catch (Exception)
                {
                    RC echo = new RC(rc.call, rc.target, this);
                    Helper.Copy(rc.message, echo.message);
                    this.Send(echo.message);
                }
            }
            else
            { 
                this.Send(rc.message);
            }
        }

        public RC PrepareRC(string call, ushort requester)
        {
            return new RC(call, requester, this);
        }

        public ushort GetRCFlag()
        {
            return (ushort)CTS.RPC;
        }

        #endregion

        private void OnMessageReceived(object sender, ClientMessageReceivedEventArgs e)
        {

            if (e.MessageId == (ushort)STC.Join)
            {
                Message msg = e.Message;

                ushort id = msg.GetUShort();
                string name = msg.GetString();
                User user = new User(id, name);

                _players.Add(id, user);
                _service?.OnUserJoin(user);
            }

            else if (e.MessageId == (ushort)STC.Leave)
            {
                Message msg = e.Message;

                ushort id = msg.GetUShort();
                User user = _players[id];

                _players.Remove(id);
                _service?.OnUserLeave(user);
            }

            else if (e.MessageId == (ushort)STC.UseService)
            {
                string tag = e.Message.GetString();
                _log($"Using service: {tag}");

                Service service = ClientService.Request(tag);
                service.InjectPeer(this);
                _service = service;
            }

            else if (e.MessageId == (ushort)STC.RPC)
            {
                ushort target = e.Message.GetUShort();

                if (target == (ushort)0)
                {
                    _service.HandleRC(e.Message);
                }
                else
                {
                    Entity.HandleRC(target, e.Message, out ushort _);
                }

            }

            else if (e.MessageId == (ushort)STC.SpawnEntity)
            {
                ushort id = e.Message.GetUShort();
                ushort userid = e.Message.GetUShort();
                int type = e.Message.GetInt();

                Entity.NetworkInstantiate(this, id, userid, type, e.Message);
            }

            else if (e.MessageId == (ushort)STC.DestroyEntity)
            {
                ushort id = e.Message.GetUShort();
                Entity.NetworkDestroy(id);
            }

            else if (e.MessageId == (ushort)STC.SyncEntity)
            {
                Entity.Sync(e.Message);
            }
        }


    }
}
