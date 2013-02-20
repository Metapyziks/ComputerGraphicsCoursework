using System;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Utils
{
    public static class Tools
    {
        public static float Clamp(float val, float min, float max)
        {
            return val < min ? min : val > max ? max : val;
        }

        public static double Clamp(double val, double min, double max)
        {
            return val < min ? min : val > max ? max : val;
        }

        public static void ErrorCheck(String loc = "unknown")
        {
#if DEBUG
            ErrorCode ec = GL.GetError();

            if (ec != ErrorCode.NoError) {
                var trace = new StackTrace();
                Debug.WriteLine(ec.ToString() + " at " + loc + Environment.NewLine + trace.ToString());

                throw new Exception(ec.ToString() + " at " + loc);
            }
#endif
        }
    }
}
