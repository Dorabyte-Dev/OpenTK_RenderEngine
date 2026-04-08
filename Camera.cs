using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System;


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

        public Vector3 Front => owner.transform.Forward;
        public Vector3 Up => owner.transform.Up;
        public Vector3 Right => owner.transform.Right;

        public Vector3 Position
        {
            get => owner.transform.position;
            set => owner.transform.position = value;
        }

        public float Pitch
        {
            get => owner.transform.rotation.X;
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                owner.transform.rotation.X = angle;
            }
        }

        public float Yaw
        {
            get => owner.transform.rotation.Y;
            set
            {
                owner.transform.rotation.Y = value;
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(owner.transform.position, owner.transform.position + owner.transform.Forward, owner.transform.Up);
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
