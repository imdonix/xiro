using RiptideNetworking;

namespace Net
{
    internal enum STC : ushort
    {
        Leave = 1,
        Join,

        RPC,
        UseService,

        SpawnEntity,
        DestroyEntity,
        SyncEntity
    }
    internal enum CTS : ushort
    {
        RPC = 1,

        SyncEntity
    }

    internal static class Helper
    {
        public static void Copy(Message src, Message dest)
        {
            byte[] buffer = Buffer(src);
            dest.AddBytes(buffer, false, false);
        }

        public static byte[] Buffer(Message src)
        {
            int size = src.UnreadLength;
            byte[] buffer = new byte[size];
            src.GetBytes(size, buffer, 0);
            return buffer;
        }


    }
}
