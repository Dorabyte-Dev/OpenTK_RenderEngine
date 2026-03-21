using BulletSharp;
using BulletSharp.Math;
using OpenTK.Mathematics;
using static BulletSharp.Dbvt;

namespace GameEngine
{
    public class Rigidbody : Component
    {
        public float Mass = 1.0f;
        public BulletSharp.RigidBody BulletBody { get; private set; }

        public Rigidbody(GameObject owner) : base(owner) { }

        public override void Start()
        {
            // 1. AUTODETECCIÓN: Buscamos si el GameObject tiene una "carcasa"
            Collider col = owner.GetComponent<Collider>();
            CollisionShape shape;

            if (col != null && col.Shape != null)
            {
                shape = col.Shape; // ¡Encontramos el collider! Usamos su forma.
            }
            else
            {
                shape = new EmptyShape(); // Si no hay collider, es invisible para los choques
            }

            // 2. MATEMÁTICAS DE BULLET
            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;
            if (Mass > 0.0f)
            {
                shape.CalculateLocalInertia(Mass, out localInertia);
            }

            // Pasamos la posición de OpenTK a la matriz de Bullet
            var pos = owner.transform.position;
            Matrix startTransform = Matrix.Translation(pos.X, pos.Y, pos.Z);
            DefaultMotionState motionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, motionState, shape, localInertia);
            BulletBody = new BulletSharp.RigidBody(rbInfo);

            // 3. INYECCIÓN EN EL MUNDO: Usamos el Singleton de Game (ver nota abajo)
            PhysicsManager.AddRigidBody(BulletBody);
        }

        // Sincronizamos la caída calculada por Bullet con tu Transform visual
        public override void FixedUpdate(float fixedDeltaTime)
        {
            if (Mass > 0 && BulletBody != null && BulletBody.MotionState != null)
            {
                BulletBody.MotionState.GetWorldTransform(out Matrix currentTransform);

                // Actualizamos la posición visual
                owner.transform.position = new OpenTK.Mathematics.Vector3(
                    currentTransform.Origin.X,
                    currentTransform.Origin.Y,
                    currentTransform.Origin.Z
                );
            }
        }

        public override void OnDestroy()
        {
            if (BulletBody != null)
            {
                PhysicsManager.RemoveRigidBody(BulletBody);
                BulletBody.Dispose();
            }
        }
    }
}