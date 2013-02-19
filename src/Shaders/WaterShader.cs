using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Shaders
{
    class WaterShader : WorldAwareShader
    {
        private LumTexture2D _ripplemap;

        private Color4 _colour = new Color4(48, 92, 120, 191);
        private int _colourLoc = -1;

        private int _viewVectorLoc = -1;
        private int _viewOriginLoc = -1;
        private int _viewMatrixLoc = -1;
        private int _viewInvMatrixLoc = -1;

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
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "view_origin");
            vert.AddUniform(ShaderVarType.Vec3, "view_vector");
            vert.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            vert.AddAttribute(ShaderVarType.Vec2, "in_vertex");
            vert.AddVarying(ShaderVarType.Float, "var_height");
            vert.AddVarying(ShaderVarType.Float, "var_scale");
            vert.AddVarying(ShaderVarType.Vec4, "var_position");
            vert.AddVarying(ShaderVarType.Vec2, "var_texpos");
            vert.Logic = @"
                void main(void)
                {
                    const float size = 512.0;

                    float rot = -atan(view_vector.z, view_vector.x);
                    mat2 rmat = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));

                    var_scale = max(0.0, 2.0 - max(1.0, length(in_vertex * size) / 48.0));

                    vec2 offset = rmat * (in_vertex * size) + view_origin;
                    var_texpos = offset / 128.0 + vec2(0.5, 0.5);
                    var_height = texture(heightmap, var_texpos).a * 2.0 * var_scale;
                    
                    var_position = vp_matrix * vec4(offset.x + 0.01625, var_height - 1.0, offset.y, 1.0);
                    gl_Position = var_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            frag.AddUniform(ShaderVarType.Sampler2D, "ripplemap");
            frag.AddUniform(ShaderVarType.Sampler2D, "spraymap");
            frag.AddUniform(ShaderVarType.SamplerCube, "skybox");
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Mat4, "view_matrix");
            frag.AddUniform(ShaderVarType.Mat4, "view_inv_matrix");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.AddUniform(ShaderVarType.Vec2, "view_origin");
            frag.AddUniform(ShaderVarType.Vec3, "light_vector");
            frag.Logic = @"
                void main(void)
                {
                    float l = textureOffset(heightmap, var_texpos, ivec2(-1,  0 )).a;
                    float t = textureOffset(heightmap, var_texpos, ivec2( 0, -1 )).a;
                    float r = textureOffset(heightmap, var_texpos, ivec2( 1,  0 )).a;
                    float b = textureOffset(heightmap, var_texpos, ivec2( 0,  1 )).a;

                    vec3 horz = normalize(vec3(0.0, (r - l) * 2.0 * var_scale, 2.0));
                    vec3 vert = normalize(vec3(2.0, (b - t) * 2.0 * var_scale, 0.0));
                    vec3 normal = cross(horz, vert);
                    
                    vec3 eye_normal = (view_matrix * vec4(normal, 0.0)).xyz;
                    vec3 eye_pos = normalize(var_position.xyz / var_position.w);

                    vec4 reflected = view_inv_matrix * vec4(reflect(eye_pos, eye_normal), 0.0);

                    out_frag_colour = texture(skybox, normalize(reflected.xyz));

                    //out_frag_colour = vec4(normal * 0.5 + vec3(0.5, 0.5, 0.5), 1.0);

                    //out_frag_colour = vec4(colour.rgb * max(0.0, dot(-light_vector, normal)), colour.a);
                    //out_frag_colour += vec4((texture(skybox, normalize(reflected.xyz)).rgb - out_frag_colour.rgb) * 0.5, 0.0);

                    //if (var_scale > 0.0) {
                    //    float ripple = texture(ripplemap, (var_texpos * 8.0) + normal.xz * 0.125).a;
                    //    float spray = texture(spraymap, var_texpos).a;
                    //    if (ripple * pow(spray, 2.0) > 0.75) {
                    //        out_frag_colour += spray * 0.75 * (vec4(1.0, 1.0, 1.0, 1.0) - out_frag_colour) * var_scale;
                    //    }
                    //}

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
            _ripplemap = new LumTexture2D(128, 128);
            for (int x = 0; x < 128; ++x) {
                for (int y = 0; y < 128; ++y) {
                    _ripplemap[x, y] = (float) rand.NextDouble() * 0.5f + 0.5f;
                }
            }

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _viewVectorLoc = GL.GetUniformLocation(Program, "view_vector");
            _viewOriginLoc = GL.GetUniformLocation(Program, "view_origin");
            _viewMatrixLoc = GL.GetUniformLocation(Program, "view_matrix");
            _viewInvMatrixLoc = GL.GetUniformLocation(Program, "view_inv_matrix");

            GL.Uniform4(_colourLoc, Colour);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Uniform3(_viewVectorLoc, Camera.ViewVector);
            GL.Uniform2(_viewOriginLoc, Camera.Position.X, Camera.Position.Z);

            Matrix4 viewMatrix = Camera.InvViewmatrix;
            Matrix4 invViewMatrix = Camera.InvViewmatrix;
            GL.UniformMatrix4(_viewMatrixLoc, false, ref viewMatrix);
            GL.UniformMatrix4(_viewInvMatrixLoc, false, ref invViewMatrix);

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
