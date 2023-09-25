
using System;
using Utils;
using System.Collections.Generic;

namespace Net
{
    public abstract class ServerService : Service
    {

        protected static readonly Action<object> _log = Loggers.ServerService;


        private IController _controller;

        protected ServerService() : base()
        {
        }

        internal void InjectController(IController controller)
        {
            _controller = controller;
        }

        public void UseService<T>() where T : ServerService
        {
            _controller.UseService<T>();
        }

        public ushort NetworkInstantiate<T>(User owner) where T : Entity
        {
            return _controller.NetworkInstantiate<T>(owner);
        }

        public void NetworkDestroy(ushort entity)
        {
            _controller.NetworkDestroy(entity);
        }

    }
}