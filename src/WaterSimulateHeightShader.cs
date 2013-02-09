using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSimulateHeightShader : WaterEffectShader
    {
        protected override void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = @"
                void main(void)
                {
                    float cur = texture(heightmap, tex_pos).a;
                    float vel = (texture(velocitymap, tex_pos).a - 0.5) / 8.0;
                    float new = cur + vel;
                    out_frag_colour = vec4(new, new, new, new);
                }
            ";
        }
    }
}
