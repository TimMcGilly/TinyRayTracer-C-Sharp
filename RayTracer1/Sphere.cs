using System;
using System.Numerics;

namespace RayTracer1
{
    public class Sphere
    {
        public Vector3 center = new Vector3();
        public float radius;
        public Material material;

        public Sphere(Vector3 c, float r, Material m)
        {
            this.center = c;
            this.radius = r;
            this.material = m;
        }

        //Origin is origin of ray. Dir is the direction of the ray
        //http://www.lighthouse3d.com/tutorials/maths/ray-sphere-intersection/
        public bool RayIntersect(Vector3 orig, Vector3 dir, ref float t0)
        {
            Vector3 vpc = Vector3.Subtract(center, orig);  // this is the vector from p to c

            // dot product of vector between p and c and the direction vector of the ray. 
            // Let us tell if the sphere is infront of the ray or behind.
            float tca = Vector3.Dot(vpc, dir);

            //The dot product here calucates the length of the vector vpc squared. This may have something to do with projection.
            float d2 = Vector3.Dot(vpc, vpc) - (tca * tca);

            if (d2 > radius * radius)
            {
                return false;
            }
            
            float distToIntercept1 = (float)Math.Sqrt(radius * radius - d2);

            t0 = tca - distToIntercept1;
            float t1 = tca + distToIntercept1;

            if (t0 < 0) t0 = t1;
            if (t0 < 0) return false;

            return true;

        }
    }
}
