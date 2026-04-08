using Assimp;


using OpenTK.Graphics.OpenGL4;
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

            material.shader.Use();

            //Transforms
            Camera mainCamera = Camera.Main;
            if (mainCamera != null)
            {
                material.shader.SetMatrix4("view", mainCamera.GetViewMatrix());
                material.shader.SetMatrix4("projection", mainCamera.GetProjectionMatrix());
                material.shader.SetVector3("viewPos", mainCamera.Position);
            }
            material.shader.SetMatrix4("model", owner.transform.ModelMatrix);

            //Directional Light
            Light? dirLight = RenderManager.GetActiveDirectionalLight();
            if (dirLight != null)
            {
                material.shader.SetVector3("dirLight.direction", dirLight.transform.Forward);
                material.shader.SetVector3("dirLight.ambient", dirLight.Ambient);
                material.shader.SetVector3("dirLight.diffuse", dirLight.Diffuse);
                material.shader.SetVector3("dirLight.specular", dirLight.Specular);
            }

            //Point Lights
            List<Light> pointLights = RenderManager.GetActivePointLights();

            material.shader.SetInt("activePointLights", Math.Clamp(pointLights.Count, 0, RenderManager.MAX_POINT_LIGHTS));

            for (int i = 0; i < 4; i++)
            {
                if (i < pointLights.Count)
                {
                    Light pLight = pointLights[i];

                    material.shader.SetVector3($"pointLights[{i}].position", pLight.transform.position);

                    material.shader.SetVector3($"pointLights[{i}].ambient", pLight.Ambient);
                    material.shader.SetVector3($"pointLights[{i}].diffuse", pLight.Diffuse);
                    material.shader.SetVector3($"pointLights[{i}].specular", pLight.Specular);

                    material.shader.SetFloat($"pointLights[{i}].constant", pLight.Constant);
                    material.shader.SetFloat($"pointLights[{i}].linear", pLight.Linear);
                    material.shader.SetFloat($"pointLights[{i}].quadratic", pLight.Quadratic);
                }
                else
                {
                    material.shader.SetFloat($"pointLights[{i}].constant", 0.0f);
                }
            }

            //SpotLights
            List<Light> spotLights = RenderManager.GetActiveSpotLights();

            material.shader.SetInt("activeSpotLights", spotLights.Count/*Math.Clamp(spotLights.Count, 0, RenderManager.MAX_SPOT_LIGHTS)*/);

            for (int i = 0; i < 2; i++)
            {
                if (i < spotLights.Count)
                {
                    Light sLight = spotLights[i];

                    material.shader.SetVector3($"spotLights[{i}].position", sLight.transform.position);
                    material.shader.SetVector3($"spotLights[{i}].direction", sLight.transform.Forward);

                    material.shader.SetVector3($"spotLights[{i}].ambient", sLight.Ambient);
                    material.shader.SetVector3($"spotLights[{i}].diffuse", sLight.Diffuse);
                    material.shader.SetVector3($"spotLights[{i}].specular", sLight.Specular);

                    material.shader.SetFloat($"spotLights[{i}].constant", sLight.Constant);
                    material.shader.SetFloat($"spotLights[{i}].linear", sLight.Linear);
                    material.shader.SetFloat($"spotLights[{i}].quadratic", sLight.Quadratic);
                    material.shader.SetFloat($"spotLights[{i}].cutOff", sLight.CutOff);
                    material.shader.SetFloat($"spotLights[{i}].outerCutOff", sLight.OuterCutOff);
                }
                else
                {
                    material.shader.SetFloat($"spotLights[{i}].constant", 0.0f);
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
        public void RenderDepth(Shader customShader)
        {
            if (!initialized) return;

            customShader.Use();

            customShader.SetMatrix4("model", owner.transform.ModelMatrix);

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
                PostProcessSteps.SortByPrimitiveType |
                PostProcessSteps.CalculateTangentSpace
            );

            vertexData = new();
            indices = new();
            indexCount = 0;

            foreach (var mesh in scene.Meshes)
            {
                bool hasTangents = mesh.HasTangentBasis;
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var pos = mesh.Vertices[i];
                    var normal = mesh.Normals[i];
                    var uv = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][i] : new Assimp.Vector3D(0, 0, 0);
                    var tangent = hasTangents ? mesh.Tangents[i] : new Assimp.Vector3D(0, 0, 0);
                    var bitangent = hasTangents ? mesh.BiTangents[i] : new Assimp.Vector3D(0, 0, 0);

                    vertexData.AddRange(new float[]
                    {
                        pos.X, pos.Y, pos.Z,
                        normal.X, normal.Y, normal.Z,
                        uv.X, uv.Y,
                        tangent.X, tangent.Y, tangent.Z,
                        bitangent.X, bitangent.Y, bitangent.Z
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
            if (material == null)
            {
                Shader defaultShader = new Shader("basic.vert", "basic.frag");
                material = new Material(defaultShader);
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

            int stride = 14 * sizeof(float);

            // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // Normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // UV
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            //Tangent
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));

            //Bitangent
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, stride, 11 * sizeof(float));

            GL.BindVertexArray(0);

            initialized = true;
        }

        public Material GetMaterial()
        {
            return material;
        }
        public Shader? GetShader()
        {
            return material?.shader;
        }
        public void SetMaterial(Material material)
        {
            this.material = material;
        }


        public override void Destroy()
        {
            RenderManager.RemoveRenderer(this);

            // Liberar la memoria de la tarjeta gráfica
            // (Asegúrate de que estas variables coincidan con cómo las llamaste en tu Renderer)
            if (vao != 0) GL.DeleteVertexArray(vao);
            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (ebo != 0) GL.DeleteBuffer(ebo);

            // Opcional: Si el material carga texturas únicas, también podrías llamar a GL.DeleteTexture()
        }
    }
}
