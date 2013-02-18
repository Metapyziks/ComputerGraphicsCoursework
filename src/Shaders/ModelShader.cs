﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Shaders
{
    class ModelShader : ShaderProgram3D
    {
        private Color4 _colour = Color4.White;
        private int _colourLoc = -1;

        private BitmapTexture2D _texture = BitmapTexture2D.Blank;

        private Matrix4 _trans = Matrix4.Identity;
        private int _transLoc = -1;

        private float _shinyness = 0f;
        private int _shinynessLoc = -1;

        private int _viewVectorLoc = -1;

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                if (_colourLoc != -1) {
                    GL.Uniform4(_colourLoc, _colour);
                }
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
                if (_transLoc != -1) {
                    GL.UniformMatrix4(_transLoc, false, ref _trans);
                }
            }
        }

        public float Shinyness
        {
            get { return _shinyness; }
            set
            {
                _shinyness = value;
                if (_transLoc != -1) {
                    GL.Uniform1(_shinynessLoc, _shinyness);
                }
            }
        }

        public ModelShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
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
                    gl_Position = view_matrix * (transform * vec4(in_vertex, 1.0));
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Sampler2D, "tex");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.AddUniform(ShaderVarType.Float, "shinyness");
            frag.Logic = @"
                void main(void)
                {
                    const vec3 light = normalize(vec3(-6, -14, -3));
                    out_frag_colour = vec4(colour.rgb * texture(tex, var_textuv + vec2(0.5, 0.5)).rgb * (3.0 + dot(-light, var_normal)) * 0.25, colour.a);
                    if (shinyness > 0.0) {
                        out_frag_colour = vec4(out_frag_colour.rgb + (vec3(1.0, 1.0, 1.0) - out_frag_colour.rgb) * 0.5 * pow(max(0.0, dot(reflect(-light, var_normal), view_vector)), shinyness), out_frag_colour.a);
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

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _transLoc = GL.GetUniformLocation(Program, "transform");
            _shinynessLoc = GL.GetUniformLocation(Program, "shinyness");
            _viewVectorLoc = GL.GetUniformLocation(Program, "view_vector");

            GL.Uniform4(_colourLoc, Colour);
            GL.UniformMatrix4(_transLoc, false, ref _trans);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.CullFace);
            GL.Uniform4(_colourLoc, _colour);
            GL.UniformMatrix4(_transLoc, false, ref _trans);
            GL.Uniform1(_shinynessLoc, _shinyness);
            Vector3 viewVector = Camera.ViewVector;
            GL.Uniform3(_viewVectorLoc, ref viewVector);

            SetTexture("tex", _texture);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.CullFace);
        }

        public void Render(Vector3 vert, Vector3 norm)
        {
            GL.VertexAttrib3(Attributes[0].Location, vert);
            GL.VertexAttrib3(Attributes[1].Location, norm);
        }
    }
}