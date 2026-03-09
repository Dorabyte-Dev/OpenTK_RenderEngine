using System;
using OpenTK.Mathematics;

namespace GameEngine
{
    // Definimos los tipos de luz disponibles
    public enum LightType
    {
        Directional,
        Point,
        Spot
    }

    public class Light : Component
    {
        // Tipo de luz por defecto
        public LightType Type = LightType.Directional;

        // --- Propiedades Comunes ---
        public Vector3 Position => owner.transform.position;
        public Vector3 Forward => owner.transform.Forward;

        public Vector3 Ambient = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 Specular = new Vector3(1.0f, 1.0f, 1.0f);

        // --- Propiedades para Point y Spot Lights (Atenuación) ---
        // Estos valores mágicos (1.0, 0.09, 0.032) son el estándar de OpenGL para cubrir unos 50 metros.
        public float Constant = 1.0f;
        public float Linear = 0.09f;
        public float Quadratic = 0.032f;

        // --- Propiedades para Spot Lights ---
        // Pre-calculamos el coseno en radianes porque es más eficiente para el shader
        public float CutOff = MathF.Cos(MathHelper.DegreesToRadians(12.5f));
        public float OuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(15.0f));

        public Light(GameObject owner) : base(owner)
        {
            // ¡La magia del automatismo! 
            // En cuanto se crea la luz, se registra en el RenderManager.
            RenderManager.AddLight(this);
        }

        public override void Start()
        {
            // Método requerido por tu clase base Component, puede quedar vacío de momento.
        }

        // Es una buena práctica tener un método para limpiar el componente si se destruye el GameObject
        public void OnDestroy()
        {
            RenderManager.RemoveLight(this);
        }
    }
}
