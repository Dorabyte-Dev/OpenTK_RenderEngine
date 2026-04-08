using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

using BulletSharp;
using BulletSharp.Math;

using Assimp;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using System.Reflection;

namespace GameEngine
{
    public class Game : GameWindow
    {
        #region Variables
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
        {

        }
        // ----- CAMERA -----
        private Camera _camera;
        private Color4 _clearColor = new Color4(0.0f, 0.0f, 0.3f, 1.0f);

        // ----- MOVEMENT -----
        private float speed = 1.5f;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private float sensitivity = 0.2f;

        // ---- PHYSICS -----
        private double _physicsAccumulator = 0.0;
        private const double FixedTimeStep = 1.0 / 60.0;

        // ----- SCENE -----
        private Scene _mainScene;
        private readonly Vector3 _cubeScale = new Vector3(1.0f, 1.0f, 1.0f);
        private readonly Vector3[] _cubePositions =
        {
            // Fila Z = 0
            new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(2f, 0f, 0f), new Vector3(3f, 0f, 0f), new Vector3(4f, 0f, 0f), new Vector3(5f, 0f, 0f),
    
            // Fila Z = 1
            new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 1f), new Vector3(3f, 0f, 1f), new Vector3(4f, 0f, 1f), new Vector3(5f, 0f, 1f),
    
            // Fila Z = 2
            new Vector3(0f, 0f, 2f), new Vector3(1f, 0f, 2f), new Vector3(2f, 0f, 2f), new Vector3(3f, 0f, 2f), new Vector3(4f, 0f, 2f), new Vector3(5f, 0f, 2f),
    
            // Fila Z = 3
            new Vector3(0f, 0f, 3f), new Vector3(1f, 0f, 3f), new Vector3(2f, 0f, 3f), new Vector3(3f, 0f, 3f), new Vector3(4f, 0f, 3f), new Vector3(5f, 0f, 3f),
    
            // Fila Z = 4
            new Vector3(0f, 0f, 4f), new Vector3(1f, 0f, 4f), new Vector3(2f, 0f, 4f), new Vector3(3f, 0f, 4f), new Vector3(4f, 0f, 4f), new Vector3(5f, 0f, 4f),
    
            // Fila Z = 5
            new Vector3(0f, 0f, 5f), new Vector3(1f, 0f, 5f), new Vector3(2f, 0f, 5f), new Vector3(3f, 0f, 5f), new Vector3(4f, 0f, 5f), new Vector3(5f, 0f, 5f),
        };

