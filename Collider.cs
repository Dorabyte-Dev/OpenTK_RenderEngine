using BulletSharp;

namespace GameEngine
{
    public class Collider : Component
    {
        // La propiedad que el Rigidbody va a venir a buscar
        public CollisionShape Shape { get; private set; }

        public Collider(GameObject owner) : base(owner)
        {
            // Bullet usa "Half-Extents" (medios tamaños). 
            // Si quieres un cubo de 1x1x1, le pasas 0.5f en cada eje.
            Shape = new BoxShape(0.5f, 0.5f, 0.5f);
        }

        public override void OnDestroy()
        {
            if (Shape != null)
                Shape.Dispose();
        }
    }
}