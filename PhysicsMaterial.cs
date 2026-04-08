using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
