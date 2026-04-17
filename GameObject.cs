using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace GameEngine { 
    public class GameObject
    {

        [JsonProperty]private string name = "GameObject";
        [JsonProperty]private bool active = true;

        private Transform _transform;

        [JsonIgnore] public Transform transform => _transform;
        [JsonIgnore]public string Name => name;
        [JsonIgnore]public bool Active => active;

        [JsonProperty] private List<Component> components = new List<Component>();
        public GameObject() {
            name = "GameObject";
            active = true;
            _transform = AddComponent<Transform>();

            if (SceneManager.CurrentScene != null)
            {
                SceneManager.CurrentScene.AddGameObject(this);
            }
        }

        public GameObject(string name)
        {
            this.name = name;
            active = true;
            _transform = AddComponent<Transform>();

            if (SceneManager.CurrentScene != null)
            {
                SceneManager.CurrentScene.AddGameObject(this);
            }
        }

        public T AddComponent<T>() where T : Component
        {
            T component = (T)Activator.CreateInstance(typeof(T), this);
            components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var c in components)
                if (c is T t) return t;

            Console.WriteLine("Error: Component of type " + typeof(T) + " not found in GameObject '" + name + "'.");
            return null;
        }

        public string DebugComponents()
        {
            string str = string.Empty;
            foreach(var c in components)
            {
                str += c.GetType();
            }
            return str;
        }

        public void Start()
        {
            foreach (var c in components)
                c.Start();
        }

        public void Update(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.Update(deltaTime);
        }

        public void FixedUpdate(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.FixedUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.LateUpdate(deltaTime);
        }

        public void Destroy()
        {
            foreach (var c in components)
            {
                c.Destroy();
            }
            components.Clear();

        }
        #region Json
        [JsonConstructor]
        private GameObject(string name, bool active)
        {
            this.name = name;
            this.active = active;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            _transform = GetComponent<Transform>();

            foreach (var c in components)
            {
                c.owner = this;
            }
        }
        #endregion
    }
}
