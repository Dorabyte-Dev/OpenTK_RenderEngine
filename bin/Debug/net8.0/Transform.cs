using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace GameEngine
{
    public class Transform : Component
    {
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero; 
        public Vector3 scale = Vector3.One;
        public Transform(GameObject owner) : base(owner){}

        public Transform(GameObject owner, Vector3 position) : base(owner)
        {
            this.position = position;
        }

        public Transform(GameObject owner, Vector3 position, Vector3 rotation) : base(owner)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Transform(GameObject owner, Vector3 position, Vector3 rotation, Vector3 scale) : base(owner)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}
