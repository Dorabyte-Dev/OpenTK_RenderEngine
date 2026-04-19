using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace GameEngine { 
    public class GameObject
    {

        [JsonProperty]private string name = "GameObject";
        [JsonProperty]private bool active = true;

        private Transform _transform;

        [JsonIgnore] public Transform transform => _transform;
        [JsonIgnore]public string Name => name;
        [JsonIgnore]public bool Active => active;

        [JsonProperty] private List<Component> components = new List<Component>();
        public GameObject() {
            name = "GameObject";
            active = true;
            _transform = AddComponent<Transform>();

            if (SceneManager.CurrentScene != null)
            {
                SceneManager.CurrentScene.AddGameObject(this);
            }
        }

        public GameObject(string name)
        {
            this.name = name;
            active = true;
            _transform = AddComponent<Transform>();

            if (SceneManager.CurrentScene != null)
            {
                SceneManager.CurrentScene.AddGameObject(this);
            }
        }

        public T AddComponent<T>() where T : Component
        {
            T component = (T)Activator.CreateInstance(typeof(T), this);
            components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var c in components)
                if (c is T t) return t;

            Console.WriteLine("Error: Component of type " + typeof(T) + " not found in GameObject '" + name + "'.");
            return null;
        }

        public string DebugComponents()
        {
            string str = string.Empty;
            foreach(var c in components)
            {
                str += c.GetType();
            }
            return str;
        }

        public void Start()
        {
            foreach (var c in components)
                c.Start();
        }

        public void Update(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.Update(deltaTime);
        }

        public void FixedUpdate(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.FixedUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (var c in components)
                if (c.enabled)
                    c.LateUpdate(deltaTime);
        }

        public void Destroy()
        {
            foreach (var c in components)
            {
                c.Destroy();
            }
            components.Clear();

        }
        #region Primitives

        public static GameObject GenerateCube(bool hasRigidbody = false, Material? mat = null)
        {
            return GeneratePrimitive(PrimitiveObjectType.Cube, hasRigidbody, mat);
        }

        public static GameObject GenerateCone(bool hasRigidbody = false, Material? mat = null)
        {
            return GeneratePrimitive(PrimitiveObjectType.Cone, hasRigidbody, mat);
        }

        public static GameObject GenerateCylinder(bool hasRigidbody = false, Material? mat = null)
        {
            return GeneratePrimitive(PrimitiveObjectType.Cylinder, hasRigidbody, mat);
        }

        public static GameObject GeneratePlane(bool hasRigidbody = false, Material? mat = null)
        {
            return GeneratePrimitive(PrimitiveObjectType.Plane, hasRigidbody, mat);
        }

        public static GameObject GenerateSphere(bool hasRigidbody = false, Material? mat = null)
        {
            return GeneratePrimitive(PrimitiveObjectType.Sphere, hasRigidbody, mat);
        }
        private static GameObject GeneratePrimitive(PrimitiveObjectType type, bool hasRigidbody = false, Material? mat = null)
        {
            Material basicMat;
            if (mat == null)
            {
                Shader litShader = new Shader("phong.vert", "phong.frag");
                Texture texture = Texture.LoadFromFile("Textures/WhiteTex.jpg");
                basicMat = new Material(litShader).SetTexture("material.diffuse", texture);
            }
            else
            {
                basicMat = mat;
            }

            string name;
            string modelPath = "Models/Primitives/";
            switch(type)
            {
                case PrimitiveObjectType.Cube:
                    name = "Primitive Cube";
                    modelPath += "Cube.fbx";
                    break;
                case PrimitiveObjectType.Cone:
                    name = "Primitive Cone";
                    modelPath += "Cone.fbx";
                    break;
                case PrimitiveObjectType.Cylinder:
                    name = "Primitive Cylinder";
                    modelPath += "Cylinder.fbx";
                    break;
                case PrimitiveObjectType.Plane:
                    name = "Primitive Plane";
                    modelPath += "Plane.fbx";
                    break;
                case PrimitiveObjectType.Sphere:
                    name = "Primitive Sphere";
                    modelPath += "Sphere.fbx";
                    break;
                default:
                    name = "Primitive Error";
                    modelPath += "Cube.fbx";
                    break;
            }

            GameObject obj = new GameObject(name);
            obj.AddComponent<Renderer>().LoadModel(modelPath).SetMaterial(basicMat);

            if (hasRigidbody)
            {
                switch(type)
                {
                    case PrimitiveObjectType.Cube:
                        obj.AddComponent<Collider>().SetBox(new Vector3(0.5f, 0.5f, 0.5f));
                        break;
                    case PrimitiveObjectType.Cone:
                        obj.AddComponent<Collider>().SetConvexMesh("Models/Primitives/ConeCollider.fbx");
                        break;
                    case PrimitiveObjectType.Cylinder:
                        obj.AddComponent<Collider>().SetCapsule(0.5f, 1f);
                        break;
                    case PrimitiveObjectType.Sphere:
                        obj.AddComponent<Collider>().SetSphere(0.5f);
                        break;
                    case PrimitiveObjectType.Plane:
                        obj.AddComponent<Collider>().SetBox(new Vector3(5f, 0.1f, 5f));
                        break;
                    default:
                        break;
                }
                obj.AddComponent<Rigidbody>();
            }

            return obj;
        }
        #endregion
        #region Json
        [JsonConstructor]
        private GameObject(string name, bool active)
        {
            this.name = name;
            this.active = active;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            _transform = GetComponent<Transform>();

            foreach (var c in components)
            {
                c.owner = this;
            }
        }
        #endregion
    }

    enum PrimitiveObjectType
    {
        Cube,
        Cone,
        Cylinder,
        Plane,
        Sphere
    }
}
