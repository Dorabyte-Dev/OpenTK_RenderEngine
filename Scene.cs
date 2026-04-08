using System.Collections.Generic;
using System.Linq;

namespace GameEngine
{
    public class Scene
    {
        public string Name { get; private set; }
        
        // La lista maestra de objetos en esta escena
        private List<GameObject> _gameObjects = new List<GameObject>();

        public Scene(string name)
        {
            Name = name;

            if(SceneManager.CurrentScene == null)
            {
                SetActiveScene();
            }
        }
        public void Unload()
        {
            // Recorremos la lista de objetos y los destruimos uno a uno
            foreach (var go in _gameObjects)
            {
                go.Destroy();
            }

            // Vaciamos la lista de la escena
            _gameObjects.Clear();
        }

        public void SetActiveScene()
        {
            SceneManager.CurrentScene = this;
        }

        // ----- GESTIÓN DE OBJETOS -----

        public void AddGameObject(GameObject go)
        {
            if (!_gameObjects.Contains(go))
            {
                _gameObjects.Add(go);
            }
        }

        public void RemoveGameObject(GameObject go)
        {
            if (_gameObjects.Contains(go))
            {
                _gameObjects.Remove(go);
                go.Destroy();
            }
        }

        public GameObject? FindGameObjectByName(string name)
        {
            var result = _gameObjects.FirstOrDefault(g => g.Name == name);
            if(result == null)
            {
                Console.WriteLine($"GameObject with name '{name}' not found in scene '{Name}'.");
            }
            return result;
        }

        // ----- CICLO DE VIDA (LIFECYCLE) -----

        public void Start()
        {
            foreach (var go in _gameObjects.ToList())
            {
                if (go.Active)
                    go.Start();
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var go in _gameObjects.ToList())
            {
                if (go.Active)
                    go.Update(deltaTime);
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var go in _gameObjects.ToList())
            {
                if (go.Active)
                    go.FixedUpdate(fixedDeltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (var go in _gameObjects.ToList())
            {
                if (go.Active)
                    go.LateUpdate(deltaTime);
            }
        }
    }
}
