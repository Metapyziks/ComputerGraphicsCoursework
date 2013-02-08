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
    class DepthClipShader : ShaderProgram3D
    {
        private Matrix4 _trans = Matrix4.Identity;
        private int _transLoc = -1;

        public Matrix4 Transform
        {
            get { return _trans; }
            set
            {
                _trans = value;
                if (_transLoc != -1) {
                    GL.UniformMatrix4(_transLoc, false, ref _trans);
                }
            }
        }

        public DepthClipShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "transform");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddAttribute(ShaderVarType.Vec3, "in_normal");
            vert.Logic = @"
                void main(void)
                {
                    gl_Position = view_matrix * (transform * vec4(in_vertex, 1.0));
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.Logic = @"
                void main(void)
                {
                    out_frag_colour = vec4(0.0, 0.0, 0.0, 0.0);
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

            _transLoc = GL.GetUniformLocation(Program, "transform");
            GL.UniformMatrix4(_transLoc, false, ref _trans);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.UniformMatrix4(_transLoc, false, ref _trans);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.Blend);
        }

        public void Render(Vector3 vert, Vector3 norm)
        {
            GL.VertexAttrib3(Attributes[0].Location, vert);
            GL.VertexAttrib3(Attributes[1].Location, norm);
        }
    }
}
