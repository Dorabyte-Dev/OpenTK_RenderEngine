using System;
using System.Collections.Generic;
namespace GameEngine
{
    public static class Time
    {
        // ----- PUBLIC PROPERTIES -----
        public static float deltaTime { get; private set; }
        public static float unscaledDeltaTime { get; private set; }
        public static float timeScale { get; set; } = 1f;

        public static float time { get; private set; }
        public static int frameCount { get; private set; }

        // ----- PRIVATE -----
        private static double _lastTime = 0;

        // This should be called once per frame by the engine (e.g., inside Game.Update())
        public static void Update()
        {
            double current = GetCurrentTimeSeconds();

            if (_lastTime == 0)
            {
                // First frame
                deltaTime = 0;
                unscaledDeltaTime = 0;
                _lastTime = current;
                return;
            }

            // Compute raw delta
            float dt = (float)(current - _lastTime);

            unscaledDeltaTime = dt;
            deltaTime = dt * timeScale;

            time += deltaTime;
            frameCount++;

            _lastTime = current;
        }

        private static double GetCurrentTimeSeconds()
        {
            return DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMilliseconds / 1000.0;
        }
    }

}
