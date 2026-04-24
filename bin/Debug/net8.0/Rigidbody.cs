using BulletSharp;
using BulletSharp.Math;
using OpenTK.Mathematics;
using System.Text.Json.Serialization;
using static BulletSharp.Dbvt;

namespace GameEngine
{
    public class Rigidbody : Component
    {
        public float Mass = 1.0f;
        private RigidBody BulletBody;

        public float LinearDrag = 0.0f;
        public float AngularDrag = 0.0f;
        public PhysicsMaterial physicsMaterial = new PhysicsMaterial();
        private CollisionShape _shape;

        [JsonIgnore]
        public float Friction
        {
            get { return physicsMaterial.Friction; }
            set
            {
                if(physicsMaterial == null)
                {
                    physicsMaterial = new PhysicsMaterial();
                }
                physicsMaterial.Friction = value;
            }
        }

        [JsonIgnore]
        public float Bounciness
        {
            get { return physicsMaterial.Bounciness; }
            set
            {
                if (physicsMaterial == null)
                {
                    physicsMaterial = new PhysicsMaterial();
                }
                physicsMaterial.Bounciness = value;
            }
        }

        private bool _useGravity = true;
        public bool UseGravity
        {
            get { return _useGravity; }
            set
            {
                _useGravity = value;
                if (BulletBody != null)
                {
                    if (!_useGravity)
                    {
                        BulletBody.Gravity = BulletSharp.Math.Vector3.Zero;
                    }
                    else
                    {
                        var globalGrav = PhysicsManager.Gravity;
                        BulletBody.Gravity = new BulletSharp.Math.Vector3(globalGrav.X, globalGrav.Y, globalGrav.Z);
                    }
                }
            }
        }

        public Rigidbody(GameObject owner) : base(owner) { }

        public override void Start()
        {
            //Autodetect Collider
            Collider col = owner.GetComponent<Collider>();
            

            if (col != null && col.Shape != null)
            {
                _shape = col.Shape;
                _shape.LocalScaling = new BulletSharp.Math.Vector3(owner.transform.scale.X, owner.transform.scale.Y, owner.transform.scale.Z);
            }
            else
            {
                _shape = new EmptyShape();
            }

            // Calculate Inertia
            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;
            if (Mass > 0.0f)
            {
                _shape.CalculateLocalInertia(Mass, out localInertia);
            }

            // Initialize BulletBody
            var pos = owner.transform.position;

            var rot = owner.transform.rotation;
            float radX = MathHelper.DegreesToRadians(rot.X);
            float radY = MathHelper.DegreesToRadians(rot.Y);
            float radZ = MathHelper.DegreesToRadians(rot.Z);

            Matrix startTransform = Matrix.RotationYawPitchRoll(radY, radX, radZ) * Matrix.Translation(pos.X, pos.Y, pos.Z);

            DefaultMotionState motionState = new DefaultMotionState(startTransform);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, motionState, _shape, localInertia);
            BulletBody = new BulletSharp.RigidBody(rbInfo);
            PhysicsManager.AddRigidBody(BulletBody);

            rbInfo.Dispose();

            //Set Gravity
            if (!_useGravity)
            {
                BulletBody.Gravity = BulletSharp.Math.Vector3.Zero;
            }

            //Set Physical Material Properties
            BulletBody.SetDamping(LinearDrag, AngularDrag); 
            BulletBody.Friction = physicsMaterial.Friction;                 
            BulletBody.Restitution = physicsMaterial.Bounciness;
        }

        public Rigidbody SetVelocity(OpenTK.Mathematics.Vector3 velocity)
        {
            if (BulletBody != null && Mass > 0.0f)
            {
                BulletBody.ActivationState = ActivationState.ActiveTag;

                BulletBody.LinearVelocity = new BulletSharp.Math.Vector3(velocity.X, velocity.Y, velocity.Z);
            }

            return this;
        }

        public Rigidbody SetMass(float mass)
        {
            Mass = mass;
            if (BulletBody != null)
            {
                BulletBody.SetMassProps(Mass, BulletSharp.Math.Vector3.Zero);
            }
            return this;
        }

        public Rigidbody SetUseGravity(bool useGravity)
        {
            UseGravity = useGravity;
            return this;
        }

        public Rigidbody SetLinearDrag(float linearDrag)
        {
            LinearDrag = linearDrag;
            if (BulletBody != null)
            {
                BulletBody.SetDamping(LinearDrag, AngularDrag);
            }
            return this;
        }

        public Rigidbody SetAngularDrag(float angularDrag)
        {
            AngularDrag = angularDrag;
            if (BulletBody != null)
            {
                BulletBody.SetDamping(LinearDrag, AngularDrag);
            }
            return this;
        }

        public Rigidbody SetFriction(float friction)
        {
            Friction = friction;
            if (BulletBody != null)
            {
                BulletBody.Friction = physicsMaterial.Friction;
            }
            return this;
        }

        public Rigidbody SetBounciness(float bounciness)
        {
            Bounciness = bounciness;
            if (BulletBody != null)
            {
                BulletBody.Restitution = physicsMaterial.Bounciness;
            }
            return this;
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            if (Mass > 0 && BulletBody != null && BulletBody.MotionState != null)
            {
                BulletBody.MotionState.GetWorldTransform(out Matrix currentTransform);

                owner.transform.position = new OpenTK.Mathematics.Vector3(
                    currentTransform.Origin.X,
                    currentTransform.Origin.Y,
                    currentTransform.Origin.Z
                );

                OpenTK.Mathematics.Matrix4 tkMatrix = new OpenTK.Mathematics.Matrix4(
                    currentTransform.M11, currentTransform.M12, currentTransform.M13, currentTransform.M14,
                    currentTransform.M21, currentTransform.M22, currentTransform.M23, currentTransform.M24,
                    currentTransform.M31, currentTransform.M32, currentTransform.M33, currentTransform.M34,
                    currentTransform.M41, currentTransform.M42, currentTransform.M43, currentTransform.M44
                );

                OpenTK.Mathematics.Vector3 radEuler = tkMatrix.ExtractRotation().ToEulerAngles();
                owner.transform.rotation = new OpenTK.Mathematics.Vector3(
                    MathHelper.RadiansToDegrees(radEuler.X),
                    MathHelper.RadiansToDegrees(radEuler.Y),
                    MathHelper.RadiansToDegrees(radEuler.Z)
                );
            }
        }

        public override void Destroy()
        {
            if (BulletBody != null)
            {
                PhysicsManager.RemoveRigidBody(BulletBody);
                BulletBody.Dispose();
            }
        }

    }
}