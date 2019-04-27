using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RayTracer1
{
    static class Program
    {
        static void Main(string[] args)
        {
            Sphere sphere = new Sphere(new Vector3(-3, 0, -16), 2);
            Render(sphere);
        }

        static void Render(Sphere sphere)
        {
            const int width = 1024;
            const int height = 768;
            const float fov = (float)Math.PI / 2;

            List<Vector3> frameBuffer = new List<Vector3>(width * height);
            frameBuffer.AddRange(Enumerable.Repeat(default(Vector3), frameBuffer.Capacity));

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    float x = ((float)((2 * (i + 0.5) / (float)width - 1) * Math.Tan(fov / 2) * width / (float)height));
                    float y = (float)(-(2 * (j + 0.5) / (float)height - 1) * Math.Tan(fov / 2));
                    //Console.WriteLine($"x={x} y={y}");
                    Vector3 dir = Vector3.Normalize(new Vector3(x, y, -1)); 
                    frameBuffer[i + j * width] = CastRay(new Vector3(0, 0, 0), dir, sphere);
                }
            }

            using (var writer = new BinaryWriter(File.Open(@"render.ppm", FileMode.Create)))
            {
                writer.Write(Encoding.ASCII.GetBytes("P6\n"));
                writer.Write(Encoding.ASCII.GetBytes($"{width} {height}\n"));
                writer.Write(Encoding.ASCII.GetBytes("255\n"));
                for (int i = 0; i < height * width; ++i)
                {
                    writer.Write((byte)(255 * Math.Max(0.0f, Math.Min(1.0f, frameBuffer[i].X))));
                    writer.Write((byte)(255 * Math.Max(0.0f, Math.Min(1.0f, frameBuffer[i].Y))));
                    writer.Write((byte)(255 * Math.Max(0.0f, Math.Min(1.0f, frameBuffer[i].Z))));

                }
            }

        }

        static Vector3 CastRay(Vector3 orig, Vector3 dir, Sphere sphere)
        {
            float sphere_dist = float.MaxValue;
            if (!sphere.RayIntersect(orig,dir,ref sphere_dist))
            {
                return new Vector3(0.2f, 0.7f, 0.8f);
            }
            return new Vector3(0.4f, 0.4f, 0.3f);
        }
    }

    public class Sphere
    {
        public Vector3 center = new Vector3();
        public float radius;

        public Sphere(Vector3 c, float r)
        {
            this.center = c;
            this.radius = r;
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
