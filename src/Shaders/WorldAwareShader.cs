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
        public World World { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddUniform("light_vector");

            AddTexture("skybox");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            SetUniform("light_vector", World.LightDirection);
            SetTexture("skybox", World.Skybox);
        }
    }
}
