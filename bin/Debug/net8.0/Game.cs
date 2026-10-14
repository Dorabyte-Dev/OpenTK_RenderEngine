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

        // ----- CAMERA MOVEMENT -----
        private float speed = 3f;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private float sensitivity = 0.2f;

        // ---- PHYSICS -----
        private double _physicsAccumulator = 0.0;
        private const double FixedTimeStep = 1.0 / 60.0;

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

            RenderManager.Initialize();
            PhysicsManager.InitializePhysics();

            LoadShadowMappingScene();
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

            /*sunObj = new GameObject("Sol_Atardecer")
                .transform.SetPosition(new Vector3(20, 30, 20))
                .SetRotation(new Vector3(-50, 45, 0)).owner
                .AddComponent<Light>().SetType(LightType.Directional)
                .SetDiffuse(new Vector3(1.5f, 1.2f, 0.9f))
                .SetAmbient(new Vector3(0.05f, 0.05f, 0.05f))
                .SetSpecular(new Vector3(1.0f, 0.9f, 0.8f)).owner;*/



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

            GameObject Spot_Estatua = new GameObject("Spot_Estatua")
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

        protected override void OnRenderFrame(FrameEventArgs e) //It's executed once for each visual frame rendered by the engine. Here goes anything related to drawing, post-processing and visual effects.
        {
            base.OnRenderFrame(e);

            RenderManager.RenderScene();
            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) //It's executed each time the Framebuffer resizes, which usually happens when the window is resized. Here we adjust the viewport and any related settings to ensure the rendering scales correctly with the new window size.
        {
            base.OnFramebufferResize(e);
            RenderManager.Resize(e.Width, e.Height);
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

            //sunObj.transform.rotation = new Vector3(sunObj.transform.rotation.X, sunObj.transform.rotation.Y + 10f * Time.deltaTime, sunObj.transform.rotation.Z);
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

        public void ResetMouseFirstMove()
        {
            _firstMove = true;
        }
    }
}

