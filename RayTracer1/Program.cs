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
            Render();
        }

        static void Render()
        {
            const int width = 1024;
            const int height = 768;
            List<Vector3> frameBuffer = new List<Vector3>(width * height);
            frameBuffer.AddRange(Enumerable.Repeat(default(Vector3), frameBuffer.Capacity));
            
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    frameBuffer[i + j * width] = new Vector3(j / (float)height, i / (float)width, 0);
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
    }
}
