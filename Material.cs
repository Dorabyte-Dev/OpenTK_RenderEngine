using Assimp.Unmanaged;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Newtonsoft.Json;

namespace GameEngine
{
    public class Material
    {
        public Shader shader;
        [JsonProperty]private Color4 baseColor = Color4.White;

        [JsonProperty]private Dictionary<string, Texture> textures = new();

        [JsonProperty] private Dictionary<string, int> ints = new();
        [JsonProperty] private Dictionary<string, float> floats = new();
        [JsonProperty] private Dictionary<string, Vector3> vectors = new();
        [JsonProperty] private Dictionary<string, Matrix4> matrices = new();

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public Material(Shader shader, Color4 baseColor)
        {
            this.shader = shader;
            this.baseColor = baseColor;
        }

        public Material(Shader shader, Texture texture)
        {
            this.shader = shader;
            textures.Add("_mainTex", texture);
        }

        public void SetBaseColor(Color4 color)
        {
            baseColor = color;
        }
        public void SetInt(string name, int value)
        {
            ints[name] = value;
        }
        public void SetFloat(string name, float value)
        {
            floats[name] = value;
        }

        public void SetVector(string name, Vector3 value)
        {
            vectors[name] = value;
        }

        public void SetMatrix(string name, Matrix4 value)
        {
            matrices[name] = value;
        }

        public void SetTexture(string name, Texture texture)
        {
            textures[name] = texture;
        }

        public void Apply()
        {
            shader.Use();

            if(textures.Count <= 0) 
            {
                shader.SetColor("baseColor", baseColor);
                shader.SetInt("hasTexture", 0);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            else
            {
                int textureUnit = 0;
                shader.SetInt("hasTexture", 1);
                foreach (var kv in textures)
                {
                    kv.Value.Use(TextureUnit.Texture0 + textureUnit);
                    shader.SetInt(kv.Key, textureUnit);
                    textureUnit++;
                }
            }
            foreach (var kv in ints)
                shader.SetInt(kv.Key, kv.Value);
            foreach (var kv in floats)
                shader.SetFloat(kv.Key, kv.Value);

            foreach (var kv in vectors)
                shader.SetVector3(kv.Key, kv.Value);

            foreach (var kv in matrices)
                shader.SetMatrix4(kv.Key, kv.Value);
        }

    }
}
