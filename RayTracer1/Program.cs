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
            
            Material ivory =  new Material(new Vector2(0.6f,0.3f), new Vector3(0.4f, 0.4f, 0.3f), 50f);
            Material red_rubber = new Material(new Vector2(0.9f, 0.1f), new Vector3(0.3f, 0.1f, 0.1f), 10f);

            List<Sphere> spheres = new List<Sphere>();
            spheres.Add(new Sphere(new Vector3(-3f, 0f, -16f), 2, ivory));
            spheres.Add(new Sphere(new Vector3(-1.0f, -1.5f, -12f), 2, red_rubber));
            spheres.Add(new Sphere(new Vector3(1.5f, -0.5f, -18f), 3, red_rubber));
            spheres.Add(new Sphere(new Vector3(7f, 5f, -18f), 4, ivory));

            List<Light> lights = new List<Light>();
            lights.Add(new Light(new Vector3(-20f, 20f, 20f), 1.5f));
            lights.Add(new Light(new Vector3(30f, 50f, -25f), 1.8f));
            lights.Add(new Light(new Vector3(30f, 20f, 30f), 1.7f));


            Render(spheres.ToArray(), lights.ToArray());
        }

        static void Render(Sphere[] spheres, Light[] lights)
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
                    frameBuffer[i + j * width] = CastRay(new Vector3(0, 0, 0), dir, spheres, lights);
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

        static bool SceneIntersect(Vector3 orig, Vector3 dir, Sphere[] spheres, ref Vector3 hit, ref Vector3 normal, ref Material material )
        {
            float spheres_dist = float.MaxValue;
            foreach (Sphere sphere in spheres)
            {
                float dist_i = 0;
                if (sphere.RayIntersect(orig, dir, ref dist_i) && dist_i < spheres_dist)
                {
                    spheres_dist = dist_i;
                    hit = orig + (dir * dist_i);
                    normal = Vector3.Normalize(Vector3.Subtract(hit, sphere.center));
                    material = sphere.material;
                }
            }
            return spheres_dist < 1000;
        }


        static Vector3 CastRay(Vector3 orig, Vector3 dir, Sphere[] spheres, Light[] lights)
        {
            Vector3 point = new Vector3();
            Vector3 normal = new Vector3();
            Material material = new Material();

            if (!SceneIntersect(orig,dir,spheres, ref point, ref normal, ref material))
            {
                return new Vector3(0.2f, 0.7f, 0.8f);
            }

            float diffuse_light_intensity = 0, specular_light_intensity = 0;

            foreach (Light light in lights)
            {
                Vector3 light_dir = Vector3.Normalize(light.position - point);

                diffuse_light_intensity += light.intensity * Math.Max(0.0f, Vector3.Dot(light_dir, normal));
                specular_light_intensity += (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(
                    -reflect(-light_dir, normal),dir)), material.specular_exponent) * light.intensity;
            }
            
            //Albedo X is diffuse component and Albedo Y is glossy component
            return material.diffuse_color * diffuse_light_intensity * material.albedo.X + new Vector3(1f, 1f, 1f) * specular_light_intensity * material.albedo.Y;
        }

        //I is the light direction and N is the normal. It calculates the reflect ray of light.  https://en.wikipedia.org/wiki/Phong_reflection_model
        static Vector3 reflect(Vector3 I, Vector3 N)
        {
            return I - N * 2.0f * (I * N);
        }

        static float Norm(Vector3 vector)
        {
            return (float) Math.Sqrt(Vector3.Dot(vector,vector));
        }

    }

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

    public class Material
    {
        //Albedo X is diffuse component and Albedo Y is glossy component
        public Vector2 albedo;
        public Vector3 diffuse_color = new Vector3();
        public float specular_exponent;
        public Material(Vector2 albedo, Vector3 color, float spec)
        {
            this.albedo = albedo;
            this.diffuse_color = (color);
            this.specular_exponent = spec;
        }
        public Material()
        {
            this.albedo = new Vector2(1,0);
            this.diffuse_color = new Vector3();
            this.specular_exponent = 0;
        }

    }
}
