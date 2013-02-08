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

        private Color4 _colour = new Color4(48, 92, 120, 127);
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
            vert.AddUniform(ShaderVarType.Sampler2D, "wavemap");
            vert.AddAttribute(ShaderVarType.Vec2, "in_vertex");
            vert.AddVarying(ShaderVarType.Float, "var_height");
            vert.AddVarying(ShaderVarType.Vec2, "var_offset");
            vert.AddVarying(ShaderVarType.Vec2, "var_texpos");
            vert.AddVarying(ShaderVarType.Vec3, "var_normal");
            vert.Logic = @"
                void main(void)
                {
                    const float resolution = 128.0;

                    const ivec2 offsets[] = ivec2[4] (
                        ivec2(-1, 0), ivec2(0, -1),
                        ivec2(1, 0), ivec2(0, 1)
                    );

                    var_offset = vec2(int(in_vertex.x), int(in_vertex.y));
                    vec2 pos = vec2((in_vertex.x - var_offset.x - 0.5) * resolution, (in_vertex.y - var_offset.y - 0.5) * resolution) + var_offset + view_origin;
                    var_offset = pos;

                    var_texpos = vec2((pos.y + 32.0) / 64.0, (pos.x + 32.0) / 64.0);
                    var_height = texture(wavemap, var_texpos).a;
                    float neighbours[] = float[4] (
                        textureOffset(wavemap, var_texpos, offsets[0]).a,
                        textureOffset(wavemap, var_texpos, offsets[1]).a,
                        textureOffset(wavemap, var_texpos, offsets[2]).a,
                        textureOffset(wavemap, var_texpos, offsets[3]).a
                    );
                    vec3 horz = normalize(vec3(2.0, 0.0, neighbours[2] - neighbours[0]));
                    vec3 vert = normalize(vec3(0.0, 2.0, neighbours[3] - neighbours[1]));
                    var_normal = cross(horz, vert);
                    gl_Position = view_matrix * vec4(pos.x, var_height, pos.y, 1.0);
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Sampler2D, "ripplemap");
            frag.AddUniform(ShaderVarType.Sampler2D, "spraymap");
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.AddVarying(ShaderVarType.Float, "var_height");
            frag.AddVarying(ShaderVarType.Vec2, "var_offset");
            frag.AddVarying(ShaderVarType.Vec2, "var_texpos");
            frag.AddVarying(ShaderVarType.Vec3, "var_normal");
            frag.Logic = @"
                void main(void)
                {
                    const vec3 light = normalize(vec3(-3, -8, -4));
                    out_frag_colour = vec4(colour.rgb * max(0.0, -dot(light, var_normal)), colour.a);
                    out_frag_colour = vec4(out_frag_colour.rgb + (vec3(0.6, 0.7, 0.9) - out_frag_colour.rgb) * pow(max(0.0, dot(reflect(-light, var_normal), view_vector)), 10.0), out_frag_colour.a);
                    float ripple = texture(ripplemap, (var_offset / 4.0) + var_normal.xz).a;
                    float spray = texture(spraymap, var_texpos).a;
                    if (ripple * pow(spray, 0.5) - var_height > 0.25) {
                        out_frag_colour += spray * (vec4(1.0, 1.0, 1.0, 1.0) - out_frag_colour);
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
            AddTexture("wavemap", TextureUnit.Texture1);
            AddTexture("ripplemap", TextureUnit.Texture2);
            AddTexture("spraymap", TextureUnit.Texture3);

            var rand = new Random();
            _ripplemap = new LumTexture2D(64, 64);
            for (int x = 0; x < 64; ++x) {
                for (int y = 0; y < 64; ++y) {
                    _ripplemap[x, y] = (float) rand.NextDouble();
                }
            }

            SetTexture("ripplemap", _ripplemap);

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _viewVectorLoc = GL.GetUniformLocation(Program, "view_vector");
            _viewOriginLoc = GL.GetUniformLocation(Program, "view_origin");

            GL.Uniform4(_colourLoc, Colour);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            if (Camera != null) {
                GL.Uniform3(_viewVectorLoc, Camera.ViewVector);
                GL.Uniform2(_viewOriginLoc, Camera.Position.X, Camera.Position.Z);
            }

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.Blend); GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.CullFace(CullFaceMode.Front);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.Blend); GL.Disable(EnableCap.CullFace);
        }

        public void Render(Vector2 vert)
        {
            GL.VertexAttrib2(Attributes[0].Location, vert);
        }
    }
}
