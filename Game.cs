using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

using Assimp;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using System.Reflection;

namespace GameEngine
{

    //A GameObject should be instanced with
    //  -Default Empty Transform (or position, if needed)
    //  -A renderer if the gameObject requires to render a model
    //      -With a Material that tells the renderer how to render it
    //          -With a Shader inside the Material
    //          -With a Texture or Base Color, or none
    public class Game : GameWindow
    {
        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
        {

        }
        // ----- CAMERA -----
        private Camera _camera;
        private Color4 _clearColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

        // ----- RENDER PIPELINE -----
        Renderer rend;

        // ----- MOVEMENT -----
        private float speed = 1.5f;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private float sensitivity = 0.2f;


        // ----- SCENE -----
        private readonly Vector3 _cubeScale = new Vector3(1.0f, 1.0f, 1.0f);
        private readonly Vector3[] _cubePositions =
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3(2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f, 3.0f, -7.5f),
            new Vector3(1.3f, -2.0f, -2.5f),
            new Vector3(1.5f, 2.0f, -2.5f),
            new Vector3(1.5f, 0.2f, -1.5f),
            new Vector3(-1.3f, 1.0f, -1.5f)
        };

        private readonly Vector3[] _pointLightPositions =
                {
            new Vector3(0.7f, 0.2f, 2.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };

        protected override void OnLoad() //Se ejecuta cuando carga el programa por primera vez
        {
            base.OnLoad();

            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            GL.ClearColor(_clearColor); // El color de fondo oscuro de tu antigua escena
            GL.Enable(EnableCap.DepthTest);

            Shader litShader = new Shader("phong.vert", "phong.frag");
            Texture texDiffuse = Texture.LoadFromFile("Textures/container2.png");
            Texture texSpecular = Texture.LoadFromFile("Textures/container2_specular.png");

            Material cubeMaterial = new Material(litShader);
            cubeMaterial.SetTexture("material.diffuse", texDiffuse);
            cubeMaterial.SetTexture("material.specular", texSpecular);
            cubeMaterial.SetFloat("material.shininess", 32.0f);

            // 3. Generar los 10 Cubos en bucle
            for (int i = 0; i < _cubePositions.Length; i++)
            {
                GameObject cube = new GameObject($"Cube_{i}");
                cube.transform.position = _cubePositions[i];
                cube.transform.scale = _cubeScale;
                // En tu código viejo rotabas en base a un eje y un ángulo. 
                // Esta es una aproximación en ángulos de Euler para darles ese aspecto caótico:
                cube.transform.rotation = new Vector3(20.0f * i, 6.0f * i, 10.0f * i);

                Renderer rend = cube.AddComponent<Renderer>();
                rend.SetMaterial(cubeMaterial);
                // Usamos tu FBX genérico en lugar de los vértices a mano
                rend.LoadModel("Models/Primitives/Cube.fbx");
            }

            // 4. Crear la Luz Direccional (Aproximando tu vector -0.2, -1.0, -0.3)
            GameObject dirLightObj = new GameObject("Directional Light");
            dirLightObj.transform.rotation = new Vector3(-70f, -30f, 0f);

            Light dirLight = dirLightObj.AddComponent<Light>();
            dirLight.Type = LightType.Directional;
            dirLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
            dirLight.Diffuse = new Vector3(0.4f, 0.4f, 0.4f);
            dirLight.Specular = new Vector3(0.5f, 0.5f, 0.5f);

            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                GameObject plObj = new GameObject($"PointLight_{i}");
                plObj.transform.position = _pointLightPositions[i];

                Light pLight = plObj.AddComponent<Light>();
                pLight.Type = LightType.Point;
                pLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
                pLight.Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
                pLight.Specular = new Vector3(1.0f, 1.0f, 1.0f);

                // Tus valores exactos de atenuación
                pLight.Constant = 1.0f;
                pLight.Linear = 0.14f;
                pLight.Quadratic = 0.07f;
            }
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

        protected override void OnUpdateFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame lógico del juego. Aquí va todo lo que es jugable.
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }
            CursorState = CursorState.Grabbed;

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
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.Position -= _camera.Up * speed * delta;
            if (KeyboardState.IsKeyDown(Keys.Space)) _camera.Position += _camera.Up * speed * delta;
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
        }
    }
}

