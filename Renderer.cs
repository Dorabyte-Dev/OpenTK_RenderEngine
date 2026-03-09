using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace GameEngine
{
    public class Renderer : Component
    {
        // OpenGL
        private int vao;
        private int vbo;
        private int ebo;

        // Geometry
        private List<float> vertexData = new();
        private List<uint> indices = new();
        private int indexCount;

        // Resources
        private Shader shader;
        private Material material;

        // State
        private bool initialized = false;

        public Renderer(GameObject owner) : base(owner) 
        {
            RenderManager.AddRenderer(this);
        }



        public override void Start()
        {
            
        }

        public void Render()
        {
            if (!initialized) return;

            shader.Use();

            //Transforms
            Camera mainCamera = Camera.Main;
            if (mainCamera != null)
            {
                shader.SetMatrix4("view", mainCamera.GetViewMatrix());
                shader.SetMatrix4("projection", mainCamera.GetProjectionMatrix());
                shader.SetVector3("viewPos", mainCamera.Position);
                Console.WriteLine("Main Camera Position " + mainCamera.Position);
            }
            shader.SetMatrix4("model", Matrix4.CreateTranslation(owner.transform.position));

            //Directional Light
            Light? dirLight = RenderManager.GetActiveDirectionalLight();
            if (dirLight != null)
            {
                shader.SetVector3("dirLight.direction", dirLight.Forward);
                shader.SetVector3("dirLight.ambient", dirLight.Ambient);
                shader.SetVector3("dirLight.diffuse", dirLight.Diffuse);
                shader.SetVector3("dirLight.specular", dirLight.Specular);
            }

            // --- CÓDIGO DE LAS POINT LIGHTS ---
            List<Light> pointLights = RenderManager.GetActivePointLights();

            for (int i = 0; i < 4; i++)
            {
                if (i < pointLights.Count)
                {
                    Light pLight = pointLights[i];

                    shader.SetVector3($"pointLights[{i}].position", pLight.Position);

                    shader.SetVector3($"pointLights[{i}].ambient", pLight.Ambient);
                    shader.SetVector3($"pointLights[{i}].diffuse", pLight.Diffuse);
                    shader.SetVector3($"pointLights[{i}].specular", pLight.Specular);

                    shader.SetFloat($"pointLights[{i}].constant", pLight.Constant);
                    shader.SetFloat($"pointLights[{i}].linear", pLight.Linear);
                    shader.SetFloat($"pointLights[{i}].quadratic", pLight.Quadratic);
                }
                else
                {
                    // Si no hay luz para este índice, la apagamos seteando constant a 0
                    shader.SetFloat($"pointLights[{i}].constant", 0.0f);
                }
            }

            material.Apply();

            GL.BindVertexArray(vao);

            GL.DrawElements(
                PrimitiveType.Triangles,
                indexCount,
                DrawElementsType.UnsignedInt,
                0
            );

            GL.BindVertexArray(0);
        }

        public void LoadModel(string path)
        {
            ImportModel(path);

            InitializeBuffers();
        }

        private void ImportModel(string path)
        {
            AssimpContext importer = new AssimpContext();

            var scene = importer.ImportFile(
                path,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.SortByPrimitiveType
            );

            vertexData = new();
            indices = new();
            indexCount = 0;

            foreach (var mesh in scene.Meshes)
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var pos = mesh.Vertices[i];
                    var normal = mesh.Normals[i];
                    var uv = mesh.TextureCoordinateChannels[0][i];

                    vertexData.AddRange(new float[]
                    {
                        pos.X, pos.Y, pos.Z,
                        normal.X, normal.Y, normal.Z,
                        uv.X, uv.Y
                    });
                }

                foreach (var face in mesh.Faces)
                {
                    foreach (var idx in face.Indices)
                    {
                        indices.Add((uint)idx);
                        indexCount++;
                    }
                }
            }
        }

        private void InitializeBuffers()
        {
            if (material != null)
            {
                shader = material.shader;
            }
            else
            {
                shader = new Shader("basic.vert", "basic.frag");
            }
                

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            //VertexBufferObject
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertexData.Count * sizeof(float),
                vertexData.ToArray(),
                BufferUsageHint.StaticDraw
            );

            //ElementBufferObject
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                indices.Count * sizeof(uint),
                indices.ToArray(),
                BufferUsageHint.StaticDraw
            );

            int stride = 8 * sizeof(float);

            // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // Normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // UV
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);

            initialized = true;
        }

        public void SetShader(Shader shader)
        {
            this.shader = shader;
        }

        public void SetMaterial(Material material)
        {
            this.material = material;
        }

    }
}
