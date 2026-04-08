using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System;
using Newtonsoft.Json;


namespace GameEngine
{
    public class Camera : Component
    {
        private float _fov = MathHelper.PiOver2; //Radians

        //Multiple Instances
        private static Camera main;
        public static Camera Main
        {
            get => main;
            set => main = value;
        }
        public float AspectRatio { private get; set; }
        public Camera(GameObject owner) : base(owner)
        {
            if(Camera.main == null)
            {
                this.SetAsMain();
            }
        }

        [JsonIgnore]public Vector3 Front
        {
            get
            {
                float pitch = MathHelper.DegreesToRadians(owner.transform.rotation.X);
                float yaw = MathHelper.DegreesToRadians(owner.transform.rotation.Y);

                Vector3 front;
                front.X = MathF.Cos(pitch) * MathF.Sin(yaw);
                front.Y = MathF.Sin(pitch);
                front.Z = -MathF.Cos(pitch) * MathF.Cos(yaw);

                return Vector3.Normalize(front);
            }
        }

        [JsonIgnore]
        public Vector3 Right
        {
            get => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        }

        [JsonIgnore]
        public Vector3 Up
        {
            get => Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(owner.transform.position, owner.transform.position + Front, Vector3.UnitY);
        }

        [JsonIgnore]
        public Vector3 Position
        {
            get => owner.transform.position;
            set => owner.transform.position = value;
        }

        [JsonIgnore]
        public float Pitch
        {
            get => owner.transform.rotation.X;
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                owner.transform.rotation.X = angle;
            }
        }

        [JsonIgnore]
        public float Yaw
        {
            get => owner.transform.rotation.Y;
            set
            {
                owner.transform.rotation.Y = value;
            }
        }

        [JsonIgnore]
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        public void SetAsMain()
        {
            Camera.main = this;
        }

    }
}
