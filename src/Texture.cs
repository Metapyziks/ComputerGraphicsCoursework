using System;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class Texture
    {
        protected static int GetNextPOTS( int wid, int hei )
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow( 2.0, Math.Ceiling( Math.Log( max, 2.0 ) ) );
        }

        private static Texture _sCurrentLoadedTexture;

        public static Texture Current
        {
            get
            {
                return _sCurrentLoadedTexture;
            }
        }

        private int _ID;
        private bool _Loaded;

        public TextureTarget TextureTarget { get; private set; }

        public bool Ready
        {
            get
            {
                return _ID > -1;
            }
        }

        public int ID
        {
            get
            {
                if ( !Ready )
                    GL.GenTextures( 1, out _ID );

                return _ID;
            }
        }

        public Texture( TextureTarget target )
        {
            TextureTarget = target;

            _ID = -1;
            _Loaded = false;
        }

        public void Update()
        {
            _Loaded = false;
        }

        protected virtual void Load()
        {

        }

        public void Bind()
        {
            if ( _sCurrentLoadedTexture != this )
            {
                GL.BindTexture( TextureTarget, ID );
                _sCurrentLoadedTexture = this;
            }

            if ( !_Loaded )
            {
                Load();
                _Loaded = true;
            }
        }
    }
}