        private readonly Vector3[] _pointLightPositions =
                {
            new Vector3(1.7f, 0.2f, 5.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };

        GameObject fallingCube;
        Light dirLight;
        Material planeMaterial;
        private bool differenceState;
        private bool differenceState2;

        // ----- SHADERS -----
        Shader litShader;

        #endregion
        #region LifeCycle Functions
        protected override void OnLoad()
        {
            base.OnLoad();

            _mainScene = new Scene("Main Scene");
            _camera = new Camera(new Vector3(0, 0, 3.5f), Size.X / (float)Size.Y); // Cámara un poco más atrás
            GL.ClearColor(_clearColor);
            GL.Enable(EnableCap.DepthTest);

            RenderManager.InitializeShadowMaps();

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Shader unlitShader = new Shader("basic.vert", "basic.frag");
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            Texture texSpecular = Texture.LoadFromFile("Textures/container2_specular.png");

            Material cubeMaterial = new Material(litShader);
            cubeMaterial.SetTexture("material.diffuse", texDiffuse);
            cubeMaterial.SetTexture("material.specular", texSpecular);
            cubeMaterial.SetFloat("material.shininess", 32.0f);

            // ----- 1. EL SUELO (AABB Estático) -----
            GameObject floor = new GameObject("Floor");
            floor.transform.position = new Vector3(0.0f, -5.0f, -5.0f);
            floor.transform.scale = new Vector3(15.0f, 1.0f, 15.0f);

            Renderer floorRend = floor.AddComponent<Renderer>();
            floorRend.SetMaterial(cubeMaterial);
            floorRend.LoadModel("Models/Primitives/Cube.fbx");
            floor.AddComponent<Collider>();

            Rigidbody floorRb = floor.AddComponent<Rigidbody>();
            floorRb.Mass = 0.0f; // Estático
            floorRb.Friction = 0.8f;


            // ----- 2. LA RAMPA (OBB Estática) -----
            // Demuestra colisión orientada contra un objeto inamovible
            GameObject ramp = new GameObject("Ramp");
            ramp.transform.position = new Vector3(0.0f, 0.0f, -5.0f);
            ramp.transform.scale = new Vector3(8.0f, 0.5f, 4.0f);

            // ¡GIRAMOS LA RAMPA 30 GRADOS EN EL EJE X!
            ramp.transform.rotation = new Vector3(30.0f, 0.0f, 0.0f);

            Renderer rampRend = ramp.AddComponent<Renderer>();
            rampRend.SetMaterial(cubeMaterial);
            rampRend.LoadModel("Models/Primitives/Cube.fbx");
            ramp.AddComponent<Collider>();

            Rigidbody rampRb = ramp.AddComponent<Rigidbody>();
            rampRb.Mass = 0.0f; // Estática
            rampRb.Bounciness = 0.5f; // Un poco de rebote para que el cubo que cae no se quede pegado a la rampa



            // ----- 3. EL CUBO QUE CAE (OBB Dinámica) -----
            // Va a caer sobre la rampa y luego al suelo, dando vueltas
            GameObject tumblingCube = new GameObject("Tumbling Cube");
            tumblingCube.transform.position = new Vector3(0.0f, 8.0f, -5.0f);

            // ¡Lo giramos de forma aleatoria para que caiga de esquina!
            tumblingCube.transform.rotation = new Vector3(45.0f, 45.0f, 0.0f);

            Renderer cubeRend = tumblingCube.AddComponent<Renderer>();
            cubeRend.SetMaterial(cubeMaterial);
            cubeRend.LoadModel("Models/Primitives/Cube.fbx");
            tumblingCube.AddComponent<Collider>();

            Rigidbody cubeRb = tumblingCube.AddComponent<Rigidbody>();
            cubeRb.Mass = 1.0f;       // Dinámico
            cubeRb.Bounciness = 100f; // Un poco de rebote


            Light directionalLight = new Light("Directional Light", LightType.Directional);
            directionalLight.transform.rotation = new Vector3(-45.0f, 30.0f, 0.0f);
            directionalLight.Diffuse = new Vector3(1.8f, 1.8f, 1.8f);

            Time.timeScale = 0.5f;
            PhysicsManager.InitializePhysics();
            _mainScene.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame visual del juego. Aquí va todo lo que se dibuja.
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderManager.RenderScene();

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) //Se ejecuta cada vez que el FrameBuffer se redimensiona
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            RenderManager.ScreenWidth = e.Width;
            RenderManager.ScreenHeight = e.Height;
        }

        
        protected override void OnUpdateFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame lógico del juego. Aquí va todo lo que es jugable.
        {
            base.OnUpdateFrame(e);

            Time.Update();

            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }
            CursorState = CursorState.Grabbed;

            //dirLight.transform.rotation = new Vector3(0.0f, 0, 0.0f); // Hacemos girar la luz para ver cómo se actualizan las sombras en tiempo real

            PhysicsManager.StepSimulation((float)Time.deltaTime);



            _mainScene.Update((float)e.Time);
            _mainScene.FixedUpdate((float)e.Time);
            _mainScene.LateUpdate((float)e.Time);


            if (KeyboardState.IsKeyDown(Keys.R) && fallingCube != null)
            {
                Rigidbody rb = fallingCube.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.SetVelocity(new Vector3(-5.0f, 0.0f, 0.0f));
                }
            }

            if (KeyboardState.IsKeyPressed(Keys.G))
            {
                DifferenceComparer();
            }

            if(KeyboardState.IsKeyPressed(Keys.H))
            {
                DifferenceComparer2();
            }

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


            float delta = (float)e.Time;
            if (KeyboardState.IsKeyDown(Keys.W)) _camera.Position += _camera.Front * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.S)) _camera.Position -= _camera.Front * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.A)) _camera.Position -= _camera.Right * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.D)) _camera.Position += _camera.Right * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.Q) || KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.Position -= _camera.Up * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.E) || KeyboardState.IsKeyDown(Keys.Space)) _camera.Position += _camera.Up * speed * delta;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsFocused) // check to see if the window is focused  
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
                    _camera.Yaw += deltaX * sensitivity;
                    _camera.Pitch -= deltaY * sensitivity;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            // Ajustar el FOV usando la rueda del ratón
            _camera.Fov -= (float)e.OffsetY;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            PhysicsManager.CleanUp();
        }
        #endregion
        #endregion
        void DifferenceComparer()
        {
            if (differenceState)
            {
                planeMaterial.SetInt("isNotBlinn", 1);
                /*planeMaterial.SetInt("gammaCorrection", 1);
                Texture.temp_sRGB = true;*/
            }
            else
            {
                planeMaterial.SetInt("isNotBlinn", 0);
                /*planeMaterial.SetInt("gammaCorrection", 0);
                Texture.temp_sRGB = false;*/
            }
            differenceState = !differenceState;
        }
        void DifferenceComparer2()
        {
            if (differenceState2)
            {
                planeMaterial.SetInt("material.isNotBlinn", 1);
            }
            else
            {
                planeMaterial.SetInt("material.isNotBlinn", 0);
            }
            differenceState2 = !differenceState2;
        }
    }
}

