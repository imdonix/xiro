
using System;
using UnityEngine;

namespace Utils
{
    public static class Settings
    {

        public const int TICK_RATE = 32;
        public const ushort MAX_PLAYER = 4;

        public static int DEMO_START_COUNT = 3;
        public static string DEMO_IP = "192.168.0.141";

        public static string Username = $"{Convert.ToChar(UnityEngine.Random.Range(65, 91))}{Convert.ToChar(UnityEngine.Random.Range(65, 91))}{Convert.ToChar(UnityEngine.Random.Range(65, 91))}{UnityEngine.Random.Range(0, 10)}BP";

        public static KeyCode DebugKey = KeyCode.F1;
    }
}
