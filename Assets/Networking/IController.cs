namespace Net
{
    public interface IController : IPeer
    {
        public ushort NetworkInstantiate<T>(User owner) where T : Entity;

        public void NetworkDestroy(ushort entity);

        public void UseService<T>() where T : ServerService;
    }
}