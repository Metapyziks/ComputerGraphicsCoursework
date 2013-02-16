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
    class WaterShader : ShaderProgram3D
    {
        private LumTexture2D _ripplemap;

        private Color4 _colour = new Color4(48, 92, 120, 191);
        private int _colourLoc = -1;

        private int _viewVectorLoc = -1;
        private int _viewOriginLoc = -1;

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                if (_colourLoc != -1) {
                    GL.Uniform4(_colourLoc, value);
                }
            }
        }

        public WaterShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "view_origin");
            vert.AddUniform(ShaderVarType.Vec3, "view_vector");
            vert.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            vert.AddAttribute(ShaderVarType.Vec2, "in_vertex");
            vert.AddVarying(ShaderVarType.Float, "var_height");
            vert.AddVarying(ShaderVarType.Float, "var_scale");
            vert.AddVarying(ShaderVarType.Vec2, "var_offset");
            vert.AddVarying(ShaderVarType.Vec2, "var_texpos");
            vert.Logic = @"
                void main(void)
                {
                    const float size = 512.0;

                    float rot = -atan(view_vector.z, view_vector.x);
                    mat2 rmat = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));

                    var_offset = rmat * (in_vertex * size) + view_origin;
                    var_scale = max(0.0, 2.0 - max(1.0, length(in_vertex * size) / 48.0));

                    var_texpos = vec2(var_offset.x / 128.0 + 0.5, var_offset.y / 128.0 + 0.5);
                    var_height = texture(heightmap, var_texpos).a * 2.0 * var_scale;

                    gl_Position = view_matrix * vec4(var_offset.x + 0.01625, var_height - 1.0, var_offset.y, 1.0);
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            frag.AddUniform(ShaderVarType.Sampler2D, "ripplemap");
            frag.AddUniform(ShaderVarType.Sampler2D, "spraymap");
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.Logic = @"
                void main(void)
                {
                    float l = textureOffset(heightmap, var_texpos, ivec2(-1,  0 )).a * 2.0 * var_scale;
                    float t = textureOffset(heightmap, var_texpos, ivec2( 0, -1 )).a * 2.0 * var_scale;
                    float r = textureOffset(heightmap, var_texpos, ivec2( 1,  0 )).a * 2.0 * var_scale;
                    float b = textureOffset(heightmap, var_texpos, ivec2( 0,  1 )).a * 2.0 * var_scale;

                    vec3 horz = normalize(vec3(1.0, 0.0, r - l));
                    vec3 vert = normalize(vec3(0.0, 1.0, b - t));
                    vec3 normal = cross(horz, vert);

                    const vec3 light = normalize(vec3(-6, -14, -3));
                    out_frag_colour = vec4(colour.rgb * max(0.0, dot(-light, normal)), colour.a);
                    float shinys = 0.75 * pow(dot(reflect(light, normal), view_vector) * 0.5 + 0.5, 4.0);
                    out_frag_colour += vec4((vec3(1.0, 1.0, 1.0) - out_frag_colour.rgb) * shinys, 0.0);

                    if (var_scale > 0.0) {
                        float ripple = texture(ripplemap, (var_texpos * 8.0) + normal.xz).a;
                        float spray = texture(spraymap, var_texpos).a;
                        if (ripple * pow(spray, 2.0) > 0.75) {
                            out_frag_colour += spray * 0.75 * (vec4(1.0, 1.0, 1.0, 1.0) - out_frag_colour) * var_scale;
                        }
                    }
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

            AddAttribute("in_vertex", 2);
            AddTexture("heightmap", TextureUnit.Texture1);
            AddTexture("spraymap", TextureUnit.Texture3);
            AddTexture("ripplemap", TextureUnit.Texture4);

            var rand = new Random();
            _ripplemap = new LumTexture2D(128, 128);
            for (int x = 0; x < 128; ++x) {
                for (int y = 0; y < 128; ++y) {
                    _ripplemap[x, y] = (float) rand.NextDouble() * 0.5f + 0.5f;
                }
            }

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _viewVectorLoc = GL.GetUniformLocation(Program, "view_vector");
            _viewOriginLoc = GL.GetUniformLocation(Program, "view_origin");

            GL.Uniform4(_colourLoc, Colour);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Uniform3(_viewVectorLoc, Camera.ViewVector);
            GL.Uniform2(_viewOriginLoc, Camera.Position.X, Camera.Position.Z);

            SetTexture("ripplemap", _ripplemap);

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.Blend); //GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            // GL.CullFace(CullFaceMode.Front);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.Blend); //GL.Disable(EnableCap.CullFace);
        }

        public void Render(Vector2 vert)
        {
            GL.VertexAttrib2(Attributes[0].Location, vert);
        }
    }
}
