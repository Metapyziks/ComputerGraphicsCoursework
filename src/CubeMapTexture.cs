using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
{
    public class CubeMapTexture : Texture
    {
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
        {
            _faces = new Bitmap[] {
                right, left, top, bottom, back, front
            };
        }

        protected override void Load()
        {
 	        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            BitmapData data = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            Bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Tools.ErrorCheck("loadtexture");
        }
    }
}
