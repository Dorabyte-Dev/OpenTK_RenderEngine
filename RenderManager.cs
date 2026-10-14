using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GameEngine
{
    public static class RenderManager
    {
        #region Variables & Settings
        private static Color4 _clearColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
        public static int ScreenWidth { get; set; } = 800;
        public static int ScreenHeight { get; set; } = 600;

        private static List<Renderer> _renderers = new();
        private static List<Light> _lights = new();

        // Directional Shadows
        public static int depthMapFBO;
        public static int depthMap;
        public static Shader depthShader;
        public const int SHADOW_WIDTH = 2048;
        public const int SHADOW_HEIGHT = 2048;
        private static float near_plane_dir = 1.0f;
        private static float far_plane_dir = 150.0f;
        private static float shadowOrthoSize = 10.0f;

        // Point Shadows
        public static int depthCubemapFBO;
        public static int depthCubemap;
        public static Shader pointShadowShader;
        public const int SHADOW_CUBE_RESOLUTION = 1024;
        public const int MAX_POINT_LIGHTS = 16;
        public const float near_plane_point = 1.0f;
        public const float far_plane_point = 25.0f;

        //Spot Shadows
        public static int spotDepthMapFBO;
        public static int spotDepthMapArray;
        public const int MAX_SPOT_LIGHTS = 8;
        public const float near_plane_spot = 1.0f;
        public const float far_plane_spot = 50.0f;
        #endregion
        #region Initialization & Frame Lifecycle
        public static void Initialize()
        {
            GL.ClearColor(_clearColor);
            GL.Enable(EnableCap.DepthTest);

            InitializeShadowMaps();
        }

        public static void Resize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
            GL.Viewport(0, 0, width, height);
        }
        #endregion
        #region Light and Renderer Management
        public static void AddLight(Light light)
        {
            if (!_lights.Contains(light))
                _lights.Add(light);
        }

        public static void RemoveLight(Light light)
        {
            if (_lights.Contains(light))
                _lights.Remove(light);
        }

        public static void AddRenderer(Renderer renderer)
        {
            if (!_renderers.Contains(renderer))
            {
                _renderers.Add(renderer);
            }
        }

        public static void RemoveRenderer(Renderer renderer)
        {
            if (_renderers.Contains(renderer))
            {
                _renderers.Remove(renderer);
            }
        }

        public static void ClearSceneData()
        {
            HashSet<Shader> shadersToDispose = new HashSet<Shader>();
            HashSet<Texture> texturesToDispose = new HashSet<Texture>();

            foreach (var renderer in _renderers.ToList())
            {
                Shader s = renderer.GetShader();
                if (s != null)
                {
                    shadersToDispose.Add(s);
                }

                Material mat = renderer.GetMaterial();
                if (mat != null)
                {
                    foreach (var tex in mat.GetAllTextures())
                    {
                        if (tex != null)
                        {
                            texturesToDispose.Add(tex);
                        }
                    }
                }

                renderer.Destroy();
            }

            foreach (var shader in shadersToDispose)
            {
                shader.Dispose();
            }

            foreach (var tex in texturesToDispose)
            {
                tex.Dispose();
            }

            _renderers.Clear();
            _lights.Clear();
        }
        #endregion
        #region Renderer Loop
        public static void Render()
        {
            foreach (var renderer in _renderers)
            {
                renderer.Render();
            }
        }

        private static void RenderDepths(Shader customShader)
        {
            foreach (var renderer in _renderers)
            {
                renderer.RenderDepth(customShader);
            }
        }
        #endregion
        #region Light Queries
        #region Get Active Lights
        public static Light? GetActiveDirectionalLight() => _lights.FirstOrDefault(l => l.Type == LightType.Directional);

        public static List<Light> GetActivePointLights()
        {
            return _lights.Where(l => l.Type == LightType.Point).ToList();
        }

        public static List<Light> GetActiveSpotLights()
        {
            return _lights.Where(l => l.Type == LightType.Spot).ToList();
        }
        #endregion
        #region Shadow Matrices
        public static Matrix4 GetDirectionalLightMatrix(Light dirLight)
        {
            //Projection Matrix
            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-shadowOrthoSize, shadowOrthoSize, -shadowOrthoSize, shadowOrthoSize, near_plane_dir, far_plane_dir);

            //View Matrix
            Vector3 lightPos = dirLight.transform.position;
            Vector3 target = lightPos + dirLight.transform.Forward;
            Vector3 up = Vector3.UnitY;

            Matrix4 lightView = Matrix4.LookAt(lightPos, target, up);

            Matrix4 lightSpaceMatrix = lightView * lightProjection;

            return lightSpaceMatrix;
        }

        public static List<Matrix4> GetPointLightMatrices(Light pointLight, float nearPlane, float farPlane)
        {
            List<Matrix4> shadowTransforms = new List<Matrix4>();
            Vector3 lightPos = pointLight.transform.position;

            Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, nearPlane, farPlane);

            //Cubemap Directions
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Right
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Left
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)) * shadowProj); // Up
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)) * shadowProj); // Down
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Near
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Far

            return shadowTransforms;
        }
        public static Matrix4 GetSpotLightMatrix(Light spotLight, float nearPlane, float farPlane)
        {
            float fov = (float)Math.Acos(spotLight.OuterCutOff) * 2.0f;

            Matrix4 shadowProjection = Matrix4.CreatePerspectiveFieldOfView(fov, 1.0f, nearPlane, farPlane);

            Vector3 lightPos = spotLight.transform.position;
            Vector3 target = lightPos + spotLight.transform.Forward;

            Matrix4 shadowView = Matrix4.LookAt(lightPos, target, Vector3.UnitY);

            return shadowView * shadowProjection;
        }
        #endregion
        #endregion
        #region Shadow Mapping
        #region Initialize Maps
        public static void InitializeShadowMaps()
        {
            InitDirectionalShadowMap();
            InitPointShadowMap();
            InitSpotShadowMap();
        }
        public static void InitDirectionalShadowMap()
        {
            //Initialize the framebuffer and texture for the shadow map
            depthMapFBO = GL.GenFramebuffer();
            depthMap = GL.GenTexture();
            depthShader = new Shader("depth.vert", "depth.frag");
            depthShader.Compile();

            //Set texture parameters
            GL.BindTexture(TextureTarget.Texture2D, depthMap);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f,
                          SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            //Set framebuffer parameters
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            //Restore default Framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public static void InitPointShadowMap()
        {
            //Initialize the framebuffer and texture for the shadow map
            depthCubemapFBO = GL.GenFramebuffer();
            depthCubemap = GL.GenTexture();
            pointShadowShader = new Shader("point_depth.vert", "point_depth.frag", "point_depth.geom");
            pointShadowShader.Compile();

            //Set textureCubemapArray parameters
            GL.BindTexture(TextureTarget.TextureCubeMapArray, depthCubemap);

            GL.TexImage3D(TextureTarget.TextureCubeMapArray, 0, PixelInternalFormat.DepthComponent32f,
                          SHADOW_CUBE_RESOLUTION, SHADOW_CUBE_RESOLUTION, MAX_POINT_LIGHTS * 6,
                          0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            //Set framebuffer parameters
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthCubemapFBO);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubemap, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            //Restore Default Framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public static void InitSpotShadowMap()
        {
            //Initialize the framebuffer and texture for the shadow map
            spotDepthMapFBO = GL.GenFramebuffer();
            spotDepthMapArray = GL.GenTexture();

            //Set texture2DArray parameters
            GL.BindTexture(TextureTarget.Texture2DArray, spotDepthMapArray);

            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.DepthComponent32f,
                          SHADOW_WIDTH, SHADOW_WIDTH, MAX_SPOT_LIGHTS,
                          0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBorderColor, borderColor);

            //Set framebuffer parameters
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, spotDepthMapFBO);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, spotDepthMapArray, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            //Restore Default Framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        #endregion
        public static void RenderScene()
        {
            //First Pass: Directional Shadows
            Light? dirLight = GetActiveDirectionalLight();
            if (dirLight != null)
            {
                Matrix4 lightSpaceMatrix = GetDirectionalLightMatrix(dirLight);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
                GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                //Use depth shader
                depthShader.Use();
                depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

                RenderDepths(depthShader);
            }

            //Extra Pass for each Point Light: Point Shadows
            List<Light> pointLights = GetActivePointLights();
            if (pointLights.Count > 0)
            {
                //Prepare Cubemap
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthCubemapFBO);
                GL.Viewport(0, 0, SHADOW_CUBE_RESOLUTION, SHADOW_CUBE_RESOLUTION);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                //Use depth shader with geometry
                pointShadowShader.Use();

                //Get Each Cubemap Face Transforms and Set them in Shader
                //For each light, calculate 6 shadow matrices and render the scene depth cubemap from the light's perspective
                int lightCount = Math.Min(pointLights.Count, MAX_POINT_LIGHTS);

                
                for (int lightIndex = 0; lightIndex < lightCount; lightIndex++)
                {
                    Light pLight = pointLights[lightIndex];


                    List<Matrix4> shadowTransforms = GetPointLightMatrices(pLight, near_plane_point, far_plane_point);

                    for (int i = 0; i < 6; ++i)
                    {
                        pointShadowShader.SetMatrix4($"shadowMatrices[{i}]", shadowTransforms[i]);
                    }

                    pointShadowShader.SetVector3("lightPos", pLight.transform.position);
                    pointShadowShader.SetFloat("far_plane", far_plane_point);
                    pointShadowShader.SetInt("lightIndex", lightIndex);

                    //Render Cubemap
                    RenderDepths(pointShadowShader);
                }
                
            }

            //Extra Pass for each Spot Light: Spot Shadows
            List<Light> spotLights = GetActiveSpotLights();
            if (spotLights.Count > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, spotDepthMapFBO);
                GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_WIDTH);

                //Use depth shader
                depthShader.Use();

                int spotCount = Math.Min(spotLights.Count, MAX_SPOT_LIGHTS);
                for (int i = 0; i < spotCount; i++)
                {
                    Matrix4 lightSpaceMat = GetSpotLightMatrix(spotLights[i], near_plane_spot, far_plane_spot);
                    depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMat);

                    GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, spotDepthMapArray, 0, i);

                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    RenderDepths(depthShader);
                }
            }

            //Final Pass: Scene
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, ScreenWidth, ScreenHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Set depthMap as Texture Unit 3
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, depthMap);

            //Set depthCubemap array as Texture Unit 4
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.TextureCubeMapArray, depthCubemap);

            //Set spotDepthMap array as Texture Unit 5
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2DArray, spotDepthMapArray);

            //Render Scene with Shadows
            foreach (var renderer in _renderers)
            {
                Shader? shader = renderer.GetShader();
                if (shader != null)
                {
                    shader.Use();
                    shader.SetMatrix4("lightSpaceMatrix", GetDirectionalLightMatrix(dirLight));

                    for(int i = 0; i< spotLights.Count; i++)
                    {
                        Matrix4 spotSpaceMat = GetSpotLightMatrix(spotLights[i], near_plane_spot, far_plane_spot);
                        shader.SetMatrix4($"spotLightSpaceMatrices[{i}]", spotSpaceMat);
                    }

                    shader.SetInt("shadowMap", 3);
                    shader.SetInt("shadowCubemap", 4);
                    shader.SetInt("shadowSpotMapArray", 5);
                    shader.SetFloat("far_plane", 25.0f);
                }

                renderer.Render();
            }
        }
        #endregion
    }
}
