using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameEngine
{
    public static class Helper
    {
        private const string tempScenePath = "temp_currentScene.json";

        private const int maxSceneSlots = 10;
        private static int currentSceneSlot = 0;

        private static float currentTimeScale = 1;
        public static void Inputs(KeyboardState Keyboard, CursorState cursorState)
        {
            if(Keyboard.IsKeyPressed(Keys.KeyPad1))
            {
                SelectSceneSlot(1);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad2))
            {
                SelectSceneSlot(2);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad3))
            {
                SelectSceneSlot(3);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad4))
            {
                SelectSceneSlot(4);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad5))
            {
                SelectSceneSlot(5);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad6))
            {
                SelectSceneSlot(6);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad7))
            {
                SelectSceneSlot(7);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad8))
            {
                SelectSceneSlot(8);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPad9))
            {
                SelectSceneSlot(9);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if(Keyboard.IsKeyPressed(Keys.KeyPad0))
            {
                SelectSceneSlot(0);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPadAdd))
            {
                SelectSceneSlot(currentSceneSlot + 1);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            else if (Keyboard.IsKeyPressed(Keys.KeyPadSubtract))
            {
                SelectSceneSlot(currentSceneSlot - 1);
                Console.WriteLine($"Selected Scene Slot: {currentSceneSlot}");
            }
            if (Keyboard.IsKeyDown(Keys.LeftControl) && Keyboard.IsKeyPressed(Keys.S))
            {
                SceneSerializer.SaveScene(SceneManager.CurrentScene, currentSceneSlot + ".json");
            }
            if (Keyboard.IsKeyDown(Keys.LeftControl) && Keyboard.IsKeyPressed(Keys.L))
            {
                string sceneToLoad = currentSceneSlot + ".json";
                if (string.IsNullOrEmpty(sceneToLoad))
                {
                    Console.WriteLine("No scene saved in this slot!");
                    return;
                }
                else if (!sceneToLoad.EndsWith(".json"))
                {
                    Console.WriteLine("Invalid scene file in this slot!");
                    return;
                }
                SceneSerializer.LoadSceneFromFile(sceneToLoad);
            }
            if (Keyboard.IsKeyDown(Keys.LeftControl) && Keyboard.IsKeyPressed(Keys.R))
            {
                SceneSerializer.LoadSceneFromFile(tempScenePath);
                Console.WriteLine($"Scene Reset Successfully");
            }

            if (Keyboard.IsKeyPressed(Keys.P))
            {
                Time.timeScale = Time.timeScale == 0 ? currentTimeScale : 0;
                cursorState = Time.timeScale == 0 ?  CursorState.Normal : CursorState.Grabbed;
                Game.instance.ResetMouseFirstMove();
            }

            if(Keyboard.IsKeyDown(Keys.LeftAlt) && Keyboard.IsKeyPressed(Keys.Right))
            {
                Time.timeScale += 0.25f;
                Time.timeScale = Math.Clamp(Time.timeScale, 0.25f, 2f);
                currentTimeScale = Time.timeScale;
                Console.WriteLine($"Current Time Scale: {Time.timeScale}");
            }

            if (Keyboard.IsKeyDown(Keys.LeftAlt) && Keyboard.IsKeyPressed(Keys.Left))
            {
                Time.timeScale -= 0.25f;
                Time.timeScale = Math.Clamp(Time.timeScale, 0.25f, 2f);
                currentTimeScale = Time.timeScale;
                Console.WriteLine($"Current Time Scale: {Time.timeScale}");
            }

            if(Keyboard.IsKeyDown(Keys.LeftAlt) && Keyboard.IsKeyPressed(Keys.R))
            {
                Time.timeScale = 1f;
                currentTimeScale = Time.timeScale;
                Console.WriteLine($"Time Scale Reset");
            }
        }

        public static void SaveTempScene()
        {
            SceneSerializer.SaveScene(SceneManager.CurrentScene, tempScenePath);
        }

        private static void SelectSceneSlot(int slot)
        {
            if(slot < 0)
            {
                slot = maxSceneSlots;
            }
            else if (slot > maxSceneSlots)
            {
                slot = 0;
            }
            currentSceneSlot = slot;
        }
    }
}
