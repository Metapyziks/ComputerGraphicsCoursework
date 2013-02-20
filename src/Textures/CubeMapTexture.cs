﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Textures
{
    public class CubeMapTexture : Texture
    {
        private static readonly String[] _sSuffixes = new String[] {
            "rt", "lf", "up", "dn", "bk", "ft"
        };

        private static readonly TextureTarget[] _sTargets = new TextureTarget[] {
            TextureTarget.TextureCubeMapPositiveZ,
            TextureTarget.TextureCubeMapNegativeZ,
            TextureTarget.TextureCubeMapPositiveY,
            TextureTarget.TextureCubeMapNegativeY,
            TextureTarget.TextureCubeMapNegativeX,
            TextureTarget.TextureCubeMapPositiveX
        };

        public static CubeMapTexture FromFiles(String pathFormat)
        {
            var bmps = _sSuffixes.Select(x => (Bitmap) Bitmap.FromFile(String.Format(pathFormat, x))).ToArray();
            return new CubeMapTexture(bmps[0], bmps[1], bmps[2], bmps[3], bmps[4], bmps[5]);
        }

        private readonly int _actualSize;
        private Bitmap[] _faces;

        public Bitmap Right  { get { return _faces[0]; } }
        public Bitmap Left   { get { return _faces[1]; } }
        public Bitmap Top    { get { return _faces[2]; } }
        public Bitmap Bottom { get { return _faces[3]; } }
        public Bitmap Back   { get { return _faces[4]; } }
        public Bitmap Front  { get { return _faces[5]; } }

        public CubeMapTexture(
            Bitmap right, Bitmap left,
            Bitmap top, Bitmap bottom,
            Bitmap back, Bitmap front)
            : base(TextureTarget.TextureCubeMap, right.Width, right.Height)
        {
            _faces = new Bitmap[] {
                right, left, top, bottom, back, front
            };

            _actualSize = _faces.Max(x => GetNextPOTS(x.Width, x.Height));

            for (int i = 0; i < 6; ++i) {
                if (_faces[i].Width != _actualSize || _faces[i].Height != _actualSize) {
                    Bitmap old = _faces[i];
                    Bitmap bmp = _faces[i] = new Bitmap(_actualSize, _actualSize);

                    for (int x = 0; x < Width; ++x)
                        for (int y = 0; y < Height; ++y)
                            bmp.SetPixel(x, y, old.GetPixel(x, y));
                }
            }
        }

        protected override void Load()
        {
            for (int i = 0; i < 6; ++i) {
                Bitmap bmp = _faces[i];
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, _actualSize, _actualSize), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(_sTargets[i], 0, PixelInternalFormat.Rgba, _actualSize, _actualSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);

            Tools.ErrorCheck("loadtexture");
        }
    }
}