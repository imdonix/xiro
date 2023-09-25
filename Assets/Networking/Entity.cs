using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Net
{
    /// <summary>
    /// A networked object.
    /// </summary>
    public abstract class Entity : MonoBehaviour, IStream, IRemoteCallHandler
    {
        private static readonly Dictionary<ushort, Entity> _entities = new Dictionary<ushort, Entity>();
        private static readonly Dictionary<int, Entity> _types = new Dictionary<int, Entity>();

        private readonly Dictionary<int, MethodInfo> _rcs = new Dictionary<int, MethodInfo>();
        internal List<IStream> _netcomps;
        private IPeer _peer;
        private ushort _id;
        private ushort _owner;

        public Service Service => _peer.GetService();

        #region UNITY

        private void Awake()
        {
            RC.RegisterRCHandlers(this, _rcs);
            NetworkComponent[] comps = GetComponentsInChildren<NetworkComponent>(true);
            _netcomps = new List<IStream>(comps);
            _netcomps.Add(this);
        }

        private void OnDestroy()
        {
            _entities.Remove(_id);
        }

        #endregion UNITY

        protected abstract void NetworkAwake(Message args);

        public virtual void OnReceive(Message message) { }

        public virtual void OnSend(Message message) { }

        public int GetEntityType()
        {
            return $"E{GetType().FullName}".GetHashCode();
        }

        public static int GetEntityType<T>()
        {
            return $"E{typeof(T).FullName}".GetHashCode();
        }

        public ushort GetUser()
        {
            return _owner;
        }

        public ushort GetNetworkID()
        {
            return _id;
        }

        public bool IsMine()
        {
            return GameNetwork.Instance.IsMine(_owner);
        }

        public RC PrepareRC(string call)
        {
            return _peer.PrepareRC(call, _id);
        }

        public bool HandleRC(Message message)
        {
            int rc = message.GetInt();
            if (_rcs.ContainsKey(rc))
            {
                _rcs[rc].Invoke(this, new object[] { message });
                return true;
            }
            else
            {
                throw new Exception($"No RC handler ({_id}) created for {rc} id. ");
            }
        }

        #region Life cycle

        internal static Entity NetworkInstantiate(IPeer peer, ushort id, ushort owner, int type, Message args)
        {

            if (!_types.ContainsKey(type))
                throw new Exception($"Entity can't be network network instantiated, no entity found with id: {type}");

            if (_entities.ContainsKey(id))
            {
                Debug.LogWarning("This shoud be destoryed fisrt");
                Destroy(_entities[id].gameObject);
                _entities.Remove(id);
            }
                

            Entity entity = GameObject.Instantiate(_types[type]);
            entity._peer = peer;
            entity._id = id;
            entity._owner = owner;
            entity.name = $"[E] {entity.GetType().Name} ({entity._id}) <{entity._owner}>";
            Entity._entities.Add(entity._id, entity);

            entity.NetworkAwake(args);

            return entity;
        }

        internal static void NetworkDestroy(ushort id)
        {
            if (_entities.ContainsKey(id))
            {
                GameObject.Destroy(_entities[id].gameObject);
            }
        }

        public static bool HasEntityByID(ushort id)
        {
            return _entities.ContainsKey(id);
        }

        /// <summary>
        /// Receive an entity network componet datapack
        /// </summary>
        /// <param name="message"></param>
        internal static void Sync(Message message)
        {
            ushort id = message.GetUShort();

            if(_entities.ContainsKey(id))
            {
                Entity entity = _entities[id];

                foreach (var comps in entity._netcomps)
                {
                    comps.OnReceive(message);
                }
            }
            else
            {
                // Entity has been destoryed in the meanwhile.
            }
        }

        /// <summary>
        /// Send all entity network component datapack to the server
        /// </summary>
        internal static void SyncAll()
        {
            foreach (Entity entity in _entities.Values)
            {
                if (entity.IsMine())
                {
                    entity._peer.Sync(entity);
                }
            }
        }

        /// <summary>
        /// Handle an entity RC call.
        /// </summary>
        /// <param name="target">id of the targeted entity</param>
        /// <param name="message">the message</param>
        /// <returns>Returns true if rc can be executed on localy</returns>
        /// <exception cref="Exception"></exception>
        internal static bool HandleRC(ushort target, Message message, out ushort owner)
        {
            if (_entities.ContainsKey(target))
            {
                Entity entity = _entities[target];
                owner = entity._owner;

                if (entity.IsMine())
                {
                    _entities[target].HandleRC(message);
                    return true;
                }

                return false;
            }
            else
                throw new Exception($"RC can't be executed because the entity with id {target} not exists");
        }

        //This should be removed I dont like to access the entities this way
        public static Entity GetEntityByID(ushort id)
        {
            return _entities[id];
        }

        #endregion


        internal static void LoadAllEntity()
        {
            if (Entity._types.Count == 0)
            {
                foreach (Entity entity in Resources.LoadAll<Entity>("Entities"))
                {
                    _types.Add(entity.GetEntityType(), entity);
                }

                Assert.IsTrue(Entity._types.Count > 0, "No entity can be loaded from Resources/Entities");
            }
        }

    }
}
