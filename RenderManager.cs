using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace GameEngine
{
    public static class RenderManager
    {
        public static int ScreenWidth { get; set; } = 800;
        public static int ScreenHeight { get; set; } = 600;

        private static List<Renderer> _renderers = new();
        private static List<Light> _lights = new();

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

        public static Light? GetActiveDirectionalLight() => _lights.FirstOrDefault(l => l.Type == LightType.Directional);

        public static List<Light> GetActivePointLights()
        {
            return _lights.Where(l => l.Type == LightType.Point).ToList();
        }

        public static List<Light> GetActiveSpotLights()
        {
            return _lights.Where(l => l.Type == LightType.Spot).ToList();
        }

        public static int depthMapFBO;
        public static int depthMap;
        public static Shader depthShader;
        public const int SHADOW_WIDTH = 2048;
        public const int SHADOW_HEIGHT = 2048;


        public static void InitShadowMap()
        {
            depthMapFBO = GL.GenFramebuffer();

            depthMap = GL.GenTexture();
            depthShader = new Shader("depth.vert", "depth.frag");
            GL.BindTexture(TextureTarget.Texture2D, depthMap);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f,
                          SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        // Variables para ajustar el "volumen" de tus sombras. 
        // Si la caja es muy grande, las sombras se verán pixeladas. 
        // Si es muy pequeña, los objetos en los bordes no tendrán sombra.
        private static float near_plane = 1.0f;
        private static float far_plane = 50.0f;

        public static Matrix4 GetLightSpaceMatrix(Light dirLight)
        {
            // 1. Proyección Ortográfica (Izquierda, Derecha, Abajo, Arriba, Cerca, Lejos)
            // Ajusta estos valores de -10 y 10 dependiendo del tamaño de tu escena
            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-50.0f, 50.0f, -50.0f, 50.0f, near_plane, far_plane);

            // 2. Matriz de Vista (Desde dónde mira, Hacia dónde mira, Vector Arriba)
            // Usamos la posición real de tu Directional Light y su vector Forward
            Vector3 lightPos = dirLight.transform.position;
            Vector3 target = lightPos + dirLight.transform.Forward;
            Vector3 up = Vector3.UnitY; // Asumimos que Y es arriba

            Matrix4 lightView = Matrix4.LookAt(lightPos, target, up);

            // 3. Multiplicamos para obtener la lightSpaceMatrix final
            // (En OpenTK el orden suele ser View * Projection)
            Matrix4 lightSpaceMatrix = lightView * lightProjection;

            return lightSpaceMatrix;
        }

        
        public static void RenderScene(Scene scene, Camera camera, Light dirLight, Shader shader)
        {
            if (dirLight != null)
            {
                // --- PASO 0: PREPARATIVOS ---
                Matrix4 lightSpaceMatrix = GetLightSpaceMatrix(dirLight);

                // Shader especial para profundidad que creamos en el mensaje anterior


                // --- PASADA 1: RENDERIZAR MAPA DE SOMBRAS (Desde la luz) ---

                // 1. Atamos el FBO de la sombra
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);

                // 2. Ajustamos el Viewport al tamaño de la textura de sombra (ej. 1024x1024)
                GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);

                // 3. Limpiamos SOLO el buffer de profundidad
                GL.Clear(ClearBufferMask.DepthBufferBit);

                // 4. Usamos el shader de profundidad y le pasamos la matriz
                depthShader.Use();
                depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

                // 5. ¡DIBUJAMOS TODOS LOS MODELOS! (Pero usando depthShader)
                RenderDepths(depthShader);
            }

            // --- PASADA EXTRA: POINT LIGHTS ---
            List<Light> pointLights = GetActivePointLights();
            if (pointLights.Count > 0)
            {
                Light pLight = pointLights[0]; // Cogemos la primera para probar

                // CUIDADO: El far_plane para point lights suele ser menor que el direccional
                float near_plane_point = 1.0f;
                float far_plane_point = 25.0f;

                List<Matrix4> shadowTransforms = GetPointLightMatrices(pLight, near_plane_point, far_plane_point);

                // 1. Preparamos el Framebuffer del Cubemap
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthCubemapFBO);
                GL.Viewport(0, 0, SHADOW_CUBE_RESOLUTION, SHADOW_CUBE_RESOLUTION);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                // 2. Usamos el nuevo shader con Geometry Shader
                pointShadowShader.Use();

                // 3. Enviamos las 6 matrices al array de matrices en el Geometry Shader
                for (int i = 0; i < 6; ++i)
                {
                    pointShadowShader.SetMatrix4($"shadowMatrices[{i}]", shadowTransforms[i]);
                    // NOTA: Asegúrate de que el nombre coincida con tu .geom (yo lo llamé shadowMatrices ahí, si usaste shadowMatrices, pon "shadowMatrices[" + i + "]")
                }

                // 4. Enviamos la posición y el far_plane al Fragment Shader
                pointShadowShader.SetVector3("lightPos", pLight.transform.position);
                pointShadowShader.SetFloat("far_plane", far_plane_point);

                // 5. Dibujamos la escena entera. 
                // El Geometry Shader se encargará de multiplicar los modelos x 6
                RenderDepths(pointShadowShader); // Ojo: necesitas modificar RenderDepths para que acepte un parámetro si antes lo tenías 'hardcodeado' a depthShader
            }

            // --- PASADA 2: RENDERIZAR ESCENA NORMAL (Desde la cámara) ---

            // 1. Volvemos al Framebuffer por defecto (la pantalla)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // 2. Restauramos el Viewport al tamaño de tu ventana original
            GL.Viewport(0, 0, ScreenWidth, ScreenHeight);

            // 3. Limpiamos color y profundidad normalmente
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, depthMap);

            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.TextureCubeMap, depthCubemap);

            foreach (var renderer in _renderers)
            {
                if (renderer.GetMaterial() != null && renderer.GetMaterial().shader != null)
                {
                    renderer.GetMaterial().shader.Use();
                    Matrix4 safeLightSpaceMatrix = dirLight != null ? GetLightSpaceMatrix(dirLight) : Matrix4.Identity;

                    // ¡Le decimos al shader que la variable shadowMap está en la unidad 3!
                    renderer.GetMaterial().shader.SetInt("shadowMap", 3);      // <---

                    renderer.GetMaterial().shader.SetInt("shadowCubemap", 4);
                    renderer.GetMaterial().shader.SetFloat("far_plane", 25.0f);
                }

                renderer.Render();
            }
        }

        // Point Shadows
        public static int depthCubemapFBO;
        public static int depthCubemap;
        public static Shader pointShadowShader;
        public const int SHADOW_CUBE_RESOLUTION = 1024;

        public static void InitPointShadowMap()
        {
            // 1. Crear el Framebuffer
            depthCubemapFBO = GL.GenFramebuffer();

            // 2. Crear la textura (esta vez es un Cubemap)
            depthCubemap = GL.GenTexture();
            pointShadowShader = new Shader("point_depth.vert", "point_depth.geom", "point_depth.frag");
            GL.BindTexture(TextureTarget.TextureCubeMap, depthCubemap);

            // 3. Asignar memoria para las 6 caras del cubo
            for (int i = 0; i < 6; i++)
            {
                // TextureCubeMapPositiveX + i itera por las 6 constantes de las caras de OpenGL (+X, -X, +Y, -Y, +Z, -Z)
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent,
                              SHADOW_CUBE_RESOLUTION, SHADOW_CUBE_RESOLUTION, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }

            // 4. Parámetros de la textura
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge); // ¡CRÍTICO! WrapR es la 3ª dimensión (Z)

            // 5. Atar el cubemap al Framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthCubemapFBO);

            // Fíjate que usamos FramebufferTexture en lugar de FramebufferTexture2D para atar el cubo entero de golpe
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubemap, 0);

            // 6. Decirle a OpenGL que no vamos a dibujar colores
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            // Restaurar el framebuffer por defecto
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public static List<Matrix4> GetPointLightMatrices(Light pointLight, float nearPlane, float farPlane)
        {
            List<Matrix4> shadowTransforms = new List<Matrix4>();
            Vector3 lightPos = pointLight.transform.position;

            // Proyección en Perspectiva (FOV 90º, Aspect Ratio 1.0 (cuadrado), Near, Far)
            Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, nearPlane, farPlane);

            // Las 6 direcciones del cubo. Fíjate en los vectores 'Up' (el tercer parámetro). 
            // Son específicos para que el Cubemap de OpenGL cuadre perfectamente.
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Derecha
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Izquierda
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)) * shadowProj); // Arriba
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)) * shadowProj); // Abajo
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Cerca
            shadowTransforms.Add(Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj); // Lejos

            return shadowTransforms;
        }
    }
}
