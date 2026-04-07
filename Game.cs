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

            // Shaders
            litShader = new Shader("phong.vert", "phong.frag");
            Shader unlitShader = new Shader("basic.vert", "basic.frag");

            // --- 1. MATERIALES DE LABORATORIO (Grises y neutros) ---
            Texture texDiffuse = Texture.LoadFromFile("Textures/brickwall.jpg"); // Úsala como base, o una textura blanca/gris si tienes
            Texture texSpecular = texDiffuse;

            Material labMaterial = new Material(litShader);
            labMaterial.SetTexture("material.diffuse", texDiffuse);
            labMaterial.SetTexture("material.specular", texSpecular);
            labMaterial.SetFloat("material.shininess", 16.0f);
            labMaterial.SetInt("material.isNotBlinn", 1);

            Material lightBulbMaterial = new Material(unlitShader, Color4.White);


            // --- 2. LA CÁMARA DE PRUEBAS (Suelo y 3 Paredes) ---
            // Suelo
            GameObject floor = new GameObject("Floor");
            floor.transform.position = new Vector3(0f, -2f, 0f);
            floor.transform.rotation = new Vector3(-90f, 0f, 0f);
            floor.transform.scale = new Vector3(15f, 15f, 1f);
            Renderer floorRend = floor.AddComponent<Renderer>();
            floorRend.SetMaterial(labMaterial);
            floorRend.LoadModel("Models/Primitives/Plane.fbx");

            // Pared Fondo
            GameObject wallBack = new GameObject("WallBack");
            wallBack.transform.position = new Vector3(0f, 3f, -10f);
            wallBack.transform.rotation = new Vector3(0f, 0f, 0f);
            wallBack.transform.scale = new Vector3(15f, 10f, 1f);
            Renderer wallBackRend = wallBack.AddComponent<Renderer>();
            wallBackRend.SetMaterial(labMaterial);
            wallBackRend.LoadModel("Models/Primitives/Cube.fbx");

            // Pared Izquierda
            GameObject wallLeft = new GameObject("WallLeft");
            wallLeft.transform.position = new Vector3(-10f, 3f, 0f);
            wallLeft.transform.rotation = new Vector3(0f, 90f, 0f);
            wallLeft.transform.scale = new Vector3(15f, 10f, 1f);
            Renderer wallLeftRend = wallLeft.AddComponent<Renderer>();
            wallLeftRend.SetMaterial(labMaterial);
            wallLeftRend.LoadModel("Models/Primitives/Cube.fbx");


            // --- 3. OBJETOS DE PRUEBA (Props) ---
            // Prop 1: Pilar central (Para ver la sombra direccional claramente)
            GameObject pillar = new GameObject("Pillar");
            pillar.transform.position = new Vector3(0f, 0f, 0f);
            pillar.transform.scale = new Vector3(1f, 4f, 1f);
            Renderer pillarRend = pillar.AddComponent<Renderer>();
            pillarRend.SetMaterial(labMaterial);
            pillarRend.LoadModel("Models/Primitives/Cube.fbx");

            // Props 2-5: Prisión para la Point Light (Para comprobar las 6 caras del Cubemap)
            Vector3[] cagePositions = {
    new Vector3(-6.0f, -1.0f, -4.0f), // Izquierda
    new Vector3(-4.0f, -1.0f, -4.0f), // Derecha
    new Vector3(-5.0f, -1.0f, -5.0f), // Atrás
    new Vector3(-5.0f, -1.0f, -3.0f)  // Adelante
};
            for (int i = 0; i < 4; i++)
            {
                GameObject cageCube = new GameObject($"CageCube_{i}");
                cageCube.transform.position = cagePositions[i];
                cageCube.transform.scale = new Vector3(0.5f, 2f, 0.5f);
                Renderer cageRend = cageCube.AddComponent<Renderer>();
                cageRend.SetMaterial(labMaterial);
                cageRend.LoadModel("Models/Primitives/Cube.fbx");
            }

            // Prop 6: Cubo flotante en la pared (Para comprobar la Spot Light)
            GameObject spotTarget = new GameObject("SpotTarget");
            spotTarget.transform.position = new Vector3(5f, 2f, -8f);
            spotTarget.transform.scale = new Vector3(1.5f, 1.5f, 0.2f); // Plano y pegado a la pared
            Renderer targetRend = spotTarget.AddComponent<Renderer>();
            targetRend.SetMaterial(labMaterial);
            targetRend.LoadModel("Models/Primitives/Cube.fbx");


            // --- 4. LUZ DIRECCIONAL (Gris neutro) ---
            Light dirLight = new Light("DirLight", LightType.Directional);
            dirLight.transform.position = new Vector3(10.0f, 20.0f, 10.0f);
            dirLight.transform.rotation = new Vector3(-60f, -45f, 0f); // Apunta desde arriba-derecha
            dirLight.Ambient = new Vector3(0.2f, 0.2f, 0.25f);

            // 4. Multiplicamos la fuerza de la luz por 5 para que compita con el HDR
            dirLight.Diffuse = new Vector3(3.0f, 3.0f, 3.0f); // Luz blanca fuerte
            dirLight.Specular = new Vector3(1.0f, 1.0f, 1.0f);


            // --- 5. LUZ PUNTUAL (Test de Cubemap - Luz Roja) ---
            // La metemos exactamente en medio de los 4 pilares de la "prisión"
            Light pLight = new Light("TestPointLight", LightType.Point);
            pLight.transform.position = new Vector3(-5.0f, 0.0f, -4.0f);
            pLight.Ambient = new Vector3(0.0f);
            pLight.Diffuse = new Vector3(25.0f, 0.0f, 0.0f); // ROJO puro HDR
            pLight.Specular = new Vector3(5.0f, 0.0f, 0.0f);
            pLight.Constant = 1.0f;
            pLight.Linear = 0.09f;
            pLight.Quadratic = 0.032f;

            Renderer pVisual = pLight.AddComponent<Renderer>();
            pVisual.SetMaterial(new Material(unlitShader, new Color4(1f, 0f, 0f, 1f)));
            pVisual.LoadModel("Models/Primitives/Cube.fbx");
            pLight.transform.scale = new Vector3(0.2f);

            Light pLight2 = new Light("TestPointLight2", LightType.Point);
            pLight2.transform.position = new Vector3(-1.0f, 0.0f, -4.0f);
            pLight2.Ambient = new Vector3(0.0f);
            pLight2.Diffuse = new Vector3(25.0f, 0.0f, 0.0f); // ROJO puro HDR
            pLight2.Specular = new Vector3(5.0f, 0.0f, 0.0f);
            pLight2.Constant = 1.0f;
            pLight2.Linear = 0.09f;
            pLight2.Quadratic = 0.032f;

            Renderer pVisual2 = pLight2.AddComponent<Renderer>();
            pVisual2.SetMaterial(new Material(unlitShader, new Color4(1f, 0f, 0f, 1f)));
            pVisual2.LoadModel("Models/Primitives/Cube.fbx");
            pLight2.transform.scale = new Vector3(0.2f);


            // --- 6. LUZ FOCAL (Test de Frustum y Array 2D - Luz Azul) ---
            // Apunta directamente a la pared del fondo, pasando por el cubo flotante
            Light sLight = new Light("TestSpotLight", LightType.Spot);
            sLight.transform.position = new Vector3(5.0f, 2.0f, -2.0f);
            sLight.transform.rotation = new Vector3(0f, 0f, 0f); // Mira directo hacia -Z (la pared)
            sLight.Ambient = new Vector3(0.0f);
            sLight.Diffuse = new Vector3(0.0f, 0.0f, 40.0f); // AZUL puro HDR
            sLight.Specular = new Vector3(0.0f, 0.0f, 10.0f);
            sLight.Constant = 1.0f;
            sLight.Linear = 0.045f;
            sLight.Quadratic = 0.0075f;
            // Ángulo estrecho para ver bien el círculo proyectado en la pared
            sLight.CutOff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
            sLight.OuterCutOff = (float)Math.Cos(MathHelper.DegreesToRadians(17.5f));

            Renderer sVisual = sLight.AddComponent<Renderer>();
            sVisual.SetMaterial(new Material(unlitShader, new Color4(0f, 0f, 1f, 1f)));
            sVisual.LoadModel("Models/Primitives/Cube.fbx");
            sLight.transform.scale = new Vector3(0.2f);

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

