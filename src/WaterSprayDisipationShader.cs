using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSprayDissipationShader : WaterEffectShader
    {
        protected override void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = @"
                void main(void)
                {
                    discard;

                    const ivec2 offsets[] = ivec2[8] (
                        ivec2(-1, -1), ivec2( 0, -1), ivec2( 1, -1),
                        ivec2(-1,  0),                ivec2( 1,  0),
                        ivec2(-1,  1), ivec2( 0,  1), ivec2( 1,  1)
                    );

                    const float weights[] = float[8] (
                        0.707, 1.0, 0.707, 1.0, 1.0, 0.707, 1.0, 0.707
                    );
                    
                    float cur = texture(heightmap, tex_pos).a;
                    float av = 0.3;
                    av += textureOffset(spraymap, tex_pos, offsets[0]).a * weights[0];
                    av += textureOffset(spraymap, tex_pos, offsets[1]).a * weights[1];
                    av += textureOffset(spraymap, tex_pos, offsets[2]).a * weights[2];
                    av += textureOffset(spraymap, tex_pos, offsets[3]).a * weights[3];
                    av += textureOffset(spraymap, tex_pos, offsets[4]).a * weights[4];
                    av += textureOffset(spraymap, tex_pos, offsets[5]).a * weights[5];
                    av += textureOffset(spraymap, tex_pos, offsets[6]).a * weights[6];
                    av += textureOffset(spraymap, tex_pos, offsets[7]).a * weights[7];

                    float new = cur + max((av / 7.828 - cur), 0.0);
                    
                    out_frag_colour = vec4(new, new, new, new);
                }
            ";
        }
    }
}
