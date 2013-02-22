using System;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class WaterShader : WorldAwareShader
    {
        private AlphaTexture2D _ripplemap;

        private Color4 _colour = new Color4(48, 132, 255, 191);

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                SetUniform("colour", value);
            }
        }

        public WaterShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Vec3, "view_origin");
            vert.AddUniform(ShaderVarType.Vec3, "view_vector");
            vert.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            vert.AddAttribute(ShaderVarType.Vec2, "in_vertex");
            vert.AddVarying(ShaderVarType.Float, "var_height");
            vert.AddVarying(ShaderVarType.Float, "var_scale");
            vert.AddVarying(ShaderVarType.Vec3, "var_position");
            vert.AddVarying(ShaderVarType.Vec2, "var_texpos");
            vert.Logic = @"
                void main(void)
                {
                    const float size = 512.0;

                    float rot = -atan(view_vector.z, view_vector.x);
                    mat2 rmat = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));

                    var_scale = max(0.0, 2.0 - max(1.0, length(in_vertex * size) / 48.0));

                    vec2 offset = rmat * (in_vertex * size) + view_origin.xz;
                    var_texpos = offset / 128.0 + vec2(0.5, 0.5);
                    var_height = texture2D(heightmap, var_texpos).a * 2.0 * var_scale;
                    
                    vec4 pos = vec4(offset.x, var_height - 1.0, offset.y, 1.0);

                    var_position = pos.xyz;
                    gl_Position = vp_matrix * pos;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            frag.AddUniform(ShaderVarType.Sampler2D, "ripplemap");
            frag.AddUniform(ShaderVarType.Sampler2D, "spraymap");
            frag.AddUniform(ShaderVarType.SamplerCube, "skybox");
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Vec3, "view_origin");
            frag.AddUniform(ShaderVarType.Vec3, "light_vector");
            frag.Logic = @"
                void main(void)
                {
                    float l = textureOffset(heightmap, var_texpos, ivec2(-1,  0 )).a;
                    float t = textureOffset(heightmap, var_texpos, ivec2( 0, -1 )).a;
                    float r = textureOffset(heightmap, var_texpos, ivec2( 1,  0 )).a;
                    float b = textureOffset(heightmap, var_texpos, ivec2( 0,  1 )).a;

                    vec3 horz = normalize(vec3(0.0, (r - l) * 2.0 * var_scale, 1.0));
                    vec3 vert = normalize(vec3(1.0, (b - t) * 2.0 * var_scale, 0.0));
                    vec3 normal = cross(horz, vert);
                    
                    vec3 cam_dir = normalize(var_position - view_origin);

                    vec3 reflected = normalize(reflect(cam_dir, normal));

                    out_colour = vec4(colour.rgb * max(0.0, dot(light_vector, normal)), colour.a);
                    out_colour += vec4((textureCube(skybox, reflected).rgb - out_colour.rgb) * 0.5, 0.0);

                    if (var_scale > 0.0) {
                        float ripple = texture2D(ripplemap, (var_texpos * 8.0) + normal.xz * 0.125).a;
                        float spray = texture2D(spraymap, var_texpos).a;
                        if (ripple * pow(spray, 2.0) > 0.75) {
                            out_colour += spray * 0.75 * (vec4(1.0, 1.0, 1.0, 1.0) - out_colour) * var_scale;
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
            AddTexture("heightmap");
            AddTexture("spraymap");
            AddTexture("ripplemap");

            var rand = new Random();
            _ripplemap = new AlphaTexture2D(128, 128);
            for (int x = 0; x < 128; ++x) {
                for (int y = 0; y < 128; ++y) {
                    _ripplemap[x, y] = (float) rand.NextDouble() * 0.5f + 0.5f;
                }
            }

            AddUniform("colour");
            AddUniform("view_vector");
            AddUniform("view_origin");

            SetUniform("colour", Colour);
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            SetUniform("view_vector", Camera.ViewVector);
            SetUniform("view_origin", Camera.Position);

            SetTexture("ripplemap", _ripplemap);

            GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.Blend); //GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            // GL.CullFace(CullFaceMode.Front);
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            GL.Disable(EnableCap.DepthTest); GL.Disable(EnableCap.Blend); //GL.Disable(EnableCap.CullFace);
        }
    }
}
