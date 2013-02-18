using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace ComputerGraphicsCoursework
{
    class World
    {
        private static CubeMapTexture _sDefaultSkyCubeMap;

        public Vector3 LightDirection { get; set; }
        public CubeMapTexture Skybox { get; set; }

        public World()
        {
            LightDirection = new Vector3(-6f, -14f, -3f);
            LightDirection.Normalize();

            Skybox = _sDefaultSkyCubeMap = _sDefaultSkyCubeMap ?? CubeMapTexture.FromFiles("../../res/sky_{0}.png");
        }
    }
}
