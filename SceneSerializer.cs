using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace GameEngine
{
    public static class SceneSerializer
    {
        // Configuramos las reglas de oro para serializar videojuegos
        private static JsonSerializerSettings GetSettings()
        {
            var settings = new JsonSerializerSettings
            {
                // Hace que el JSON tenga saltos de línea y sea fácil de leer para un humano
                Formatting = Formatting.Indented,

                // ¡LA MAGIA DEL POLIMORFISMO!
                // Esto añade una línea "$type" en el JSON indicando si un componente es Light, Camera, etc.
                TypeNameHandling = TypeNameHandling.Auto,

                // Por si se nos ha escapado algún bucle infinito, esto evita que el motor crashee
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new Color4Converter());

            return settings;
        }

        public static void SaveScene(Scene scene, string filePath)
        {
            Console.WriteLine($"Guardando escena '{scene.Name}' en {filePath}...");

            try
            {
                // Convertimos toda la instancia de la escena a un string JSON gigante
                string json = JsonConvert.SerializeObject(scene, GetSettings());

                // Lo guardamos en el disco duro
                File.WriteAllText(filePath, json);

                Console.WriteLine("¡Escena guardada con éxito!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la escena: {ex.Message}");
            }
        }
        /*public static Scene LoadScene(string filePath)
        {
            Console.WriteLine($"Intentando cargar escena desde {filePath}...");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: No se encontró el archivo {filePath}");
                return null;
            }

            try
            {
                // 1. Leemos el texto plano
                string json = File.ReadAllText(filePath);

                // 2. ¡La magia de Newtonsoft! Reconstruye todo el árbol de objetos
                Scene loadedScene = JsonConvert.DeserializeObject<Scene>(json, GetSettings());

                Console.WriteLine($"¡Escena '{loadedScene.Name}' cargada con éxito en la memoria RAM!");
                return loadedScene;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico al cargar la escena: {ex.Message}");
                return null;
            }
        }*/
    }

    // --- CONVERTIDOR PARA VECTOR 3 ---
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

    // --- CONVERTIDOR PARA VECTOR 2 ---
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

    // --- CONVERTIDOR PARA COLOR4 ---
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
}
