public class PhysicsMaterial
{
    public float Friction { get; set; }
    public float Bounciness { get; set; }

    public PhysicsMaterial(float friction = 0.5f, float bounciness = 0.0f)
    {
        Friction = friction;
        Bounciness = bounciness;
    }
}
