using OpenTK;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class DepthClipShader : ShaderProgram3D
    {
        private Matrix4 _trans = Matrix4.Identity;

        public Matrix4 Transform
        {
            get { return _trans; }
            set
            {
                _trans = value;
                SetUniform("transform", ref _trans);
            }
        }

        public DepthClipShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "transform");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.Logic = @"
                void main(void)
                {
                    gl_Position = vp_matrix * (transform * vec4(in_vertex, 1.0));
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
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
            AddUnusedAttribute(5);

            AddUniform("transform");
            SetUniform("transform", ref _trans);
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            SetUniform("transform", ref _trans);
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.Blend);
        }
    }
}
