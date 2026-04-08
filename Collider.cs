using BulletSharp;
using Newtonsoft.Json;

namespace GameEngine
{
    public class Collider : Component
    {
        [JsonIgnore] public CollisionShape Shape { get; private set; }

        public Collider(GameObject owner) : base(owner)
        {
            Shape = new BoxShape(0.5f, 0.5f, 0.5f);
            Shape.LocalScaling = new BulletSharp.Math.Vector3(owner.transform.scale.X, owner.transform.scale.Y, owner.transform.scale.Z);
        }

        public override void Destroy()
        {
            if (Shape != null)
                Shape.Dispose();
        }
    }
}