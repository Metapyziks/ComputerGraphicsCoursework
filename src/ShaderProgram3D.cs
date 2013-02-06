using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class ShaderProgram3D : ShaderProgram
    {
        private int _ViewMatrixLoc;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Camera Camera { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            _ViewMatrixLoc = GL.GetUniformLocation(Program, "view_matrix");
        }

        protected override void OnStartBatch()
        {
            if (Camera != null) {
                Matrix4 viewMat = Camera.ViewMatrix;
                GL.UniformMatrix4(_ViewMatrixLoc, false, ref viewMat);
            }
        }
    }
}
