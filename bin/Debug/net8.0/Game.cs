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
        private Color4 _clearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);

        // ----- RENDER PIPELINE -----
        Renderer rend;

        protected override void OnLoad() //Se ejecuta cuando carga el programa por primera vez
        {
            base.OnLoad();

            GameObject cube = new GameObject("Cube");
            cube.transform.position = new Vector3(1, 1, 1);
            Shader shader = new Shader("basic.vert", "basic.frag");
            Texture tex = Texture.LoadFromFile("Textures/awesomeface.png");
            Material mat = new Material(shader, Color4.Blue);
            rend = cube.AddComponent<Renderer>();
            rend.LoadModel("Models/Oso.fbx");
            rend.SetMaterial(mat);

            Console.WriteLine(cube.DebugComponents());

            GL.ClearColor(_clearColor);
        }

        protected override void OnRenderFrame(FrameEventArgs e) //Se ejecuta al renderizar cada frame visual del juego. Aquí va todo lo que se dibuja.
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

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

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}

