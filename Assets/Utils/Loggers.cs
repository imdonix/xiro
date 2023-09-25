using System;
using UnityEngine;

namespace Utils
{
    public static class Loggers
    {
        public static Action<object> Server = (obj) => Debug.Log($"<color=#ADD8E6>[Server]</color> {obj}");
        public static Action<object> Client = (obj) => Debug.Log($"<color=green>[Client]</color> {obj}");
        public static Action<object> ServerService = (obj) => Debug.Log($"<color=#ADD8E6>[Service]</color> {obj}");
        public static Action<object> ClientService = (obj) => Debug.Log($"<color=green>[Service]</color> {obj}");


    }
}
