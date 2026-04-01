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

            // ¡CRÍTICO! Inicializar el Framebuffer de sombras
            RenderManager.InitShadowMap();
            RenderManager.InitPointShadowMap();

            // Shaders
            litShader = new Shader("phong.vert", "phong.frag");
            Shader unlitShader = new Shader("basic.vert", "basic.frag");

            // Texturas y Material
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            Texture texSpecular = texDiffuse;

            planeMaterial = new Material(litShader);
            planeMaterial.SetTexture("material.diffuse", texDiffuse);
            planeMaterial.SetTexture("material.specular", texSpecular);
            planeMaterial.SetFloat("material.shininess", 32.0f);
            planeMaterial.SetInt("material.isNotBlinn", 1);


            // 1. LA HABITACIÓN (6 Planos formando un cubo gigante de 10x10x10)
            // Asumiendo que tu Plane.fbx con rotación (-90,0,0) se vuelve un suelo horizontal.
            Vector3[] roomRotations = {
    new Vector3(-90f, 0f, 0f),  // Suelo
    new Vector3(90f, 0f, 0f),   // Techo
    new Vector3(0f, 0f, 0f),    // Pared Fondo (Asumiendo que el plano mira a +Z)
    new Vector3(0f, 180f, 0f),  // Pared Frente
    new Vector3(0f, 90f, 0f),   // Pared Izquierda
    new Vector3(0f, -90f, 0f)   // Pared Derecha
};

            Vector3[] roomPositions = {
    new Vector3(0f, -5f, 0f),   // Suelo
    new Vector3(0f, 5f, 0f),    // Techo
    new Vector3(0f, 0f, -5f),   // Pared Fondo
    new Vector3(0f, 0f, 5f),    // Pared Frente
    new Vector3(-5f, 0f, 0f),   // Pared Izquierda
    new Vector3(5f, 0f, 0f)     // Pared Derecha
};

            for (int i = 0; i < 6; i++)
            {
                GameObject wall = new GameObject($"Wall_{i}");
                wall.transform.position = roomPositions[i];
                wall.transform.rotation = roomRotations[i];
                wall.transform.scale = new Vector3(10f); // Escala para que cierren las esquinas
                Renderer wallRend = wall.AddComponent<Renderer>();
                wallRend.SetMaterial(planeMaterial);
                wallRend.LoadModel("Models/Primitives/Plane.fbx");
            }

            // 2. OBJETOS FLOTANTES (Para proyectar sombras en todas las direcciones)
            Vector3[] cubePositions = {
                new Vector3(2.0f, -1.5f, 1.0f),    // Abajo a la derecha
                new Vector3(-2.0f, 1.5f, -1.5f),   // Arriba a la izquierda, atrás
                new Vector3(0.0f, 2.5f, 1.5f),     // Casi en el techo
                new Vector3(-1.5f, -2.5f, 2.0f)    // Cerca del suelo
            };

            Vector3[] cubeRotations = {
                new Vector3(15f, 30f, 45f),
                new Vector3(60f, 0f, -30f),
                new Vector3(0f, 45f, 0f),
                new Vector3(-20f, -45f, 60f)
            };

            for (int i = 0; i < cubePositions.Length; i++)
            {
                GameObject cube = new GameObject($"Cube_{i}");
                cube.transform.position = cubePositions[i];
                cube.transform.rotation = cubeRotations[i];
                cube.transform.scale = new Vector3(1.5f); // Cubos medianos
                Renderer cubeRend = cube.AddComponent<Renderer>();
                cubeRend.SetMaterial(planeMaterial);
                cubeRend.LoadModel("Models/Primitives/Cube.fbx");
            }

            // 3. LA LUZ PUNTUAL (En el corazón de la habitación)
            Light pointLight = new Light("CenterPointLight", LightType.Point);
            pointLight.transform.position = new Vector3(0.0f, 0.0f, 0.0f); // Justo en el centro
            pointLight.Ambient = new Vector3(0.05f); // Penumbra suave en las esquinas

            // HDR: Usamos multiplicadores altísimos para contrarrestar la atenuación física real
            pointLight.Diffuse = new Vector3(70.0f, 70.0f, 70.0f);
            pointLight.Specular = new Vector3(20.0f, 20.0f, 20.0f);

            // Atenuación física real (Ley de la Inversa del Cuadrado)
            pointLight.Constant = 1.0f;
            pointLight.Linear = 0.0f;
            pointLight.Quadratic = 0.5f; // Hará que la luz llegue bien a las paredes a 5 metros de distancia

            // El cubito visual blanco para saber de dónde sale la luz
            Renderer lightVisual = pointLight.AddComponent<Renderer>();
            lightVisual.SetMaterial(new Material(unlitShader, Color4.White));
            lightVisual.LoadModel("Models/Primitives/Sphere.fbx");
            pointLight.transform.scale = new Vector3(0.2f);

            PhysicsManager.InitializePhysics();
            _mainScene.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame visual del juego. Aquí va todo lo que se dibuja.
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Light? dirLight = RenderManager.GetActiveDirectionalLight();
            RenderManager.RenderScene(_mainScene, _camera, dirLight, litShader);

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) //Se ejecuta cada vez que el FrameBuffer se redimensiona
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            RenderManager.ScreenWidth = e.Width;
            RenderManager.ScreenHeight = e.Height;
        }

        #region Inputs
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

