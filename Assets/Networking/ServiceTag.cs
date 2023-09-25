using System;

namespace Net
{
    public class ServiceTag : Attribute
    {
        public readonly string tag;

        public ServiceTag(string tag)
        {
            this.tag = tag;
        }
    }
}
