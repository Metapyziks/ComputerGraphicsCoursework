using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Shaders
{
    class SkyShader : WorldAwareShader
    {
        private const float BoxSize = 64f;

        private static readonly float[] _sVerts = new float[] {
            -BoxSize, -BoxSize, -BoxSize, /**/ -BoxSize, +BoxSize, -BoxSize, /**/ -BoxSize, +BoxSize, +BoxSize, /**/ -BoxSize, -BoxSize, +BoxSize, // Left
            +BoxSize, -BoxSize, -BoxSize, /**/ +BoxSize, +BoxSize, -BoxSize, /**/ +BoxSize, +BoxSize, +BoxSize, /**/ +BoxSize, -BoxSize, +BoxSize, // Right
            -BoxSize, -BoxSize, -BoxSize, /**/ +BoxSize, -BoxSize, -BoxSize, /**/ +BoxSize, -BoxSize, +BoxSize, /**/ -BoxSize, -BoxSize, +BoxSize, // Bottom
            -BoxSize, +BoxSize, -BoxSize, /**/ +BoxSize, +BoxSize, -BoxSize, /**/ +BoxSize, +BoxSize, +BoxSize, /**/ -BoxSize, +BoxSize, +BoxSize, // Top
            -BoxSize, -BoxSize, -BoxSize, /**/ +BoxSize, -BoxSize, -BoxSize, /**/ +BoxSize, +BoxSize, -BoxSize, /**/ -BoxSize, +BoxSize, -BoxSize, // Front
            -BoxSize, -BoxSize, +BoxSize, /**/ +BoxSize, -BoxSize, +BoxSize, /**/ +BoxSize, +BoxSize, +BoxSize, /**/ -BoxSize, +BoxSize, +BoxSize, // Back
        };

        private static VertexBuffer _sVB;

        public SkyShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddVarying(ShaderVarType.Vec3, "var_texcoord");
            vert.Logic = @"
                void main(void)
                {
                    vec4 pos = view_matrix * vec4(in_vertex, 0.0);
                    gl_Position = pos.xyww;
                    var_texcoord = in_vertex;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.SamplerCube, "skybox");
            frag.Logic = @"
                void main(void)
                {
                    //out_frag_colour = vec4(var_texcoord, 1.0);
                    out_frag_colour = texture(skybox, var_texcoord);
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Quads;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_vertex", 3);

            if (_sVB == null) {
                _sVB = new VertexBuffer(3);
                _sVB.SetData(_sVerts);
            }
        }

        public void Render()
        {
            StartBatch();
            _sVB.StartBatch(this);
            _sVB.Render(this);
            _sVB.EndBatch(this);
            EndBatch();
        }
    }
}
