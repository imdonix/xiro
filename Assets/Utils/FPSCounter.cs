using UnityEngine;

namespace Utils
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] string display;

        float duration = 0;
        float frames = 0;
        float worstFps = float.MaxValue;
        float maxFps = float.MinValue;


        void Update()
        {
            float frameduration = Time.unscaledDeltaTime;
            duration += frameduration;
            frames++;
            float FpsCount = 1f / frameduration;
            float averageFps = frames / duration;
            worstFps = FpsCount < worstFps || frames < 300 ? FpsCount : worstFps;
            maxFps = FpsCount > maxFps || frames < 300 ? FpsCount : maxFps;
            display = $"FPS\n{FpsCount:0}\n{averageFps:0}\n{worstFps:0}\n{maxFps:0}";
        }
    }
}