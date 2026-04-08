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

        [JsonIgnore]public Vector3 Forward
        {
            get
            {
                // Asumimos que tu variable 'rotation' está en GRADOS (ej: rotation.Y = 90f).
                // Si ya estuviera en radianes, puedes quitar el 'MathHelper.DegreesToRadians'.
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );

                // Creamos un Quaternion (la forma más segura de rotar en 3D sin bugs extraños)
                Quaternion q = Quaternion.FromEulerAngles(radRot);

                // Multiplicamos el vector -Z por nuestra rotación
                Vector3 forward = Vector3.Transform(-Vector3.UnitZ, q);
                return forward.Normalized();
            }
        }

        [JsonIgnore]
        public Vector3 Right
        {
            get
            {
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );
                Quaternion q = Quaternion.FromEulerAngles(radRot);

                // Multiplicamos el vector +X por nuestra rotación
                Vector3 right = Vector3.Transform(Vector3.UnitX, q);
                return right.Normalized();
            }
        }

        [JsonIgnore]
        public Vector3 Up
        {
            get
            {
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );
                Quaternion q = Quaternion.FromEulerAngles(radRot);

                // Multiplicamos el vector +Y por nuestra rotación
                Vector3 up = Vector3.Transform(Vector3.UnitY, q);
                return up.Normalized();
            }
        }

        [JsonIgnore]
        public Matrix4 ModelMatrix
        {
            get
            {
                // 1. Convertir la rotación a radianes para la matriz
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );

                // 2. El orden correcto en OpenGL suele ser: Escala * Rotación * Traslación
                Matrix4 scaleMat = Matrix4.CreateScale(scale);
                Matrix4 rotMat = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(radRot));
                Matrix4 transMat = Matrix4.CreateTranslation(position);

                return scaleMat * rotMat * transMat;
            }
        }
    }
}
