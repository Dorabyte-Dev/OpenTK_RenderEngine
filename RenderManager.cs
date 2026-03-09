using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public static class RenderManager
    {

        private static List<Renderer> _renderers = new();
        private static List<Light> _lights = new();

        public static void AddLight(Light light)
        {
            if (!_lights.Contains(light))
                _lights.Add(light);
        }

        public static void RemoveLight(Light light)
        {
            if (_lights.Contains(light))
                _lights.Remove(light);
        }

        public static void AddRenderer(Renderer renderer)
        {
            if (!_renderers.Contains(renderer))
            {
                _renderers.Add(renderer);
            }
        }

        public static void RemoveRenderer(Renderer renderer)
        {
            if (_renderers.Contains(renderer))
            {
                _renderers.Remove(renderer);
            }
        }

        public static void Render()
        {
            foreach (var renderer in _renderers)
            {
                renderer.Render();
            }
        }

        public static Light? GetActiveDirectionalLight() => _lights.FirstOrDefault(l => l.Type == LightType.Directional);
    }
}
