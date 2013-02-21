using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class WaterSimulateHeightShader : WaterEffectShader
    {
        protected override void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = @"
                void main(void)
                {
                    float cur = texture2D(heightmap, tex_pos).a;
                    float vel = (texture2D(velocitymap, tex_pos).a - 0.5) / 8.0;
                    float new = cur + vel;
                    out_frag_colour = vec4(new, new, new, new);
                }
            ";
        }
    }
}
