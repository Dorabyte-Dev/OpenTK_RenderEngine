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
using Assimp.Unmanaged;

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

            GL.ClearColor(_clearColor);
            GL.Enable(EnableCap.DepthTest);

            RenderManager.InitializeShadowMaps(); // Asegúrate de que esta función inicia Direccional, Point y Spot
    

            // Arrancamos el juego cargando directamente la Escena 1
            LoadScene1_CubeAndLight();
            
        }

        // ==========================================
        // CONFIGURACIÓN 1: Un cubo, un plano y el sol
        // ==========================================
        private void LoadScene1_CubeAndLight()
        {
            Scene newScene = new Scene("Escena_Basica");

            SceneManager.LoadScene(newScene);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Material basicMat = new Material(litShader);
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            basicMat.SetTexture("material.diffuse", texDiffuse);

            // 1. Suelo
            GameObject floor = new GameObject("Floor");
            floor.transform.position = new Vector3(0.0f, -2.0f, 0.0f);
            floor.transform.rotation = new Vector3(15f, 0, 0f);
            floor.transform.scale = new Vector3(10.0f, 1.0f, 10.0f);
            Renderer floorRend = floor.AddComponent<Renderer>();
            floorRend.SetMaterial(basicMat);
            floorRend.LoadModel("Models/Primitives/Cube.fbx");
            floor.AddComponent<Collider>();
            Rigidbody floorRb = floor.AddComponent<Rigidbody>();
            floorRb.Mass = 0.0f;

            // 2. Cubo central flotando
            GameObject cube = new GameObject("CenterCube");
            cube.AddComponent<Collider>();
            cube.transform.position = new Vector3(0.0f, 1.0f, -3.0f);
            cube.transform.rotation = new Vector3(0, 0, 0f);
            Renderer cubeRend = cube.AddComponent<Renderer>();
            cubeRend.SetMaterial(basicMat);
            cubeRend.LoadModel("Models/Primitives/Cube.fbx");
            
            Rigidbody cubeRb = cube.AddComponent<Rigidbody>();
            cubeRb.Mass = 1.0f;

            // 3. Luz Direccional
            GameObject sunObj = new GameObject("SunLight");
            sunObj.transform.position = new Vector3(5.0f, 10.0f, 5.0f);
            sunObj.transform.rotation = new Vector3(-45.0f, -45.0f, 0.0f);
            Light sunLight = sunObj.AddComponent<Light>();
            sunLight.Type = LightType.Directional;
            sunLight.Diffuse = new Vector3(2.0f, 2.0f, 2.0f);

            // 4. Cámara
            GameObject camObj = new GameObject("MainCamera");
            camObj.transform.position = new Vector3(0, 2, 5);
            _camera = camObj.AddComponent<Camera>();
            _camera.AspectRatio = Size.X / (float)Size.Y;

            PhysicsManager.InitializePhysics();

            newScene.Start();
        }

        // ==========================================
        // CONFIGURACIÓN 2: Prisión de sombras rojas
        // ==========================================
        private void LoadScene2_RedPointLight()
        {
            Scene newScene = new Scene("Escena_Roja");

            SceneManager.LoadScene(newScene);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Material basicMat = new Material(litShader);
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            basicMat.SetTexture("material.diffuse", texDiffuse);

                    // 1. Suelo
                    GameObject floor = new GameObject("Floor");
            floor.transform.position = new Vector3(0.0f, -2.0f, 0.0f);
            floor.transform.scale = new Vector3(10.0f, 1.0f, 10.0f);
            Renderer floorRend = floor.AddComponent<Renderer>();
            floorRend.SetMaterial(basicMat);
            floorRend.LoadModel("Models/Primitives/Cube.fbx");

            // 2. Dos Pilares
            GameObject p1 = new GameObject("Pilar1");
            p1.transform.position = new Vector3(-2f, 0f, -3f);
            p1.transform.scale = new Vector3(0.5f, 3f, 0.5f);
            Renderer p1Rend = p1.AddComponent<Renderer>();
            p1Rend.SetMaterial(basicMat);
            p1Rend.LoadModel("Models/Primitives/Cube.fbx");

            GameObject p2 = new GameObject("Pilar2");
            p2.transform.position = new Vector3(2f, 0f, -3f);
            p2.transform.scale = new Vector3(0.5f, 3f, 0.5f);
            Renderer p2Rend = p2.AddComponent<Renderer>();
            p2Rend.SetMaterial(basicMat);
            p2Rend.LoadModel("Models/Primitives/Cube.fbx");

            // 3. Luz Puntual Roja en medio de los pilares
            GameObject pointObj = new GameObject("RedLight");
            pointObj.transform.position = new Vector3(0.0f, 0.0f, -3.0f);
            Light pLight = pointObj.AddComponent<Light>();
            pLight.Type = LightType.Point;
            pLight.Diffuse = new Vector3(25.0f, 0.0f, 0.0f); // Rojo intenso HDR
    
            // (Opcional: puedes ponerle el Sol aquí también muy tenue para no tener negros puros)

            // 4. Cámara
            GameObject camObj = new GameObject("MainCamera");
            camObj.transform.position = new Vector3(0, 1, 3);
            _camera = camObj.AddComponent<Camera>();
            _camera.AspectRatio = Size.X / (float)Size.Y;

            PhysicsManager.InitializePhysics();

            newScene.Start();
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

            if (!IsFocused)
            {
                return;
            }
            CursorState = CursorState.Grabbed;


            PhysicsManager.StepSimulation((float)Time.deltaTime);


            SceneManager.CurrentScene.Update((float)e.Time);
            SceneManager.CurrentScene.FixedUpdate((float)e.Time);
            SceneManager.CurrentScene.LateUpdate((float)e.Time);


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

            /*if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyPressed(Keys.S))
            {
                SceneSerializer.SaveScene(SceneManager.CurrentScene, SceneManager.CurrentScene.Name + ".json");
            }

            if (KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyPressed(Keys.L))
            {
                SceneSerializer.LoadScene("Escena_Basica.json");
            }*/

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
            if (KeyboardState.IsKeyDown(Keys.W)) Camera.Main.Position += Camera.Main.Front * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.S)) Camera.Main.Position -= Camera.Main.Front * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.A)) Camera.Main.Position -= Camera.Main.Right * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.D)) Camera.Main.Position += Camera.Main.Right * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.Q) || KeyboardState.IsKeyDown(Keys.LeftShift)) Camera.Main.Position -= Camera.Main.Up * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.E) || KeyboardState.IsKeyDown(Keys.Space)) Camera.Main.Position += Camera.Main.Up * speed * delta;

            Helper.Inputs(input);
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

            // Ajustar el FOV usando la rueda del ratón
            if(Camera.Main != null) Camera.Main.Fov -= (float)e.OffsetY;
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
                LoadScene1_CubeAndLight();
            }
            else
            {
                LoadScene2_RedPointLight();
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

