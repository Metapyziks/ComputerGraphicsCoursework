using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
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
    }
}
