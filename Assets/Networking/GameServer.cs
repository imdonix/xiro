using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utils;

namespace Net
{
    internal class GameServer : Server, IPeer, IController
    {
        private static readonly Action<object> _log = Loggers.Server;

        private readonly Dictionary<ushort, User> _players;

        private Service _service;

        private GameServer()
        {
            _players = new Dictionary<ushort, User>();
            for (int i = 1; i < ushort.MaxValue; i++)
            {
                ids.Enqueue((ushort)i);
            }


            ClientConnected += OnConnected;
            ClientDisconnected += OnDisconnected;
            MessageReceived += OnMessage;

            Entity.LoadAllEntity();
        }

        public void Update()
        {
            _service.Tick();
            this.Tick();
        }

        public void Quit()
        {
            Stop();
            ClientConnected -= OnConnected;
            ClientDisconnected -= OnDisconnected;
            MessageReceived -= OnMessage;
        }

        private void OnConnected(object sender, ServerClientConnectedEventArgs e)
        {
            // Handle connected player
            ushort id = e.Client.Id;
            string name = e.ConnectMessage.GetString();
            User connected = new User(id, name);
            _players.Add(e.Client.Id, connected);
            _service.OnUserJoin(connected);
            _log($"New player connected. [{connected})]");

            // Notify the already connected players
            Message msg = Message.Create(MessageSendMode.reliable, STC.Join, 15, true);
            msg.Add(id);
            msg.Add(name);
            this.SendToAll(msg);

            // Set service for the new player
            string tag = _service.GetType().GetCustomAttribute<ServiceTag>().tag;
            msg = Message.Create(MessageSendMode.reliable, STC.UseService);
            msg.Add(tag);
            this.Send(msg, id);
        }

        private void OnDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Handle disconnected player
            _log($"A player left. [{_players[e.Id]}]");
            User left = _players[e.Id];
            _players.Remove(e.Id);
            _service.OnUserJoin(left);

            // Notify the already connected players
            Message msg = Message.Create(MessageSendMode.reliable, STC.Leave, 15, true);
            msg.Add(e.Id);
            this.SendToAll(msg);
        }

        internal void ExtendedStart(ushort port, ushort max)
        {
            Start(port, max);
            _log($"Started on {port} port [{max}]");
        }

        public bool IsServer()
        {
            return true;
        }

        public void UseService<T>() where T : ServerService
        {
            ServerService service = (ServerService)Activator.CreateInstance(typeof(T));
            service.InjectPeer(this);
            service.InjectController(this);
            _service = service;

            if (this.IsRunning)
            {
                string tag = _service.GetType().GetCustomAttribute<ServiceTag>().tag;
                Message msg = Message.Create(MessageSendMode.reliable, STC.UseService);
                msg.Add(tag);
                this.SendToAll(msg);
            }
        }

        public ushort NetworkInstantiate<T>(User owner) where T : Entity
        {
            ushort id = GenerateNextID();
            int type = Entity.GetEntityType<T>();
            ushort userid = ReferenceEquals(owner, null) ? (ushort)0 : owner.id;

            Message msg = Message.Create(MessageSendMode.reliable, STC.SpawnEntity);
            msg.Add(id);
            msg.Add(userid);
            msg.Add(type);

            this.SendToAll(msg);
            if (GameNetwork.Instance.IsServerOnly())
            {
                Entity.NetworkInstantiate(this, id, userid, type, null);
            }

            return id;
        }

        public void NetworkDestroy(ushort entity)
        {
            Message msg = Message.Create(MessageSendMode.reliable, STC.DestroyEntity);
            msg.Add(entity);

            ids.Enqueue(entity);
            this.SendToAll(msg);
            if (GameNetwork.Instance.IsServerOnly())
            {
                Entity.NetworkDestroy(entity);
            }
        }

        public void Sync(Entity entity)
        {
            //We should directly handle the massage on the server,
            //and only create a copy for the client.

            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)CTS.SyncEntity);
            msg.Add(entity.GetNetworkID());

            foreach (var comp in entity._netcomps)
            {
                comp.OnSend(msg);
            }

            OnMessage(this, new ServerMessageReceivedEventArgs(0, (ushort)CTS.SyncEntity, msg));
        }

        public IEnumerable<User> GetPlayers()
        {
            return new List<User>(_players.Values);
        }

        public void SendRC(RC rc)
        {
            if (rc.target == (ushort)0)
            {
                ushort _ = rc.message.GetUShort();

                try
                {
                    _service.HandleRC(rc.message);
                }
                catch (Exception)
                {
                    RC echo = new RC(rc.call, rc.target, this);
                    Helper.Copy(rc.message, echo.message);
                    this.SendToAll(echo.message);
                }
            }
            else
            {
                this.SendToAll(rc.message);
            }
        }

        public Service GetService()
        {
            return _service;
        }

        public RC PrepareRC(string call, ushort requester)
        {
            return new RC(call, requester, this);
        }

        public ushort GetRCFlag()
        {
            return (ushort)STC.RPC;
        }

        private void OnMessage(object sender, ServerMessageReceivedEventArgs e)
        {

            if (e.MessageId == (ushort)CTS.RPC)
            {
                ushort target = e.Message.GetUShort();

                if (target == (ushort)0)
                {
                    _service.HandleRC(e.Message);
                }
                else
                {
                    if (!Entity.HandleRC(target, e.Message, out ushort owner))
                    {
                        Message forwarded = Message.Create(MessageSendMode.reliable, (ushort)STC.RPC);
                        forwarded.AddUShort(target);
                        Helper.Copy(e.Message, forwarded);

                        this.Send(forwarded, owner);
                    }
                }
            }

            else if (e.MessageId == (ushort)CTS.SyncEntity)
            {

                Message sync = Message.Create(MessageSendMode.unreliable, (ushort)STC.SyncEntity);
                byte[] buffer = Helper.Buffer(e.Message);
                sync.AddBytes(buffer, false, false);

                this.SendToAll(sync, e.FromClientId);

                if (GameNetwork.Instance.IsServerOnly())
                {
                    Message replica = Message.Create(MessageSendMode.unreliable, (ushort)CTS.SyncEntity);
                    replica.AddBytes(buffer, false, false);
                    Entity.Sync(replica);
                }
            }

        }


        private readonly Queue<ushort> ids = new Queue<ushort>();

        private ushort GenerateNextID()
        {
            if (ids.Count == 0)
            {
                throw new Exception("Entity limit is reached. (65534)");
            }

            return ids.Dequeue();
        }

        public static GameServer Create<T>() where T : ServerService
        {
            GameServer server = new GameServer();
            server.UseService<T>();
            return server;
        }
    }
}
