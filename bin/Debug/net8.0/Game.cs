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
        private Color4 _clearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);

        // ----- RENDER PIPELINE -----
        Renderer rend;

        // ----- MOVEMENT -----
        private float speed = 1.5f;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private float sensitivity = 0.2f;

        protected override void OnLoad() //Se ejecuta cuando carga el programa por primera vez
        {
            base.OnLoad();


            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            GameObject cube = new GameObject("Cube");
            cube.transform.position = new Vector3(0, 0, 0);
            Shader shader = new Shader("phong.vert", "phong.frag");
            Texture tex = Texture.LoadFromFile("Textures/awesomeface.png");
            Material mat = new Material(shader);
            mat.SetTexture("material.diffuse", tex);
            mat.SetFloat("material.shininess", 32.0f); // 32 es un valor estándar
            rend = cube.AddComponent<Renderer>();
            rend.SetMaterial(mat);
            rend.LoadModel("Models/Primitives/Cube.fbx");

            GameObject lightObj = new GameObject("Directional Light");
            // Apuntamos la luz hacia abajo y un poco hacia adelante
            lightObj.transform.rotation = new Vector3(-45f, 45f, 0);

            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.Type = LightType.Directional;
            // Asegúrate de que los colores de la luz no sean negros
            dirLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
            dirLight.Diffuse = new Vector3(0.4f, 0.4f, 0.4f);
            dirLight.Specular = new Vector3(0.5f, 0.5f, 0.5f);

            // Crear una Point Light
            GameObject pointLightObj = new GameObject("Point Light 1");
            // La ponemos un poco a la derecha, arriba y hacia nosotros
            pointLightObj.transform.position = new Vector3(-1.0f, -1.0f, -2.0f);

            Light pLight = pointLightObj.AddComponent<Light>();
            pLight.Type = LightType.Point;

            // Le damos un color llamativo (ej. rojo o azul) para notar bien la diferencia con la direccional
            pLight.Ambient = new Vector3(0.05f, 0.0f, 0.0f);
            pLight.Diffuse = new Vector3(1.0f, 0.2f, 0.2f); // Luz roja
            pLight.Specular = new Vector3(1.0f, 0.5f, 0.5f);


            //Console.WriteLine(cube.DebugComponents());

            GL.ClearColor(_clearColor);
            GL.Enable(EnableCap.DepthTest);
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

