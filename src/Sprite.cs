using System;

using OpenTK;
using OpenTK.Graphics;

namespace ComputerGraphicsCoursework
{
    public class Sprite : IRenderable<SpriteShader>
    {
        internal float[] Vertices
        {
            get
            {
                return _Vertices;
            }
        }

        private BitmapTexture2D _Texture;
        private float[] _Vertices;

        private Vector2 _Position;
        private Vector2 _Scale;

        private Vector2 _SubrectOffset;
        private Vector2 _SubrectSize;

        private bool _FlipHorz;
        private bool _FlipVert;
        
        private float _Rotation;
        private bool _UseCentreAsOrigin;
        private Color4 _Colour;

        protected bool VertsChanged;
        
        public virtual Vector2 Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if ( value != _Position )
                {
                    _Position = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 Size
        {
            get
            {
                return new Vector2( _SubrectSize.X * Scale.X, _SubrectSize.Y * Scale.Y );
            }
            set
            {
                Scale = new Vector2( value.X / _SubrectSize.X, value.Y / _SubrectSize.Y );
            }
        }

        public virtual Vector2 Scale
        {
            get
            {
                return _Scale;
            }
            set
            {
                if ( value != _Scale )
                {
                    _Scale = value;
                    VertsChanged = true;
                }
            }
        }

        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector2( value, Y );
            }
        }
        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector2( X, value );
            }
        }

        public virtual Vector2 SubrectOffset
        {
            get
            {
                return _SubrectOffset;
            }
            set
            {
                if ( value != _SubrectOffset )
                {
                    _SubrectOffset = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 SubrectSize
        {
            get
            {
                return _SubrectSize;
            }
            set
            {
                if ( value != _SubrectSize )
                {
                    _SubrectSize = value;
                    VertsChanged = true;
                }
            }
        }

        public float SubrectLeft
        {
            get
            {
                return SubrectOffset.X;
            }
            set
            {
                SubrectOffset = new Vector2( value, SubrectTop );
            }
        }

        public float SubrectTop
        {
            get
            {
                return SubrectOffset.Y;
            }
            set
            {
                SubrectOffset = new Vector2( SubrectLeft, value );
            }
        }

        public float SubrectRight
        {
            get
            {
                return SubrectOffset.X + SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2( value - SubrectOffset.X, SubrectHeight );
            }
        }

        public float SubrectBottom
        {
            get
            {
                return SubrectOffset.Y + SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2( SubrectWidth, value - SubrectOffset.Y );
            }
        }

        public float SubrectWidth
        {
            get
            {
                return SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2( value, SubrectHeight );
            }
        }

        public float SubrectHeight
        {
            get
            {
                return SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2( SubrectWidth, value );
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }
            set
            {
                Scale = new Vector2( value / SubrectSize.X, Scale.Y );
            }
        }
        public float Height
        {
            get
            {
                return Size.Y;
            }
            set
            {
                Scale = new Vector2( Scale.X, value / SubrectSize.Y );
            }
        }

        public bool FlipHorizontal
        {
            get
            {
                return _FlipHorz;
            }
            set
            {
                if ( value != _FlipHorz )
                {
                    _FlipHorz = value;
                    VertsChanged = true;
                }
            }
        }

        public bool FlipVertical
        {
            get
            {
                return _FlipVert;
            }
            set
            {
                if ( value != _FlipVert )
                {
                    _FlipVert = value;
                    VertsChanged = true;
                }
            }
        }

        public float Rotation
        {
            get
            {
                return _Rotation;
            }
            set
            {
                if ( value != _Rotation )
                {
                    _Rotation = value;
                    VertsChanged = true;
                }
            }
        }

        public bool UseCentreAsOrigin
        {
            get
            {
                return _UseCentreAsOrigin;
            }
            set
            {
                if ( value != _UseCentreAsOrigin )
                {
                    _UseCentreAsOrigin = value;
                    VertsChanged = true;
                }
            }
        }

        public Color4 Colour
        {
            get
            {
                return _Colour;
            }
            set
            {
                if ( value != _Colour )
                {
                    _Colour = value;
                    VertsChanged = true;
                }
            }
        }

        public BitmapTexture2D Texture
        {
            get
            {
                return _Texture;
            }
        }

        public Sprite( float width, float height, Color4 colour )
        {
            _Texture = BitmapTexture2D.Blank;

            Position = new Vector2();
            Scale = new Vector2( width, height );
            SubrectOffset = new Vector2( 0, 0 );
            SubrectSize = new Vector2( _Texture.Width, _Texture.Height );
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = colour;
        }

        public Sprite( BitmapTexture2D texture, float scale = 1.0f )
        {
            _Texture = texture;

            Position = new Vector2();
            Scale = new Vector2( 1, 1 );
            SubrectOffset = new Vector2( 0, 0 );
            SubrectSize = new Vector2( _Texture.Width, _Texture.Height );
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = new Color4( 1.0f, 1.0f, 1.0f, 1.0f );

            Scale = new Vector2( scale, scale );
        }

        protected virtual float[] FindVerts()
        {
            Vector2 tMin = _Texture.GetCoords( SubrectLeft, SubrectTop );
            Vector2 tMax = _Texture.GetCoords( SubrectRight, SubrectBottom );
            float xMin = FlipHorizontal ? tMax.X : tMin.X;
            float yMin = FlipVertical ? tMax.Y : tMin.Y;
            float xMax = FlipHorizontal ? tMin.X : tMax.X;
            float yMax = FlipVertical ? tMin.Y : tMax.Y;

            float halfWid = Width / 2;
            float halfHei = Height / 2;

            float[,] verts = UseCentreAsOrigin ? new float[ , ]
            {
                { -halfWid, -halfHei },
                { +halfWid, -halfHei },
                { +halfWid, +halfHei },
                { -halfWid, +halfHei }
            } : new float[ , ]
            {
                { 0, 0 },
                { Width, 0 },
                { Width, Height },
                { 0, Height }
            };

            float[,] mat = new float[,]
            {
                { (float) Math.Cos( Rotation ), -(float) Math.Sin( Rotation ) },
                { (float) Math.Sin( Rotation ),  (float) Math.Cos( Rotation ) }
            };

            for ( int i = 0; i < 4; ++i )
            {
                float x = verts[ i, 0 ];
                float y = verts[ i, 1 ];
                verts[ i, 0 ] = X + mat[ 0, 0 ] * x + mat[ 0, 1 ] * y;
                verts[ i, 1 ] = Y + mat[ 1, 0 ] * x + mat[ 1, 1 ] * y;
            }

            return new float[]
            {
                verts[ 0, 0 ], verts[ 0, 1 ], xMin, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 1, 0 ], verts[ 1, 1 ], xMax, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 2, 0 ], verts[ 2, 1 ], xMax, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[ 3, 0 ], verts[ 3, 1 ], xMin, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
            };
        }

        public virtual void Render( SpriteShader shader )
        {
            if ( VertsChanged )
            {
                _Vertices = FindVerts();
                VertsChanged = false;
            }

            if ( !Texture.Ready || shader.Texture.ID != Texture.ID )
                shader.Texture = Texture;

            shader.Render( _Vertices );
        }
    }
}
