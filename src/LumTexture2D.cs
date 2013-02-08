using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class LumTexture2D : Texture
    {
        private readonly int _actualSize;
        private float[,] _data;

        public LumTexture2D(int width, int height)
            : base(TextureTarget.Texture2D, width, height)
        {
            _actualSize = GetNextPOTS(Width, Height);

            _data = new float[Width, Height];
        }

        public Vector2 GetCoords(Vector2 pos)
        {
            return GetCoords(pos.X, pos.Y);
        }

        public Vector2 GetCoords(float x, float y)
        {
            return new Vector2 {
                X = x / _actualSize,
                Y = y / _actualSize
            };
        }

        public float this[int x, int y]
        {
            get { return _data[x, y]; }
            set
            {
                _data[x, y] = value;
                Update();
            }
        }

        protected override void Load()
        {
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, _actualSize, _actualSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, _data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        }
    }
}
