using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Shaders
{
    class WorldAwareShader : ShaderProgram3D
    {
        private int _lightDirLoc;

        public World World { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            _lightDirLoc = GL.GetUniformLocation(Program, "light_vector");

            AddTexture("skybox");
        }
    }
}
