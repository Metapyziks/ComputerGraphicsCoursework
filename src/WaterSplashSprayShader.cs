using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSplashSprayShader : WaterSplashEffectShader
    {
        public override string GetMagnitudeCalculation()
        {
            return "max(scale, texture(spraymap, tex_pos).a)";
        }
    }
}
