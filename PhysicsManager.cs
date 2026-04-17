using BulletSharp;
using BulletSharp.Math; // Recuerda: Bullet usa sus propias matemáticas internamente

namespace GameEngine
{
    public static class PhysicsManager
    {
        //Physics Pipeline
        private static DefaultCollisionConfiguration _collisionConf;
        private static CollisionDispatcher _dispatcher;
        private static DbvtBroadphase _broadphase;
        private static SequentialImpulseConstraintSolver _solver;
        private static DiscreteDynamicsWorld _physicsWorld;

        //Global Gravity
        public static Vector3 Gravity
        {
            get { return _physicsWorld != null ? _physicsWorld.Gravity : Vector3.Zero; }
            set { if (_physicsWorld != null) _physicsWorld.Gravity = value; }
        }

        public static void InitializePhysics()
        {
            _collisionConf = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConf);
            _broadphase = new DbvtBroadphase();
            _solver = new SequentialImpulseConstraintSolver();

            _physicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConf);

            Gravity = new Vector3(0, -9.81f, 0);
        }

        public static void InitializePhysics(float gravityScale)
        {
            _collisionConf = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConf);
            _broadphase = new DbvtBroadphase();
            _solver = new SequentialImpulseConstraintSolver();

            _physicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConf);

            Gravity = new Vector3(0, -gravityScale, 0);
        }

        public static void InitializePhysics(Vector3 gravityDirection)
        {
            _collisionConf = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(_collisionConf);
            _broadphase = new DbvtBroadphase();
            _solver = new SequentialImpulseConstraintSolver();

            _physicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConf);

            Gravity = gravityDirection;
        }

        //Called on OnUpdateFrame inside Game.cs
        public static void StepSimulation(float deltaTime)
        {
            if (_physicsWorld != null)
            {
                _physicsWorld.StepSimulation(deltaTime, 10);
            }
        }

        //Bridge methods for adding/removing rigid bodies to the physics world
        public static void AddRigidBody(BulletSharp.RigidBody body)
        {
            if (_physicsWorld != null && body != null)
                _physicsWorld.AddRigidBody(body);
        }

        public static void RemoveRigidBody(BulletSharp.RigidBody body)
        {
            if (_physicsWorld != null && body != null)
                _physicsWorld.RemoveRigidBody(body);
        }

        public static void CleanUp()
        {
            _physicsWorld?.Dispose();
            _solver?.Dispose();
            _broadphase?.Dispose();
            _dispatcher?.Dispose();
            _collisionConf?.Dispose();
        }

        public static void ResetPhysics()
        {
            CleanUp();
            InitializePhysics();
        }
    }
}
