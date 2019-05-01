using System.Numerics;

namespace RayTracer1
{
    public class Light
    {
        public Vector3 position;
        public float intensity;
        public Light(Vector3 p, float i)
        {
            this.position = p;
            this.intensity = i;
        }

    }
}
