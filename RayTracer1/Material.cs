using System.Numerics;

namespace RayTracer1
{
    public class Material
    {
        //Albedo X is diffuse component and Albedo Y is glossy component
        public Vector3 albedo;
        public Vector3 diffuse_color = new Vector3();
        public float specular_exponent;
        public Material(Vector3 albedo, Vector3 color, float spec)
        {
            this.albedo = albedo;
            this.diffuse_color = (color);
            this.specular_exponent = spec;
        }
        public Material()
        {
            this.albedo = new Vector3(1,0,0);
            this.diffuse_color = new Vector3();
            this.specular_exponent = 0;
        }

    }
}
