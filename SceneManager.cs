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
                    Console.WriteLine("Error: Cannot assign null scene to CurrentScene.");
                }
            }
        }

        public static void LoadScene(Scene newScene)
        {
            Console.WriteLine($"Loading Scene: {newScene.Name}");

            if (_currentScene != null)
            {
                _currentScene.Unload();
                RenderManager.ClearSceneData();
            }

            _currentScene = newScene;

            _currentScene.Start();
        }
    }
}