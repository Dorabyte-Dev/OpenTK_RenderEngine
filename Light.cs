using System;
using Newtonsoft.Json;
using OpenTK.Mathematics;

namespace GameEngine
{
    public enum LightType
    {
        Directional,
        Point,
        Spot
    }

    public class Light : Component
    {
        [JsonIgnore]public Transform transform => owner.transform;

        public LightType Type = LightType.Directional;

        public Vector3 Ambient = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 Specular = new Vector3(1.0f, 1.0f, 1.0f);

        public float Constant = 1.0f;
        public float Linear = 0.00f;
        public float Quadratic = 0.032f;

        public float CutOff = MathF.Cos(MathHelper.DegreesToRadians(12.5f));
        public float OuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(15.0f));

        public Light(GameObject owner) : base(owner)
        {
            
        }

        public override void Start()
        {
            RenderManager.AddLight(this);
        }
        public override void Destroy()
        {
            RenderManager.RemoveLight(this);
        }
        #region Builder Pattern
        public Light SetType(LightType type)
        {
            Type = type;
            return this;
        }

        public Light SetAmbient(Vector3 ambient)
        {
            Ambient = ambient;
            return this;
        }

        public Light SetDiffuse(Vector3 diffuse)
        {
            Diffuse = diffuse;
            return this;
        }

        public Light SetSpecular(Vector3 specular)
        {
            Specular = specular;
            return this;
        }

        public Light SetDistance(float distance)
        {
            if (distance <= 0.001f)
                distance = 0.1f;

            this.Constant = 1.0f;

            this.Linear = 4.5f / distance;
            this.Quadratic = 75.0f / (distance * distance);

            return this;
        }

        public Light SetCutOff(float cutOff)
        {
            CutOff = MathF.Cos(MathHelper.DegreesToRadians(cutOff));
            return this;
        }

        public Light SetOuterCutOff(float outerCutOff)
        {
            OuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(outerCutOff));
            return this;
        }

        #endregion
    }
}
