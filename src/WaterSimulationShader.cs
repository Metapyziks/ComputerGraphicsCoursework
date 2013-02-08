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

        public WaterSimulationShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, true);
            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.Logic = @"
                void main( void )
                {
                    gl_Position = in_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, true);
            frag.AddUniform(ShaderVarType.Sampler2D, "wavemap");
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = vec4(1.0, 1.0, 1.0, 1.0);
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
            AddTexture("wavemap", TextureUnit.Texture0);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void Render()
        {
            Render(_bounds);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();
            GL.Disable(EnableCap.Blend);
        }
    }
}
