using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSimulationShader : ShaderProgram2D
    {
        private Texture _wavemap;
        private float[] _bounds;

        public Texture WaveMap
        {
            get { return _wavemap; }
            set
            {
                if (_wavemap != value) {
                    SetTexture("wavemap", value);
                    _wavemap = value;
                }
            }
        }

        private WaterSimulationShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, true);
            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.Logic = "void main( void ) { gl_Position = in_position; }";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, true);
            frag.AddUniform(ShaderVarType.Sampler2D, "wavemap");
            frag.Logic = @"
                const float pi = 3.1415927;
                void main(void)
                {
                    vec2 tex_pos = gl_FragCoord.xy / 512.0;
                    vec3 cur = texture(wavemap, tex_pos).xyz;

                    const ivec2 offsets[] = ivec2[8] (
                        ivec2(-1, -1), ivec2( 0, -1), ivec2( 1, -1),
                        ivec2(-1,  0),                ivec2( 1,  0),
                        ivec2(-1,  1), ivec2( 0,  1), ivec2( 1,  1)
                    );
                    const float weights[] = float[8] (
                        0.707, 1.0, 0.707, 1.0, 1.0, 0.707, 1.0, 0.707
                    );

                    /*
                    float av = 0.25;
                    av += textureOffset(wavemap, tex_pos, offsets[0]).x * weights[0];
                    av += textureOffset(wavemap, tex_pos, offsets[1]).x * weights[1];
                    av += textureOffset(wavemap, tex_pos, offsets[2]).x * weights[2];
                    av += textureOffset(wavemap, tex_pos, offsets[3]).x * weights[3];
                    av += textureOffset(wavemap, tex_pos, offsets[4]).x * weights[4];
                    av += textureOffset(wavemap, tex_pos, offsets[5]).x * weights[5];
                    av += textureOffset(wavemap, tex_pos, offsets[6]).x * weights[6];
                    av += textureOffset(wavemap, tex_pos, offsets[7]).x * weights[7];
                    av /= 7.828;
                    */

                    float av = 0.3;
                    av += textureOffset(wavemap, tex_pos, offsets[0]).x * weights[0];
                    av += textureOffset(wavemap, tex_pos, offsets[1]).x * weights[1];
                    av += textureOffset(wavemap, tex_pos, offsets[2]).x * weights[2];
                    av += textureOffset(wavemap, tex_pos, offsets[3]).x * weights[3];
                    av += textureOffset(wavemap, tex_pos, offsets[4]).x * weights[4];
                    av += textureOffset(wavemap, tex_pos, offsets[5]).x * weights[5];
                    av += textureOffset(wavemap, tex_pos, offsets[6]).x * weights[6];
                    av += textureOffset(wavemap, tex_pos, offsets[7]).x * weights[7];

                    out_frag_colour = vec4(cur.x, cur.y, cur.z * 0.995 + max(av / 7.828 - cur.z, 0.0), 1.0);
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Quads;
        }

        public WaterSimulationShader(int width, int height)
            : this()
        {
            Create();
            SetScreenSize(width, height);
            _bounds = new float[] {
                0f, 0f,
                width, 0f,
                width, height,
                0f, height
            };
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_position", 2);
            AddTexture("wavemap", TextureUnit.Texture1);
        }

        public void Render()
        {
            Render(_bounds);
        }
    }
}
