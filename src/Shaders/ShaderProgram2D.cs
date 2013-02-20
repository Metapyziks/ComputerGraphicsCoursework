using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ShaderProgram2D : ShaderProgram
    {
        public ShaderProgram2D()
            : base()
        {

        }

        public ShaderProgram2D(int width, int height)
            : base()
        {
            Create();
            SetScreenSize(width, height);
        }

        public void SetScreenSize(int width, int height)
        {
            int loc = GL.GetUniformLocation(Program, "screen_resolution");
            GL.Uniform2(loc, (float) width, (float) height);

            Tools.ErrorCheck("screensize");
        }
    }
}
