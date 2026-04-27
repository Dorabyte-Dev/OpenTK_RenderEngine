using Assimp;
using BulletSharp;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System.Runtime.Serialization;

namespace GameEngine
{
    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule,
        ConvexMesh, 
        ConcaveMesh
    }
    public class Collider : Component
    {
        private struct ModelShapeData
        {
            public List<Vector3> Vertices;
            public List<int> Indices;
        }
        [JsonProperty] public ColliderType Type { get; private set; } = ColliderType.Box;
        [JsonProperty] public Vector3 Dimensions { get; private set; } = new Vector3(0.5f, 0.5f, 0.5f);

        [JsonIgnore] public CollisionShape Shape { get; private set; }
        [JsonProperty] public string MeshPath { get; private set; }

        public Collider(GameObject owner) : base(owner)
        {
            GenerateShape();
        }

        [JsonConstructor]
        private Collider() : base()
        {
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            GenerateShape();
        }

        
        private void GenerateShape()
        {
            
            if (Shape != null)
            {
                Shape.Dispose();
            }

            switch (Type)
            {
                case ColliderType.Box:
                    
                    Shape = new BoxShape(Dimensions.X, Dimensions.Y, Dimensions.Z);
                    break;
                case ColliderType.Sphere:
                    Shape = new SphereShape(Dimensions.X);
                    break;
                case ColliderType.Capsule:
                    Shape = new CapsuleShape(Dimensions.X, Dimensions.Y);
                    break;
                case ColliderType.ConvexMesh:
                    ModelShapeData modelDataConvex = ImportMesh(MeshPath);

                    List<Vector3> convexVerts = modelDataConvex.Vertices;

                    var convexShape = new ConvexHullShape();
                    foreach (var v in convexVerts)
                    {
                        convexShape.AddPoint(new BulletSharp.Math.Vector3(v.X, v.Y, v.Z));
                    }
                    convexShape.OptimizeConvexHull();
                    Shape = convexShape;
                    break;

                case ColliderType.ConcaveMesh:

                    ModelShapeData modelDataConcave = ImportMesh(MeshPath);

                    List<Vector3> concaveVerts = modelDataConcave.Vertices;
                    List<int> indices = modelDataConcave.Indices;

                    var triangleMesh = new TriangleMesh();
                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var v1 = concaveVerts[indices[i]];
                        var v2 = concaveVerts[indices[i + 1]];
                        var v3 = concaveVerts[indices[i + 2]];

                        triangleMesh.AddTriangle(
                            new BulletSharp.Math.Vector3(v1.X, v1.Y, v1.Z),
                            new BulletSharp.Math.Vector3(v2.X, v2.Y, v2.Z),
                            new BulletSharp.Math.Vector3(v3.X, v3.Y, v3.Z)
                        );
                    }
                    Shape = new BvhTriangleMeshShape(triangleMesh, true);
                    break;
                default:
                    Shape = new BoxShape(0.5f, 0.5f, 0.5f);
                    break;
            }

            ApplyScale();
        }

        private ModelShapeData ImportMesh(string path)
        {
            ModelShapeData msd = new ModelShapeData
            {
                Vertices = new List<Vector3>(),
                Indices = new List<int>()
            };

            AssimpContext importer = new AssimpContext();

            var scene = importer.ImportFile(
                path,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.SortByPrimitiveType |
                PostProcessSteps.CalculateTangentSpace
            );
            foreach (var mesh in scene.Meshes)
            {
                foreach (var vertex in mesh.Vertices)
                {
                    msd.Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
                }

                foreach (var face in mesh.Faces)
                {
                    msd.Indices.AddRange(face.Indices);
                }
            }

            return msd;
        }

        private void ApplyScale()
        {
            if (Shape != null && owner != null && owner.transform != null)
            {
                Shape.LocalScaling = new BulletSharp.Math.Vector3(
                    owner.transform.scale.X,
                    owner.transform.scale.Y,
                    owner.transform.scale.Z
                );
            }
        }

        public override void Start()
        {
            ApplyScale();
        }

        public Collider SetBox(Vector3 halfExtents)
        {
            Type = ColliderType.Box;
            Dimensions = halfExtents;
            GenerateShape();
            return this;
        }

        public Collider SetSphere(float radius)
        {
            Type = ColliderType.Sphere;
            Dimensions = new Vector3(radius, 0, 0);
            GenerateShape();
            return this;
        }

        public Collider SetCapsule(float radius, float height)
        {
            Type = ColliderType.Capsule;
            Dimensions = new Vector3(radius, height, 0);
            GenerateShape();
            return this;
        }
        public Collider SetConvexMesh(string modelPath)
        {
            Type = ColliderType.ConvexMesh;
            MeshPath = modelPath;
            GenerateShape();
            return this;
        }

        public Collider SetConcaveMesh(string modelPath)
        {
            Type = ColliderType.ConcaveMesh;
            MeshPath = modelPath;
            GenerateShape();
            return this;
        }
        public override void Destroy()
        {
            if (Shape != null)
                Shape.Dispose();
        }
    }
}