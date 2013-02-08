using System;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public abstract class Texture
    {
        protected static int GetNextPOTS(int wid, int hei)
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow(2.0, Math.Ceiling(Math.Log(max, 2.0)));
        }

        private static Texture _sCurrentLoadedTexture;

        public static Texture Current
        {
            get
            {
                return _sCurrentLoadedTexture;
            }
        }

        private int _id;
        private bool _loaded;

        public TextureTarget TextureTarget { get; private set; }

        public bool Ready
        {
            get
            {
                return _id > -1;
            }
        }

        public int ID
        {
            get
            {
                if (!Ready)
                    GL.GenTextures(1, out _id);

                return _id;
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(TextureTarget target, int width, int height)
        {
            TextureTarget = target;
            Width = width;
            Height = height;

            _id = -1;
            _loaded = false;
        }

        public void Update()
        {
            _loaded = false;
        }

        protected abstract void Load();

        public void Bind()
        {
            if (_sCurrentLoadedTexture != this) {
                GL.BindTexture(TextureTarget, ID);
                _sCurrentLoadedTexture = this;
            }

            Tools.ErrorCheck("bindtexture");

            if (!_loaded) {
                Load();
                _loaded = true;
            }
        }
    }
}
