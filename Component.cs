using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

using Newtonsoft.Json;

namespace GameEngine
{
    public abstract class Component
    {
        [JsonIgnore]public GameObject owner { get; internal set; }

        private bool _enabled = true;
        
        [JsonProperty]public bool enabled
        {
            get {
                return _enabled;
            }
            set
            {
                _enabled = value;
                if (_enabled)
                {
                    OnEnable();
                }
                else
                {
                    OnDisable();
                }
            }
        }
        public Component(GameObject owner) 
        {
            this.owner = owner;
        }

        protected Component() //For JSON Constructor
        {
        }


        public virtual void Start() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime) { }
        public virtual void LateUpdate(float deltaTime) { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void Destroy() { }
    }
}
