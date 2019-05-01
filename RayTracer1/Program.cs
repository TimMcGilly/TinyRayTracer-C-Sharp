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
            
            Material ivory =  new Material(new Vector3(0.6f,0.3f, 0.1f), new Vector3(0.4f, 0.4f, 0.3f), 50f);
            Material red_rubber = new Material(new Vector3(0.9f, 0.1f, 0.0f), new Vector3(0.3f, 0.1f, 0.1f), 10f);
            Material mirror = new Material(new Vector3(0.0f, 10.0f, 0.8f), new Vector3(1.0f, 1.0f, 1.0f), 1425.0f);

            List<Sphere> spheres = new List<Sphere>();
            spheres.Add(new Sphere(new Vector3(-3f, 0f, -16f), 2, ivory));
            spheres.Add(new Sphere(new Vector3(-1.0f, -1.5f, -12f), 2, mirror));
            spheres.Add(new Sphere(new Vector3(1.5f, -0.5f, -18f), 3, red_rubber));
            spheres.Add(new Sphere(new Vector3(7f, 5f, -18f), 4, mirror));

            List<Light> lights = new List<Light>();
            lights.Add(new Light(new Vector3(-20f, 20f, 20f), 1.5f));
            lights.Add(new Light(new Vector3(30f, 50f, -25f), 1.8f));
            lights.Add(new Light(new Vector3(30f, 20f, 30f), 1.7f));


            Render(spheres.ToArray(), lights.ToArray());
        }

        public static void Render(Sphere[] spheres, Light[] lights)
        {
            const int width = 1024*4;
            const int height = 768*4;
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
                    float angle = (float)0;
                    float new_x = (float)(x * Math.Cos(angle) - y * Math.Cos(angle));
                    float new_y = (float)(y * Math.Cos(angle) + x * Math.Cos(angle));

                    Vector3 dir = Vector3.Normalize(new Vector3(new_x, new_y, -1)); 
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


        public static Vector3 CastRay(Vector3 orig, Vector3 dir, Sphere[] spheres, Light[] lights, int depth = 0)
        {
            Vector3 point = new Vector3();
            Vector3 normal = new Vector3();
            Material material = new Material();

            if (depth > 2 || !SceneIntersect(orig,dir,spheres, ref point, ref normal, ref material))
            {
                return new Vector3(0.2f, 0.7f, 0.8f);
            }


            //Calculates reflections
            Vector3 reflect_dir = reflect(dir, normal);
            Vector3 reflect_orig;
            if (Vector3.Dot(reflect_dir, normal) < 0)
            {
                reflect_orig = point - (normal * 0.001f);
            }
            else
            {
                reflect_orig = point + (normal * 0.001f);
            }

            Vector3 reflect_color = CastRay(reflect_orig, reflect_dir, spheres, lights, depth + 1);


            //Calculates diffuse and specular
            float diffuse_light_intensity = 0, specular_light_intensity = 0;
            foreach (Light light in lights)
            {
                             Vector3 light_dir = Vector3.Normalize(light.position - point);
                float light_distance = Norm(light.position - point);

                Vector3 shadow_orig;
                // checking if the point lies in the shadow of the llight. This moves the shadow origin onto the surface of the object.
                if (Vector3.Dot(light_dir, normal) < 0) {
                    shadow_orig = Vector3.Subtract(point, Vector3.Multiply(normal, 0.001f));
                }
                else
                {
                    shadow_orig = Vector3.Add(point, Vector3.Multiply(normal, 0.001f));
                }

                Vector3 shadow_pt = default, shadow_N = default;
                Material tmpmaterial = new Material();

                //Skips the current light source if it would be in shadow.
                if (SceneIntersect(shadow_orig, light_dir, spheres, ref shadow_pt, ref shadow_N, ref tmpmaterial) && Norm(shadow_pt - shadow_orig) < light_distance)
                {
                    continue;
                }
                diffuse_light_intensity += light.intensity * Math.Max(0.0f, Vector3.Dot(light_dir, normal));
                specular_light_intensity += (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(
                    -reflect(-light_dir, normal),dir)), material.specular_exponent) * light.intensity;
            }
            
            //Albedo X is diffuse component and Albedo Y is glossy component
            return material.diffuse_color * diffuse_light_intensity * material.albedo.X + new Vector3(1f, 1f, 1f) * specular_light_intensity * material.albedo.Y +  reflect_color*material.albedo.Z;
        }

        //I is the light direction and N is the normal. It calculates the reflect ray of light.  https://en.wikipedia.org/wiki/Phong_reflection_model
        static Vector3 reflect(Vector3 I, Vector3 N)
        {
            return (I-(Vector3.Multiply(2.0f * Vector3.Dot(I, N), N)));
        }

        static float Norm(Vector3 vector)
        {
            return (float) Math.Sqrt(Vector3.Dot(vector,vector));
        }

    }
}
