using System;
using OpenTK.Mathematics;
using BulletSharp;
using BulletSharp.Math; // Precaución: Bullet tiene sus propios Vector3 y Matrix

namespace GameEngine
{
    public class Rigidbody : Component
    {
        public float Mass = 1.0f; // Por defecto tiene masa (caerá)
        public BulletSharp.RigidBody BulletBody { get; private set; }

        // Constructor que exige tu clase Component base
        public Rigidbody(GameObject owner) : base(owner) { }

        // Lo llamaremos manualmente desde Game.cs una vez hayamos añadido el Collider
        public void Initialize(CollisionShape shape, DiscreteDynamicsWorld physicsWorld)
        {
            // 1. Calcular la inercia (solo si se mueve, es decir, Masa > 0)
            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;
            if (Mass != 0.0f)
            {
                shape.CalculateLocalInertia(Mass, out localInertia);
            }

            // 2. Extraer la posición de tu Transform y convertirla a Matriz de Bullet
            var pos = owner.transform.position;
            BulletSharp.Math.Matrix startTransform = BulletSharp.Math.Matrix.Translation(pos.X, pos.Y, pos.Z);

            // 3. Crear el estado de movimiento (optimiza la sincronización)
            DefaultMotionState motionState = new DefaultMotionState(startTransform);

            // 4. Construir el RigidBody interno de Bullet
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, motionState, shape, localInertia);
            BulletBody = new BulletSharp.RigidBody(rbInfo);

            // 5. Añadirlo al mundo físico
            physicsWorld.AddRigidBody(BulletBody);
        }

        // Usamos tu método FixedUpdate para sincronizar la física con los gráficos
        public override void FixedUpdate(float fixedDeltaTime)
        {
            // Si el objeto no tiene masa, es estático, no necesitamos gastar recursos en moverlo
            if (Mass > 0 && BulletBody != null && BulletBody.MotionState != null)
            {
                // Extraemos la matriz de transformación que ha calculado Bullet
                BulletSharp.Math.Matrix currentTransform;
                BulletBody.MotionState.GetWorldTransform(out currentTransform);

                // ACTUALIZAR POSICIÓN: De Bullet a tu Transform de OpenTK
                owner.transform.position = new OpenTK.Mathematics.Vector3(
                    currentTransform.Origin.X,
                    currentTransform.Origin.Y,
                    currentTransform.Origin.Z
                );

                // ACTUALIZAR ROTACIÓN: Extraer de la matriz de Bullet y pasar a tus grados de Euler
                // (Omitido temporalmente para simplificar este primer paso de caída por gravedad)
            }
        }

        public override void OnDestroy()
        {
            // Limpieza vital de memoria
            if (BulletBody != null)
            {
                // Deberíamos sacarlo del mundo físico también, pero requeriría referencia al PhysicsWorld
                BulletBody.Dispose();
            }
        }
    }
}

}