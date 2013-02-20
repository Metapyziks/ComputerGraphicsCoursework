using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework.Shaders
{
    public abstract class WaterEffectShader : ShaderProgram2D
    {
        private Texture _heightmap;
        private Texture _velocitymap;
        private Texture _spraymap;

        private int _resolutionLoc;

        private float[] _bounds;

        public Texture HeightMap
        {
            get { return _heightmap; }
            set
            {
                if (_heightmap != value) {
                    SetTexture("heightmap", value);
                    _heightmap = value;
                }
            }
        }

        public Texture VelocityMap
        {
            get { return _velocitymap; }
            set
            {
                if (_velocitymap != value) {
                    SetTexture("velocitymap", value);
                    _velocitymap = value;
                }
            }
        }
        
        public Texture SprayMap
        {
            get { return _spraymap; }
            set
            {
                if (_spraymap != value) {
                    SetTexture("spraymap", value);
                    _spraymap = value;
                }
            }
        }

        protected WaterEffectShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, true);
            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.AddVarying(ShaderVarType.Vec2, "tex_pos");
            vert.AddUniform(ShaderVarType.Int, "resolution");
            vert.Logic = @"
                void main( void )
                {
                    tex_pos = in_position.xy * vec2(1.0, -1.0) / resolution;
                    gl_Position = in_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, true, vert);
            OnAddShaderVariables(frag);
            OnAddShaderLogic(frag);
            
            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Quads;

            Create();
            SetScreenSize(Water.Resolution, Water.Resolution);
            _bounds = new float[] {
                0f, 0f,
                Water.Resolution, 0f,
                Water.Resolution, Water.Resolution,
                0f, Water.Resolution
            };
        }

        protected virtual void OnAddShaderVariables(ShaderBuilder frag)
        {
            frag.AddUniform(ShaderVarType.Sampler2D, "heightmap");
            frag.AddUniform(ShaderVarType.Sampler2D, "velocitymap");
            frag.AddUniform(ShaderVarType.Sampler2D, "spraymap");
        }

        protected virtual void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = "void main(void) { discard; }";
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_position", 2);
            AddTexture("heightmap");
            AddTexture("velocitymap");
            AddTexture("spraymap");

            _resolutionLoc = GL.GetUniformLocation(Program, "resolution");
            GL.Uniform1(_resolutionLoc, Water.Resolution);
        }

        public void SetTextures(Texture heightmap, Texture velocitymap, Texture spraymap)
        {
            HeightMap = heightmap;
            VelocityMap = velocitymap;
            SprayMap = spraymap;
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            SetTexture("heightmap", HeightMap);
            SetTexture("velocitymap", VelocityMap);
            SetTexture("spraymap", SprayMap);
        }

        public void Render()
        {
            Render(_bounds);
        }
    }
}
