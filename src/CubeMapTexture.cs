using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class CubeMapTexture : Texture
    {
        private static readonly String[] _sSuffixes = new String[] {
            "_rt", "_lf", "_up", "_dn", "_bk", "_ft"
        };

        private static readonly TextureTarget[] _sTargets = new TextureTarget[] {
            TextureTarget.TextureCubeMapPositiveX,
            TextureTarget.TextureCubeMapPositiveX,
        };

        public static CubeMapTexture FromFiles(String pathPrefix)
        {
            var bmps = _sSuffixes.Select(x => (Bitmap) Bitmap.FromFile(pathPrefix + x)).ToArray();
            return new CubeMapTexture(bmps[0], bmps[1], bmps[2], bmps[3], bmps[4], bmps[5]);
        }

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
        }

        protected override void Load()
        {
 	        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            for (int i = 0; i < 6; ++i) {
                Bitmap bmp = _faces[i];
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(_sTargets[i], 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Tools.ErrorCheck("loadtexture");
        }
    }
}
