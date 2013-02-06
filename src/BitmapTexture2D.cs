using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class BitmapTexture2D : Texture
    {
        public static readonly BitmapTexture2D Blank;

        static BitmapTexture2D()
        {
            Bitmap blankBmp = new Bitmap(1, 1);
            blankBmp.SetPixel(0, 0, Color.White);
            Blank = new BitmapTexture2D(blankBmp);
        }

        private readonly int _ActualSize;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public BitmapTexture2D(Bitmap bitmap)
            : base(TextureTarget.Texture2D)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;

            _ActualSize = GetNextPOTS(bitmap.Width, bitmap.Height);

            if (_ActualSize == bitmap.Width && _ActualSize == bitmap.Height)
                Bitmap = bitmap;
            else {
                Bitmap = new Bitmap(_ActualSize, _ActualSize);

                for (int x = 0; x < Width; ++x)
                    for (int y = 0; y < Height; ++y)
                        Bitmap.SetPixel(x, y, bitmap.GetPixel(x, y));
            }
        }

        public Vector2 GetCoords(Vector2 pos)
        {
            return GetCoords(pos.X, pos.Y);
        }

        public Vector2 GetCoords(float x, float y)
        {
            return new Vector2 {
                X = x / _ActualSize,
                Y = y / _ActualSize
            };
        }

        protected override void Load()
        {
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            BitmapData data = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            Bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        }
    }
}
