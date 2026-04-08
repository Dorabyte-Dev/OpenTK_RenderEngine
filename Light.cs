using System;
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
        public Transform transform => owner.transform;

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
            RenderManager.AddLight(this);
        }

        public override void Destroy()
        {
            RenderManager.RemoveLight(this);
        }
    }
}
