using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSplashShader : ShaderProgram2D
    {
        private Texture _wavemap;
        private float[] _bounds;

        private Vector3 _splash;
        private int _splashLoc;

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

        public Vector2 SplashPosition
        {
            get { return _splash.Xy * 64f; }
            set
            {
                _splash.X = value.X / 64f + 0.5f;
                _splash.Y = value.Y / 64f + 0.5f;

                while (_splash.X < 0f) _splash.X += 1f;
                while (_splash.X >= 1f) _splash.X -= 1f;
                while (_splash.Y < 0f) _splash.Y += 1f;
                while (_splash.Y >= 1f) _splash.Y -= 1f;
            }
        }

        public float SplashMagnitude
        {
            get { return _splash.Z; }
            set
            {
                _splash.Z = value;
            }
        }

        private WaterSplashShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, true);
            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.Logic = "void main( void ) { gl_Position = in_position; }";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, true);
            frag.AddUniform(ShaderVarType.Sampler2D, "wavemap");
            frag.AddUniform(ShaderVarType.Vec3, "splash");
            frag.Logic = @"
                float diff(float a, float b)
                {
                    float d = b - a;
                    if (d < -0.5) return d + 1.0;
                    if (d >= 0.5) return d - 1.0;
                    return d;
                }

                void main(void)
                {
                    vec2 tex_pos = gl_FragCoord.xy / 512.0;
                    vec3 cur = texture(wavemap, tex_pos).xyz;
                    vec2 diff = vec2(diff(splash.x, tex_pos.x), diff(splash.y, tex_pos.y));
                    float dist = length(diff);
                    if (dist < 1.0 / 64.0) {
                        out_frag_colour = vec4(0.125, cur.y, 1.0, 1.0);
                    } else {
                        discard;
                    }
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Quads;
        }

        public WaterSplashShader(int width, int height)
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

            _splashLoc = GL.GetUniformLocation(Program, "splash");
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Uniform3(_splashLoc, ref _splash);
        }

        public void Render()
        {
            Render(_bounds);
        }
    }
}
