using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ModelShader : WorldAwareShader
    {
        private Color4 _colour = Color4.White;
        private BitmapTexture2D _texture = BitmapTexture2D.Blank;
        private Matrix4 _trans = Matrix4.Identity;
        private float _shinyness = 0f;

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                SetUniform("colour", _colour);
            }
        }

        public BitmapTexture2D Texture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                SetTexture("tex", _texture);
            }
        }

        public Matrix4 Transform
        {
            get { return _trans; }
            set
            {
                _trans = value;
                SetUniform("transform", ref _trans);
            }
        }

        public float Shinyness
        {
            get { return _shinyness; }
            set
            {
                _shinyness = value;
                SetUniform("shinyness", value);
            }
        }

        public ModelShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "transform");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddAttribute(ShaderVarType.Vec2, "in_textuv");
            vert.AddAttribute(ShaderVarType.Vec3, "in_normal");
            vert.AddVarying(ShaderVarType.Vec3, "var_normal");
            vert.AddVarying(ShaderVarType.Vec2, "var_textuv");
            vert.Logic = @"
                void main(void)
                {
                    var_normal = (transform * vec4(in_normal, 0.0)).xyz;
                    var_textuv = in_textuv;
                    gl_Position = vp_matrix * (transform * vec4(in_vertex, 1.0));
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Sampler2D, "tex");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.AddUniform(ShaderVarType.Vec3, "light_vector");
            frag.AddUniform(ShaderVarType.Float, "shinyness");
            frag.Logic = @"
                void main(void)
                {
                    out_frag_colour = vec4(colour.rgb * texture2D(tex, var_textuv + vec2(0.5, 0.5)).rgb * (dot(-light_vector, var_normal) * 0.5 + 0.5), colour.a);
                    if (shinyness > 0.0) {
                        out_frag_colour = vec4(out_frag_colour.rgb + (vec3(1.0, 1.0, 1.0) - out_frag_colour.rgb) * 0.5 * pow(max(0.0, dot(reflect(-light_vector, var_normal), view_vector)), shinyness), out_frag_colour.a);
                    }
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
            AddAttribute("in_textuv", 2);
            AddAttribute("in_normal", 3);

            AddTexture("tex");

            AddUniform("colour");
            AddUniform("transform");
            AddUniform("shinyness");
            AddUniform("view_vector");

            SetUniform("colour", Colour);
            SetUniform("transform", ref _trans);
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.CullFace);

            SetUniform("colour", Colour);
            SetUniform("transform", ref _trans);
            SetUniform("shinyness", _shinyness);
            SetUniform("view_vector", Camera.ViewVector);

            SetTexture("tex", _texture);
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.CullFace);
        }
    }
}
