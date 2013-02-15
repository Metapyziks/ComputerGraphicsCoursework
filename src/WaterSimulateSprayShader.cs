using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSimulateSprayShader : WaterEffectShader
    {
        protected override void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = @"
                void main(void)
                {
                    const ivec2 offsets[] = ivec2[8] (
                        ivec2(-1, -1), ivec2( 0, -1), ivec2( 1, -1),
                        ivec2(-1,  0),                ivec2( 1,  0),
                        ivec2(-1,  1), ivec2( 0,  1), ivec2( 1,  1)
                    );

                    const float weights[] = float[8] (
                        0.707, 1.0, 0.707, 1.0, 1.0, 0.707, 1.0, 0.707
                    );
                    
                    float cur = texture(spraymap, tex_pos).a;
                    float mx = 
                        max(
                            max(
                                max(
                                    textureOffset(spraymap, tex_pos, offsets[0]).a * weights[0],
                                    textureOffset(spraymap, tex_pos, offsets[1]).a * weights[1]
                                ),
                                max(
                                    textureOffset(spraymap, tex_pos, offsets[2]).a * weights[2],
                                    textureOffset(spraymap, tex_pos, offsets[3]).a * weights[3]
                                )
                            ),
                            max(
                                max(
                                    textureOffset(spraymap, tex_pos, offsets[4]).a * weights[4],
                                    textureOffset(spraymap, tex_pos, offsets[5]).a * weights[5]
                                ),
                                max(
                                    textureOffset(spraymap, tex_pos, offsets[6]).a * weights[6],
                                    textureOffset(spraymap, tex_pos, offsets[7]).a * weights[7]
                                )
                            )
                        );

                    float vel = pow(abs(texture(velocitymap, tex_pos).a - 0.5), 3.0);
                    float new = min(1.0, cur + max((mx - cur) * 0.5, 0.0) + vel) * 0.98;
                    
                    out_frag_colour = vec4(new, new, new, new);
                }
            ";
        }
    }
}
