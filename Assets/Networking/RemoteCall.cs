using System;

namespace Net
{
    public class RemoteCall : Attribute
    {
        public readonly string tag;

        public RemoteCall(string tag)
        {
            this.tag = tag;
        }
    }
}
