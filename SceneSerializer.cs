using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace GameEngine
{
    public static class SceneSerializer
    {
        private static JsonSerializerSettings GetSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,

                TypeNameHandling = TypeNameHandling.Auto,

                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new Color4Converter());

            return settings;
        }

        public static void SaveScene(Scene scene, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(scene, GetSettings());

                File.WriteAllText("Scenes/" + filePath, json);

                Console.WriteLine("Saved Scene successfully in " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving scene: {ex.Message}");
            }
        }
        private static Scene DeserializeScene(string filePath)
        {
            filePath = "Scenes/" + filePath;

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: Scene not found in {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                Scene loadedScene = JsonConvert.DeserializeObject<Scene>(json, GetSettings());
                Console.WriteLine($"Scene '{loadedScene.Name}' loaded successfully!");
                return loadedScene;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error when loading scene: {ex.Message}");
                return null;
            }
        }

        public static void LoadSceneFromFile(string filePath)
        {
            Scene loadedScene = DeserializeScene(filePath);
            if (loadedScene != null)
            {
                PhysicsManager.ResetPhysics();
                SceneManager.CurrentScene = loadedScene;
            }
        }   
    }

    #region Custom Converters for OpenTK types
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X"); writer.WriteValue(value.X);
            writer.WritePropertyName("Y"); writer.WriteValue(value.Y);
            writer.WritePropertyName("Z"); writer.WriteValue(value.Z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Vector3(
                jo["X"].Value<float>(),
                jo["Y"].Value<float>(),
                jo["Z"].Value<float>()
            );
        }
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X"); writer.WriteValue(value.X);
            writer.WritePropertyName("Y"); writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Vector2(
                jo["X"].Value<float>(),
                jo["Y"].Value<float>()
            );
        }
    }

    public class Color4Converter : JsonConverter<Color4>
    {
        public override void WriteJson(JsonWriter writer, Color4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("R"); writer.WriteValue(value.R);
            writer.WritePropertyName("G"); writer.WriteValue(value.G);
            writer.WritePropertyName("B"); writer.WriteValue(value.B);
            writer.WritePropertyName("A"); writer.WriteValue(value.A);
            writer.WriteEndObject();
        }

        public override Color4 ReadJson(JsonReader reader, Type objectType, Color4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            return new Color4(
                jo["R"].Value<float>(),
                jo["G"].Value<float>(),
                jo["B"].Value<float>(),
                jo["A"].Value<float>()
            );
        }
    }
    #endregion
}
