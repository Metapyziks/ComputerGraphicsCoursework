using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework
{
    class World
    {
        private static CubeMapTexture _sDefaultSkyCubeMap;

        public Vector3 LightDirection { get; set; }
        public CubeMapTexture Skybox { get; set; }

        public World()
        {
            float sunPitch = 35f;
            float sunYaw = 165f;

            LightDirection = new Vector3((float) (Math.Cos(sunPitch) * Math.Cos(sunYaw)),
                (float) Math.Sin(sunPitch), (float) (Math.Cos(sunPitch) *(float)  Math.Sin(sunYaw)));
            LightDirection /= LightDirection.Length;

            Skybox = _sDefaultSkyCubeMap = _sDefaultSkyCubeMap ?? CubeMapTexture.FromFiles("../../res/sky_{0}.png");
        }
    }
}
