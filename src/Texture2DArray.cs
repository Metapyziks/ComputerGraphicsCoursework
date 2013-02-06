using System;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class Texture2DArray : Texture
    {
        private String[] _Names;
        private BitmapTexture2D[] _Textures;

        private UInt32[] _Data;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }

        public Texture2DArray(int width, int height, params BitmapTexture2D[] textures)
            : base(TextureTarget.Texture2DArray)
        {
            Width = width;
            Height = height;

            _Textures = textures;

            Count = 1;
            while (Count < textures.Length)
                Count <<= 1;

            int tileLength = width * height;

            _Data = new uint[tileLength * Count];

            for (int i = 0; i < _Textures.Length; ++i) {
                Bitmap tile = _Textures[i].Bitmap;

                int xScale = tile.Width / width;
                int yScale = tile.Height / height;

                for (int x = 0; x < width; ++x) {
                    for (int y = 0; y < height; ++y) {
                        int tx = x * xScale;
                        int ty = y * yScale;

                        Color clr = tile.GetPixel(tx, ty);

                        _Data[i * tileLength + x + y * width]
                            = (UInt32) (clr.R << 24 | clr.G << 16 | clr.B << 08 | clr.A << 00);
                    }
                }
            }
        }

        public ushort GetTextureIndex(String texture)
        {
            return (ushort) Array.IndexOf(_Names, texture);
        }

        protected override void Load()
        {
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
                Width, Height, Count, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, _Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            _Data = null;
        }
    }
}
