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
    }
}
