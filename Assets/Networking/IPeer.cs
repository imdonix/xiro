using System.Collections.Generic;

namespace Net
{
    public interface IPeer
    {

        #region RC

        void SendRC(RC rpc);

        RC PrepareRC(string call, ushort requester);

        ushort GetRCFlag();

        #endregion

        void Sync(Entity entity);

        void Update();

        IEnumerable<User> GetPlayers();

        Service GetService();

    }
}
