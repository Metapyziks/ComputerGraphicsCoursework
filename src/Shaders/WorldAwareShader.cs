using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework.Shaders
{
    public class WorldAwareShader : ShaderProgram3D
    {
        private int _lightDirLoc;

        public World World { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            _lightDirLoc = GL.GetUniformLocation(Program, "light_vector");

            AddTexture("skybox");
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            if (_lightDirLoc != -1) {
                GL.Uniform3(_lightDirLoc, World.LightDirection);
            }

            SetTexture("skybox", World.Skybox);
        }
    }
}
