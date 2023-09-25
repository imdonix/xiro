using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using Utils;

namespace Net
{
    public abstract class ClientService : Service
    {
        protected static readonly Action<object> _log = Loggers.ClientService;

        private static readonly Dictionary<string, Type> _services = _services = new Dictionary<string, Type>();

        internal static void Init(Assembly assembly)
        {
            if(_services.Count == 0)
            {
                HashSet<string> tags = new HashSet<string>();
                foreach (Type item in assembly.GetTypes())
                {
                    var tag = item.GetCustomAttribute<ServiceTag>();
                    if (!ReferenceEquals(tag, null))
                    {

                        if (typeof(ClientService).IsAssignableFrom(item))
                        {
                            _services[tag.tag] = item;
                        }

                        if (tags.Contains(tag.tag))
                        {
                            tags.Remove(tag.tag);
                        }
                        else
                        {
                            tags.Add(tag.tag);
                        }
                    }
                }

                Assert.IsFalse(tags.Count > 0, $"Services are not created properly for: {String.Join(", ", tags)}");
            }
        }

        public static ClientService Request(string tag)
        {
            if(_services.Count > 0)
            {
                return (ClientService)Activator.CreateInstance(_services[tag]);
            }

            throw new Exception("Client services are not initialized.");            
        }

    }
}