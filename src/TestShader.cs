using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
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
            vert.AddAttribute(ShaderVarType.Vec3, "in_normal");
            vert.AddVarying(ShaderVarType.Vec3, "var_normal");
            vert.Logic = @"
                void main(void)
                {
                    var_normal = in_normal;
                    gl_Position = view_matrix * vec4(in_vertex, 1.0);
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddVarying(ShaderVarType.Vec3, "var_normal");
            frag.Logic = @"
                void main(void)
                {
                    const vec3 light = normalize(vec3(-3, -4, -8));
                    out_frag_colour = vec4(colour.rgb * (3.0 + dot(light, var_normal)) * 0.25, 1.0);
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
            AddAttribute("in_normal", 3);

            GL.Uniform4(GL.GetUniformLocation(Program, "colour"), Color4.White);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest);
        }

        public void Render(Vector3 vert, Vector3 norm)
        {
            GL.VertexAttrib3(Attributes[0].Location, vert);
            GL.VertexAttrib3(Attributes[1].Location, norm);
        }
    }
}
