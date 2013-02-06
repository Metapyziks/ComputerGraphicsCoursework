using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class SpriteShader : ShaderProgram2D
    {
        private BitmapTexture2D _Texture;

        public BitmapTexture2D Texture
        {
            get
            {
                return _Texture;
            }
            set
            {
                if ( _Texture != value )
                {
                    SetTexture( "texture0", value );
                    _Texture = value;
                }
            }
        }

        public SpriteShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, true );
            vert.AddAttribute( ShaderVarType.Vec2, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_texture" );
            vert.AddAttribute( ShaderVarType.Vec4, "in_colour" );
            vert.AddVarying( ShaderVarType.Vec2, "var_texture" );
            vert.AddVarying( ShaderVarType.Vec4, "var_colour" );
            vert.Logic = @"
                void main( void )
                {
                    var_texture = in_texture;
                    var_colour = in_colour;

                    gl_Position = in_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, true );
            frag.AddUniform( ShaderVarType.Sampler2D, "texture0" );
            frag.AddVarying( ShaderVarType.Vec2, "var_texture" );
            frag.AddVarying( ShaderVarType.Vec4, "var_colour" );
            frag.Logic = @"
                void main( void )
                {
                    vec4 clr = texture2D( texture0, var_texture ) * var_colour;

                    if( clr.a != 0.0 )
                        out_frag_colour = clr.rgba;
                    else
                        discard;
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;
        }

        public SpriteShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            if ( NVidiaCard )
            {
                AddAttribute( "in_texture", 2, 0, 2 );
                AddAttribute( "in_colour", 4, 0, 4 );
                AddAttribute( "in_position", 2, 0, 0 );
            }
            else
            {
                AddAttribute( "in_position", 2 );
                AddAttribute( "in_texture", 2 );
                AddAttribute( "in_colour", 4 );
            }

            AddTexture( "texture0", TextureUnit.Texture0 );
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable( EnableCap.Blend );

            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable( EnableCap.Blend );
        }
    }
}
