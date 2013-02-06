using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class TestShader : ShaderProgram3D
    {
        public TestShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.Logic = @"
                void main(void)
                {
                    gl_Position = view_matrix * vec4(in_vertex.x, 1.0);
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.Logic = @"
                void main(void)
                {
                    out_frag_colour = colour;
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Triangles;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_vertex", 3);
        }

        public void Render(params Vector3[] verts)
        {
            foreach (var vert in verts) {
                GL.VertexAttrib3(Attributes[0].Location, vert);
            }
        }
    }
}
