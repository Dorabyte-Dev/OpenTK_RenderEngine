using OpenTK.Mathematics;
using Newtonsoft.Json;

namespace GameEngine
{
    public class Transform : Component
    {
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero; 
        public Vector3 scale = Vector3.One;
        [JsonConstructor]
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

        public Transform SetPosition(Vector3 newPosition)
        {
            position = newPosition;
            return this;
        }

        public Transform SetRotation(Vector3 newRotation)
        {
            rotation = newRotation;
            return this;
        }

        public Transform SetScale(Vector3 newScale)
        {
            scale = newScale;
            return this;
        }
        [JsonIgnore]public Vector3 Forward
        {
            get
            {
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );

                Quaternion q = Quaternion.FromEulerAngles(radRot);

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

                Vector3 up = Vector3.Transform(Vector3.UnitY, q);
                return up.Normalized();
            }
        }

        [JsonIgnore]
        public Matrix4 ModelMatrix
        {
            get
            {
                Vector3 radRot = new Vector3(
                    MathHelper.DegreesToRadians(rotation.X),
                    MathHelper.DegreesToRadians(rotation.Y),
                    MathHelper.DegreesToRadians(rotation.Z)
                );

                Matrix4 scaleMat = Matrix4.CreateScale(scale);
                Matrix4 rotMat = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(radRot));
                Matrix4 transMat = Matrix4.CreateTranslation(position);

                return scaleMat * rotMat * transMat;
            }
        }
    }
}
