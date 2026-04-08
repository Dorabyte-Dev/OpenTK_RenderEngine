using System;

namespace GameEngine
{
    public static class SceneManager
    {
        private static Scene _currentScene;
        public static Scene CurrentScene
        {
            get => _currentScene;
            set
            {
                if (value != null)
                {
                    LoadScene(value);
                }
                else
                {
                    Console.WriteLine("Error: No se puede asignar una escena nula a CurrentScene.");
                }
            }
        }

        public static void LoadScene(Scene newScene)
        {
            Console.WriteLine($"Cargando escena: {newScene.Name}");

            // 1. LIMPIEZA DE MEMORIA (El paso crítico)
            if (_currentScene != null)
            {
                _currentScene.Unload();
                RenderManager.ClearSceneData();
            }

            // 2. ASIGNACIÓN
            _currentScene = newScene;

            // 3. INICIALIZACIÓN
            // Llamamos al Start de todos los objetos de la nueva escena
            _currentScene.Start();
        }
    }
}