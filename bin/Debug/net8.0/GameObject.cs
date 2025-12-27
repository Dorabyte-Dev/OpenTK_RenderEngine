using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace GameEngine { 
    public class GameObject
    {
        private string name = "GameObject";
        private bool active = true;

        private Transform _transform;

        public Transform transform => _transform;
        public string Name => name;
        public bool Active => active;

        private List<Component> components = new List<Component>();
        public GameObject() {
            name = "GameObject";
            active = true;
            //_transform = new Transform(this);
            _transform = AddComponent<Transform>();
        }

        public GameObject(string name)
        {
            this.name = name;
            active = true;
            _transform = AddComponent<Transform>();
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

            //Console Write Error "Component Not Found"
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
    }
}
