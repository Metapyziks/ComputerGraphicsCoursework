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
        private Color4 _colour = new Color4(48, 92, 120, 127);
        private int _colourLoc = -1;

        private int _viewVectorLoc = -1;

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
            vert.AddUniform(ShaderVarType.Sampler2D, "wavemap");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddVarying(ShaderVarType.Vec3, "var_normal");
            vert.Logic = @"
                void main(void)
                {
                    const ivec2 offsets[] = ivec2[4] (
                        ivec2(-1, 0), ivec2(0, -1),
                        ivec2(1, 0), ivec2(0, 1)
                    );

                    vec2 tex_pos = vec2((in_vertex.x + 32.0) / 64.0, (in_vertex.z + 32.0) / 64.0);
                    float wave = texture(wavemap, tex_pos).a;
                    float neighbours[] = float[4] (
                        textureOffset(wavemap, tex_pos, offsets[0]).a,
                        textureOffset(wavemap, tex_pos, offsets[1]).a,
                        textureOffset(wavemap, tex_pos, offsets[2]).a,
                        textureOffset(wavemap, tex_pos, offsets[3]).a
                    );
                    vec3 horz = normalize(vec3(2.0, 0.0, neighbours[2] - neighbours[0]));
                    vec3 vert = normalize(vec3(0.0, 2.0, neighbours[3] - neighbours[1]));
                    var_normal = cross(horz, vert);
                    gl_Position = view_matrix * vec4(in_vertex.x, in_vertex.y + wave, in_vertex.z, 1.0);
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddUniform(ShaderVarType.Vec3, "view_vector");
            frag.AddVarying(ShaderVarType.Vec3, "var_normal");
            frag.Logic = @"
                void main(void)
                {
                    const vec3 light = normalize(vec3(-3, -4, -8));
                    out_frag_colour = vec4(colour.rgb + (vec3(0.3, 0.7, 0.9) - colour.rgb) * pow(max(0.0, dot(reflect(-light, var_normal), view_vector)), 3.4), colour.a);
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

            AddAttribute("in_vertex", 3);
            AddTexture("wavemap", TextureUnit.Texture1);

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _viewVectorLoc = GL.GetUniformLocation(Program, "view_vector");

            GL.Uniform4(_colourLoc, Colour);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            if (Camera != null) {
                GL.Uniform3(_viewVectorLoc, Camera.ViewVector);
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

        public void Render(Vector3 vert)
        {
            GL.VertexAttrib3(Attributes[0].Location, vert);
        }
    }
}
