using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
namespace GameEngine
{
    public class Game : GameWindow
    {
        #region Variables
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
        {
            
        }

        public static Game instance;
        
        private Color4 _clearColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

        // ----- CAMERA MOVEMENT -----
        private float speed = 1.5f;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private float sensitivity = 0.2f;

        // ---- PHYSICS -----
        private double _physicsAccumulator = 0.0;
        private const double FixedTimeStep = 1.0 / 60.0;

        // ----- DEBUG -----
        private bool differenceState;
        private bool differenceState2;

        #endregion
        #region LifeCycle Functions
        protected override void OnLoad()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                return;
            }

            base.OnLoad();

            GL.ClearColor(_clearColor);
            GL.Enable(EnableCap.DepthTest);

            RenderManager.InitializeShadowMaps();
            PhysicsManager.InitializePhysics();

            LoadBaseArchitectureScene();
        }

        private void LoadBaseArchitectureScene()
        {
            Scene architectureScene = new Scene("Demostracion_Arquitectura");

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Texture floorTex = Texture.LoadFromFile("Textures/brickwall.jpg");
            Texture modelTex = Texture.LoadFromFile("Textures/TexturasMago.png");

            GameObject floor = new GameObject("Suelo_Tecnico")
            .transform.SetPosition(new Vector3(0, -2f, 0))
            .SetScale(new Vector3(15, 1, 15)).owner
            .AddComponent<Renderer>()
            .LoadModel("Models/Primitives/Cube.fbx")
            .SetMaterial(new Material(litShader).SetTexture("material.diffuse", floorTex)).owner;

            GameObject heroModel = new GameObject("Modelo_Importado")
            .transform.SetPosition(new Vector3(0, 0, 0))
            .SetRotation(new Vector3(-90, 0, 0))
            .SetScale(new Vector3(2, 2, 2)).owner
            .AddComponent<Renderer>()
            .LoadModel("Models/Castle.fbx")
            .SetMaterial(new Material(litShader)
              .SetTexture("material.diffuse", modelTex)
              .SetFloat("material.shininess", 64.0f)).owner;

            GameObject camera = new GameObject("MainCamera")
            .transform.SetPosition(new Vector3(10, 10, 10)).owner
            .AddComponent<Camera>()
            .SetFov(60f)
            .SetAspectRatio(Size.X / (float)Size.Y).owner;
            
            GameObject sunObj = new GameObject("Luz_Estudio")
              .transform.SetPosition(new Vector3(5, 5, 5))
              .SetRotation(new Vector3(-45, 45, 0))
              .owner
              .AddComponent<Light>()
              .SetType(LightType.Directional)
              .SetDiffuse(new Vector3(1.2f, 1.2f, 1.2f))
              .SetAmbient(new Vector3(0.2f, 0.2f, 0.2f))
              .owner;

            architectureScene.Start();
        }

        private void LoadLightingLabScene()
        {
            Scene lightingScene = new Scene("Laboratorio_Iluminacion_Claro");
            SceneManager.LoadScene(lightingScene);

            lightingScene.clearColor = _clearColor;

            Shader litShader = new Shader("phong.vert", "phong.frag");

            Texture floorTex = Texture.LoadFromFile("Textures/brickwall.jpg");
            Texture whiteTex = Texture.LoadFromFile("Textures/WhiteTex.jpg");
            Texture sphereTex = Texture.LoadFromFile("Textures/plastic_floor.png");
            Texture metalTex = Texture.LoadFromFile("Textures/metal_floor.jpg");
            
            GameObject floor = new GameObject("Suelo_Estudio")
                .transform.SetPosition(new Vector3(0, -1.0f, 0))
                .SetScale(new Vector3(20, 1, 20)).owner
                .AddComponent<Renderer>()
                .LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", floorTex)
                    .SetFloat("material.shininess", 16.0f)).owner;

            GameObject sphereMatte = new GameObject("Esfera_Mate")
                .transform.SetPosition(new Vector3(-4, 1.5f, 0))
                .SetScale(new Vector3(1.5f, 1.5f, 1.5f)).owner
                .AddComponent<Renderer>()
                .LoadModel("Models/Primitives/Sphere.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", whiteTex)
                    .SetFloat("material.shininess", 8.0f)).owner;

            GameObject spherePlastic = new GameObject("Esfera_Plastico")
                .transform.SetPosition(new Vector3(0, 1.5f, 0))
                .SetScale(new Vector3(1.5f, 1.5f, 1.5f)).owner
                .AddComponent<Renderer>()
                .LoadModel("Models/Primitives/Sphere.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", sphereTex)
                    .SetFloat("material.shininess", 64.0f)).owner; 
         
            GameObject sphereMetal = new GameObject("Esfera_Metal")
                .transform.SetPosition(new Vector3(4, 1.5f, 0))
                .SetScale(new Vector3(1.5f, 1.5f, 1.5f)).owner
                .AddComponent<Renderer>()
                .LoadModel("Models/Primitives/Sphere.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", metalTex)
                    .SetFloat("material.shininess", 256.0f)).owner; 
           
            GameObject camera = new GameObject("MainCamera")
                .transform.SetPosition(new Vector3(0, 4, 12))
                .SetRotation(new Vector3(-10, 0, 0)).owner 
                .AddComponent<Camera>()
                .SetFov(60f)
                .SetAspectRatio(Size.X / (float)Size.Y).owner;
            
            GameObject dirLight = new GameObject("Luz_Relleno")
                .transform.SetRotation(new Vector3(-45, -45, 0)).owner
                .AddComponent<Light>()
                .SetType(LightType.Directional)
                .SetDiffuse(new Vector3(0.4f, 0.4f, 0.4f)) 
                .SetAmbient(new Vector3(0.2f, 0.2f, 0.2f))
                .SetSpecular(new Vector3(0.3f, 0.3f, 0.3f)).owner;

            
            GameObject pointLightRed = new GameObject("Puntual_Roja")
                .transform.SetPosition(new Vector3(-4, 3f, 3f)).owner
                .AddComponent<Light>()
                .SetType(LightType.Point)
                .SetDiffuse(new Vector3(3f, 0.4f, 0.4f)) 
                .SetAmbient(new Vector3(0.1f, 0.0f, 0.0f))
                .SetSpecular(new Vector3(1.0f, 0.2f, 0.2f))
                .SetDistance(40.0f).owner; 

            
            GameObject pointLightCyan = new GameObject("Puntual_Cyan")
                .transform.SetPosition(new Vector3(4, 3f, 3f)).owner
                .AddComponent<Light>()
                .SetType(LightType.Point)
                .SetDiffuse(new Vector3(0.2f, 1.5f, 1.5f)) 
                .SetAmbient(new Vector3(0.0f, 0.1f, 0.1f))
                .SetSpecular(new Vector3(0.2f, 1.0f, 1.0f))
                .SetDistance(40.0f).owner;

            
            GameObject spotLight = new GameObject("Linterna_Central")
                .transform.SetPosition(new Vector3(0, 6f, 4f))
                .SetRotation(new Vector3(-35, 0, 0)).owner 
                .AddComponent<Light>()
                .SetType(LightType.Spot)
                .SetDiffuse(new Vector3(2.0f, 2.0f, 2.0f)) 
                .SetAmbient(new Vector3(0.0f, 0.0f, 0.0f))
                .SetSpecular(new Vector3(2.0f, 2.0f, 2.0f))
                .SetCutOff(15.0f)
                .SetOuterCutOff(20.0f)
                .SetDistance(30.0f).owner;

            lightingScene.Start();
        }

        private void LoadPhysicsPlaygroundScene()
        {
            Scene physicsScene = new Scene("Playground_Fisicas");

            
            SceneManager.LoadScene(physicsScene);
            physicsScene.clearColor = new Color4(0.5f, 0.7f, 0.9f, 1.0f);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Texture gridTex = Texture.LoadFromFile("Textures/container2.png");
            Texture ballTex = Texture.LoadFromFile("Textures/awesomeface.png");

            
            GameObject floor = new GameObject("Suelo_Fisico")
                .transform.SetPosition(new Vector3(0, -1, 0))
                .SetScale(new Vector3(30, 1, 30)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", gridTex)).owner
                .AddComponent<Collider>()
                .SetBox(new Vector3(0.5f, 0.5f, 0.5f))
                .owner 
                .AddComponent<Rigidbody>()
                .SetMass(0.0f) 
                .SetFriction(0.5f)
                .SetBounciness(0.1f).owner;

            
            GameObject ramp = new GameObject("Rampa")
                .transform.SetPosition(new Vector3(-10, 4, 0))
                .SetRotation(new Vector3(0, 0, -25)) 
                .SetScale(new Vector3(10, 1, 5)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", gridTex)).owner
                .AddComponent<Collider>()
                .SetBox(new Vector3(0.5f,0.5f,0.5f)).owner
                .AddComponent<Rigidbody>().SetMass(0.0f).SetFriction(0.1f).owner; 


            for (int y = 0; y < 5; y++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    GameObject cajaNueva = new GameObject($"Caja_Muro_{x}_{y}")
                        .transform.SetPosition(new Vector3(5 + x * 2.1f, 0.5f + y * 2.1f, 0))
                        .SetScale(new Vector3(2, 2, 2)).owner
                        .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                        .SetMaterial(new Material(litShader).SetTexture("material.diffuse", gridTex)).owner
                        .AddComponent<Collider>()
                        .SetBox(new Vector3(0.5f, 0.5f, 0.5f))
                        .owner
                        .AddComponent<Rigidbody>()
                        .SetMass(1.0f) 
                        .SetFriction(0.8f).owner;

                }
            }

            for (int i = 0; i < 3; i++)
            {
                GameObject esferaNueva = new GameObject($"Bola_Boliche_{i}")
                    .transform.SetPosition(new Vector3(-12 + i * 3, 12 + i * 5, 0)) 
                    .SetScale(new Vector3(2, 2, 2)).owner
                    .AddComponent<Renderer>().LoadModel("Models/Primitives/Sphere.fbx")
                    .SetMaterial(new Material(litShader).SetTexture("material.diffuse", ballTex)).owner
                    .AddComponent<Collider>()
                    .SetSphere(1)
                    .owner 
                    .AddComponent<Rigidbody>()
                    .SetMass(5.0f) 
                    .SetBounciness(0.8f) 
                    .SetFriction(0.2f).owner;
            }

            GameObject camera = new GameObject("MainCamera")
                .transform.SetPosition(new Vector3(0, 10, 25))
                .SetRotation(new Vector3(-15, 0, 0)).owner
                .AddComponent<Camera>().SetFov(60f).SetAspectRatio(Size.X / (float)Size.Y).owner;

            GameObject sun = new GameObject("Sol")
                .transform.SetPosition(new Vector3(10, 20, 10))
                .SetRotation(new Vector3(-45, -45, 0)).owner
                .AddComponent<Light>().SetType(LightType.Directional)
                .SetDiffuse(new Vector3(1.0f, 1.0f, 1.0f))
                .SetAmbient(new Vector3(0.3f, 0.3f, 0.3f)).owner; 

            physicsScene.Start();
        }

        private void LoadShadowMappingScene()
        {
            Scene shadowScene = new Scene("Demostracion_Sombras_Avanzada");
            SceneManager.LoadScene(shadowScene);

            shadowScene.clearColor = new Color4(0.75f, 0.45f, 0.25f, 1.0f);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Texture floorTex = Texture.LoadFromFile("Textures/brickwall.jpg");
            Texture heroTex = Texture.LoadFromFile("Textures/TexturasMago.png");
            Texture boxTex = Texture.LoadFromFile("Textures/container2.png");
            Texture whiteTex = Texture.LoadFromFile("Textures/WhiteTex.jpg"); 

            GameObject floor = new GameObject("Suelo_Sombras")
                .transform.SetPosition(new Vector3(0, -0.5f, 0)) 
                .SetScale(new Vector3(50, 1, 50)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", floorTex)
                    .SetFloat("material.shininess", 16.0f)).owner;

            
            GameObject hero = new GameObject("Personaje_Central")
                .transform.SetPosition(new Vector3(0, 1.25f, 0)) 
                .SetRotation(new Vector3(-90, 0, 0))
                .SetScale(new Vector3(2.5f, 2.5f, 2.5f)).owner
                .AddComponent<Renderer>().LoadModel("Models/Castle.fbx") 
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", heroTex)
                    .SetFloat("material.shininess", 32.0f)).owner;

            GameObject Dir_Pilar = new GameObject("Dir_Pilar_Izquierdo")
                .transform.SetPosition(new Vector3(-6, 3, 2))
                .SetScale(new Vector3(1, 6, 1)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", boxTex)).owner;

            GameObject Dir_Pilar_D = new GameObject("Dir_Pilar_Derecho")
                .transform.SetPosition(new Vector3(6, 4, -2))
                .SetScale(new Vector3(1, 8, 1)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", boxTex)).owner;

            GameObject Dir_Caja = new GameObject("Dir_Caja_Flotante")
                .transform.SetPosition(new Vector3(0, 6, 4))
                .SetRotation(new Vector3(45, 45, 0))
                .SetScale(new Vector3(1.5f, 1.5f, 1.5f)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", boxTex)).owner;

            GameObject sun = new GameObject("Sol_Atardecer")
                .transform.SetPosition(new Vector3(20, 30, 20))
                .SetRotation(new Vector3(-50, 45, 0)).owner
                .AddComponent<Light>().SetType(LightType.Directional)
                .SetDiffuse(new Vector3(1.5f, 1.2f, 0.9f)) 
                .SetAmbient(new Vector3(0.05f, 0.05f, 0.05f)) 
                .SetSpecular(new Vector3(1.0f, 0.9f, 0.8f)).owner;



            float pointZoneX = -18f;
            float pointZoneZ = -5f;

            
            GameObject Point_Pared_F = new GameObject("Point_Pared_Fondo")
                .transform.SetPosition(new Vector3(pointZoneX, 4, pointZoneZ - 5))
                .SetScale(new Vector3(12, 8, 1)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", floorTex)).owner;


            GameObject Point_Pared_L = new GameObject("Point_Pared_Lateral")
                .transform.SetPosition(new Vector3(pointZoneX - 5.5f, 4, pointZoneZ))
                .SetScale(new Vector3(1, 8, 10)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", floorTex)).owner;

            
            GameObject Point_Pared_Derecha = new GameObject("Point_Pared_Derecha")
                .transform.SetPosition(new Vector3(pointZoneX + 5.5f, 4, pointZoneZ))
                .SetScale(new Vector3(1, 8, 10)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", floorTex)).owner;


            
            GameObject Point_Caja = new GameObject("Point_Caja_Central")
                .transform.SetPosition(new Vector3(pointZoneX, 2f, pointZoneZ))
                .SetRotation(new Vector3(0, 45, 0))
                .SetScale(new Vector3(2, 4, 2)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", boxTex)).owner;

            
            GameObject Point_Esfera = new GameObject("Point_Esfera_Flotante")
                .transform.SetPosition(new Vector3(pointZoneX + 3, 5f, pointZoneZ - 2))
                .SetScale(new Vector3(1.5f, 1.5f, 1.5f)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Sphere.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", whiteTex)).owner;

            
            GameObject pointLight = new GameObject("Luz_Puntual_Magica")
                .transform.SetPosition(new Vector3(pointZoneX - 2f, 4f, pointZoneZ + 2f)).owner
                .AddComponent<Light>().SetType(LightType.Point)
                .SetDiffuse(new Vector3(0.4f, 3.0f, 5.0f)) 
                .SetAmbient(new Vector3(0.0f, 0.05f, 0.1f))
                .SetSpecular(new Vector3(0.5f, 1.0f, 1.0f))
                .SetDistance(40.0f).owner;


            float spotZoneX = 18f;
            float spotZoneZ = 0f;

            GameObject Spot_Pantalla = new GameObject("Spot_Pantalla")
                .transform.SetPosition(new Vector3(spotZoneX, 5, spotZoneZ - 6))
                .SetScale(new Vector3(14, 10, 1)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", whiteTex)).owner;

            GameObject Spot_Estatua =  new GameObject("Spot_Estatua")
                .transform.SetPosition(new Vector3(spotZoneX, 2f, spotZoneZ - 2))
                .SetRotation(new Vector3(0, 0, 0))
                .SetScale(new Vector3(2f, 4f, 2f)).owner
                .AddComponent<Renderer>().LoadModel("Models/Primitives/Cube.fbx")
                .SetMaterial(new Material(litShader).SetTexture("material.diffuse", boxTex)).owner;

            GameObject spotLight = new GameObject("Foco_Teatro")
                .transform.SetPosition(new Vector3(spotZoneX, 7f, spotZoneZ + 5f))
                .SetRotation(new Vector3(-20, 0, 0)).owner
                .AddComponent<Light>().SetType(LightType.Spot)
                .SetDiffuse(new Vector3(3.0f, 3.0f, 3.0f))
                .SetAmbient(new Vector3(0.0f, 0.0f, 0.0f))
                .SetSpecular(new Vector3(2.0f, 2.0f, 2.0f))
                .SetCutOff(12.5f)
                .SetOuterCutOff(17.5f)
                .SetDistance(50.0f).owner;


            GameObject camera = new GameObject("MainCamera")
                .transform.SetPosition(new Vector3(0, 8, 25))
                .SetRotation(new Vector3(-10, 0, 0)).owner
                .AddComponent<Camera>().SetFov(60f).SetAspectRatio(Size.X / (float)Size.Y).owner;

            shadowScene.Start();
        }

        private void LoadGrandFinaleScene()
        {
            Scene finaleScene = new Scene("Gran_Final_StressTest");
            SceneManager.LoadScene(finaleScene);

            finaleScene.clearColor = new Color4(0.15f, 0.15f, 0.2f, 1.0f);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Texture castleTex = Texture.LoadFromFile("Textures/castle_diff.png");
            Texture boxTex = Texture.LoadFromFile("Textures/container2.png");
            Texture ballTex = Texture.LoadFromFile("Textures/awesomeface.png");

            GameObject castle = new GameObject("Castillo_Fisico")
                .transform.SetPosition(new Vector3(0, -2f, 0))
                .SetRotation(new Vector3(-90, 0, 0))
                .SetScale(new Vector3(3f, 3f, 3f)).owner
                .AddComponent<Renderer>().LoadModel("Models/RealCastle.fbx")
                .SetMaterial(new Material(litShader)
                    .SetTexture("material.diffuse", castleTex)
                    .SetFloat("material.shininess", 16.0f)).owner
                .AddComponent<Collider>()
                .SetConcaveMesh("Models/RealCastle.fbx").owner
                .AddComponent<Rigidbody>()
                .SetMass(0.0f) 
                .SetFriction(0.6f).owner;

            Random rnd = new Random();
            for (int i = 0; i < 30; i++)
            {
                float xOffset = (float)(rnd.NextDouble() * 10 - 5);
                float zOffset = (float)(rnd.NextDouble() * 10 - 5);
                float yHeight = 15f + (i * 2f);

                bool isSphere = i % 2 == 0;

                GameObject prop = new GameObject($"Prop_Dinamico_{i}")
                    .transform.SetPosition(new Vector3(xOffset, yHeight, zOffset))
                    .SetScale(new Vector3(1f, 1f, 1f)).owner
                    .AddComponent<Renderer>().LoadModel(isSphere ? "Models/Primitives/Sphere.fbx" : "Models/Primitives/Cube.fbx")
                    .SetMaterial(new Material(litShader).SetTexture("material.diffuse", isSphere ? ballTex : boxTex)).owner
                    .AddComponent<Collider>().owner;

                if (isSphere) prop.GetComponent<Collider>().SetSphere(0.5f);
                else prop.GetComponent<Collider>().SetBox(new Vector3(0.5f, 0.5f, 0.5f));

                prop.AddComponent<Rigidbody>()
                    .SetMass(isSphere ? 3.0f : 1.5f)
                    .SetBounciness(isSphere ? 0.7f : 0.1f)
                    .SetFriction(0.5f);
            }

            GameObject moonLight = new GameObject("Luna_Direccional")
                .transform.SetPosition(new Vector3(-20, 30, -20))
                .SetRotation(new Vector3(-45, -135, 0)).owner
                .AddComponent<Light>().SetType(LightType.Directional)
                .SetDiffuse(new Vector3(1.5f, 1.8f, 2.5f))
                .SetAmbient(new Vector3(0.15f, 0.15f, 0.25f))
                .SetSpecular(new Vector3(2.0f, 2.0f, 2.5f)).owner;

            GameObject fireLight = new GameObject("Fuego_Central")
                .transform.SetPosition(new Vector3(0, 2f, 0)).owner
                .AddComponent<Light>().SetType(LightType.Point)
                .SetDiffuse(new Vector3(8.0f, 4.0f, 0.5f))
                .SetAmbient(new Vector3(0.2f, 0.1f, 0.0f))
                .SetSpecular(new Vector3(4.0f, 2.0f, 0.2f))
                .SetDistance(45.0f).owner;

            GameObject explorerSpot = new GameObject("Foco_Explorador")
                .transform.SetPosition(new Vector3(15, 15, 15))
                .SetRotation(new Vector3(-35, 45, 0)).owner
                .AddComponent<Light>().SetType(LightType.Spot)
                .SetDiffuse(new Vector3(6.0f, 6.0f, 6.0f))
                .SetAmbient(new Vector3(0.0f, 0.0f, 0.0f))
                .SetSpecular(new Vector3(5.0f, 5.0f, 5.0f))
                .SetCutOff(10.0f)
                .SetOuterCutOff(15.0f)
                .SetDistance(70.0f).owner;

            GameObject camera = new GameObject("MainCamera")
                .transform.SetPosition(new Vector3(0, 12, 30))
                .SetRotation(new Vector3(-15, 0, 0)).owner
                .AddComponent<Camera>().SetFov(60f).SetAspectRatio(Size.X / (float)Size.Y).owner;

            finaleScene.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e) //It's executed once for each visual frame rendered by the engine. Here goes anything related to drawing, post-processing and visual effects.
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderManager.RenderScene();

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) //It's executed each time the Framebuffer resizes, which usually happens when the window is resized. Here we adjust the viewport and any related settings to ensure the rendering scales correctly with the new window size.
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            RenderManager.ScreenWidth = e.Width;
            RenderManager.ScreenHeight = e.Height;
        }

        
        protected override void OnUpdateFrame(FrameEventArgs e) //It's executed when rendering each logic tick of the engine. Here goes anything related to inputs, physics and movement.
        {
            base.OnUpdateFrame(e);

            Time.Update();

            if (!IsFocused)
            {
                return;
            }
            if(Time.timeScale != 0)
            {
                CursorState = CursorState.Grabbed;
            }
            else
            {
                CursorState = CursorState.Normal;
            }

            _physicsAccumulator += Time.deltaTime;

            while (_physicsAccumulator >= FixedTimeStep)
            {
                PhysicsManager.StepSimulation((float)FixedTimeStep);

                SceneManager.CurrentScene.FixedUpdate((float)FixedTimeStep);

                _physicsAccumulator -= FixedTimeStep;
            }

            PhysicsManager.StepSimulation((float)Time.deltaTime);

            SceneManager.CurrentScene.Update((float)e.Time);
            SceneManager.CurrentScene.LateUpdate((float)e.Time);

            HandleInputs(e);
        }

        #region Inputs
        private void HandleInputs(FrameEventArgs e)
        {
            KeyboardState input = KeyboardState;
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if(Time.timeScale != 0)
            {
                if (KeyboardState.IsKeyPressed(Keys.G))
                {
                    DifferenceComparer();
                }

                if (KeyboardState.IsKeyPressed(Keys.H))
                {
                    DifferenceComparer2();
                }

                float delta = (float)e.Time;
                if (KeyboardState.IsKeyDown(Keys.W)) Camera.Main.Position += Camera.Main.Front * speed * delta;
                if (KeyboardState.IsKeyDown(Keys.S)) Camera.Main.Position -= Camera.Main.Front * speed * delta;
                if (KeyboardState.IsKeyDown(Keys.A)) Camera.Main.Position -= Camera.Main.Right * speed * delta;
                if (KeyboardState.IsKeyDown(Keys.D)) Camera.Main.Position += Camera.Main.Right * speed * delta;
                if (KeyboardState.IsKeyDown(Keys.Q)) Camera.Main.Position -= Camera.Main.Up * speed * delta;
                if (KeyboardState.IsKeyDown(Keys.E)) Camera.Main.Position += Camera.Main.Up * speed * delta;
            }

            Helper.Inputs(input, CursorState);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsFocused && Time.timeScale != 0) // check to see if the window is focused  
            {
                var mouse = MouseState;
                if (_firstMove)
                {
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    _firstMove = false;
                }
                else
                {
                    var deltaX = mouse.X - _lastPos.X;
                    var deltaY = mouse.Y - _lastPos.Y;
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    if (Camera.Main != null)
                    {
                        Camera.Main.Yaw += deltaX * sensitivity;
                        Camera.Main.Pitch -= deltaY * sensitivity;
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if(Camera.Main != null) Camera.Main.Fov -= (float)e.OffsetY;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            PhysicsManager.CleanUp();
        }
        #endregion
        #endregion
        void DifferenceComparer() //Useful for comparing two states of the engine, for example, to see the difference between two rendering techniques or physics settings.
                                  //You can toggle between them with a key press and observe the changes in real-time.
        {
            if (differenceState)
            {

            }
            else
            {

            }
            differenceState = !differenceState;
        }
        void DifferenceComparer2()
        {
            if (differenceState2)
            {

            }
            else
            {

            }
            differenceState2 = !differenceState2;
        }

        public void ResetMouseFirstMove()
        {
            _firstMove = true;
        }
    }
}

