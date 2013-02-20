using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class WaterSimulateVelocityShader : WaterEffectShader
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
                    
                    float cur = texture(heightmap, tex_pos).a;
                    float avg = (0.5
                        + textureOffset(heightmap, tex_pos, offsets[0]).a * weights[0]
                        + textureOffset(heightmap, tex_pos, offsets[1]).a * weights[1]
                        + textureOffset(heightmap, tex_pos, offsets[2]).a * weights[2]
                        + textureOffset(heightmap, tex_pos, offsets[3]).a * weights[3]
                        + textureOffset(heightmap, tex_pos, offsets[4]).a * weights[4]
                        + textureOffset(heightmap, tex_pos, offsets[5]).a * weights[5]
                        + textureOffset(heightmap, tex_pos, offsets[6]).a * weights[6]
                        + textureOffset(heightmap, tex_pos, offsets[7]).a * weights[7]) / 7.828;

                    float new = min(1.0, max(0.0, texture(velocitymap, tex_pos).a + (avg - cur) * 2.0)) * 0.996;
                    
                    out_frag_colour = vec4(new, new, new, new);
                }
            ";
        }
    }
}
