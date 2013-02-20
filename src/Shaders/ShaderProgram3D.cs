using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ShaderProgram3D : ShaderProgram
    {
        private int _viewMatrixLoc;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Camera Camera { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            _viewMatrixLoc = GL.GetUniformLocation(Program, "vp_matrix");
        }

        protected override void OnBegin()
        {
            if (Camera != null) {
                Matrix4 viewMat = Camera.CombinedMatrix;
                GL.UniformMatrix4(_viewMatrixLoc, false, ref viewMat);
            }
        }
    }
}
