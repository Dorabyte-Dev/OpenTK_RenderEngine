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
        private Color4 _clearColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

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

        #endregion
        #region LifeCycle Functions

        protected override void OnLoad() //Se ejecuta cuando carga el programa por primera vez
        {
            base.OnLoad();

            _mainScene = new Scene("Main Scene");

            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            GL.ClearColor(_clearColor); // El color de fondo oscuro de tu antigua escena
            GL.Enable(EnableCap.DepthTest);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Shader unlitShader = new Shader("basic.vert", "basic.frag");
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            Texture texSpecular = Texture.LoadFromFile("Textures/container2_specular.png");

            Material cubeMaterial = new Material(litShader);
            cubeMaterial.SetTexture("material.diffuse", texDiffuse);
            cubeMaterial.SetTexture("material.specular", texSpecular);
            cubeMaterial.SetFloat("material.shininess", 32.0f);

            // 3. Generar los 10 Cubos en bucle
            /*for (int i = 0; i < _cubePositions.Length; i++)
            {
                GameObject cube = new GameObject($"Cube_{i}");
                cube.transform.position = _cubePositions[i];
                cube.transform.scale = _cubeScale;

                Renderer rend = cube.AddComponent<Renderer>();
                rend.SetMaterial(cubeMaterial);
                // Usamos tu FBX genérico en lugar de los vértices a mano
                rend.LoadModel("Models/Primitives/Cube.fbx");
            }*/

            // 4. Crear la Luz Direccional (Aproximando tu vector -0.2, -1.0, -0.3)
            GameObject dirLightObj = new GameObject("Directional Light");
            dirLightObj.transform.rotation = new Vector3(-70f, -30f, 0f);

            Light dirLight = new Light("DirectionalLight", LightType.Directional);
            dirLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
            dirLight.Diffuse = new Vector3(0.4f, 0.4f, 0.4f);
            dirLight.Specular = new Vector3(0.5f, 0.5f, 0.5f);

            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                Light pLight = new Light($"PointLight_{i}", LightType.Point);
                pLight.transform.position = _pointLightPositions[i];
                pLight.transform.scale = new Vector3(.1f);
                Renderer lightRend = pLight.AddComponent<Renderer>();
                lightRend.SetMaterial(new Material(unlitShader, Color4.White));
                lightRend.LoadModel("Models/Primitives/Sphere.fbx");

                pLight.Type = LightType.Point;
                pLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
                pLight.Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
                pLight.Specular = new Vector3(1.0f, 1.0f, 1.0f);

                // Tus valores exactos de atenuación
                pLight.Constant = 1.0f;
                pLight.Linear = 0.14f;
                pLight.Quadratic = 0.07f;
            }

            GameObject fallingCube = new GameObject("Falling Cube");
            Renderer fallingRend = fallingCube.AddComponent<Renderer>();
            fallingRend.SetMaterial(cubeMaterial);
            fallingRend.LoadModel("Models/Primitives/Cube.fbx");

            Collider col = fallingCube.AddComponent<Collider>();

            // (El Rigidbody se auto-añadirá al PhysicsManager cuando la escena llame a Start)
            Rigidbody rb = fallingCube.AddComponent<Rigidbody>();
            rb.Mass = 1.0f;

            PhysicsManager.InitializePhysics();
            _mainScene.Start();
           
        }

        protected override void OnRenderFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame visual del juego. Aquí va todo lo que se dibuja.
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderManager.Render();

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) //Se ejecuta cada vez que el FrameBuffer se redimensiona
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
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



            PhysicsManager.StepSimulation((float)Time.deltaTime);



            _mainScene.Update((float)e.Time);
            _mainScene.FixedUpdate((float)e.Time);
            _mainScene.LateUpdate((float)e.Time);

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
    }
}

