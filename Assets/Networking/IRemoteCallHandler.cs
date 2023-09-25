using RiptideNetworking;

namespace Net
{
    public interface IRemoteCallHandler
    {
        /// <summary>
        /// Handle a remote call.
        /// </summary>
        /// <returns>true if it can handle the call</returns>
        public bool HandleRC(Message message);

        /// <summary>
        /// Prepare an RC sent to this entity/service
        /// </summary>
        /// <param name="call">Remote call tag</param>
        public RC PrepareRC(string call);

    }
}
