using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework
{
    public class FrameBuffer : IDisposable
    {
        private int _fboID;

        public int FboID
        {
            get 
            {
                if (_fboID == 0) _fboID = GL.GenFramebuffer();

                return _fboID;
            }
        }

        public Texture Texture { get; private set; }
        
        public FrameBuffer(Texture tex)
        {
            Texture = tex;

            Texture.Bind();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tex.TextureTarget, tex.ID, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Tools.ErrorCheck("fbo_init");
        }

        public void Begin()
        {
            Texture.Bind();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboID);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.PushAttrib(AttribMask.ViewportBit);
            GL.Viewport(0, 0, Texture.Width, Texture.Height);

            Tools.ErrorCheck("fbo_begin");
        }

        public void End()
        {
            GL.PopAttrib();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Tools.ErrorCheck("fbo_end");
        }

        public void Dispose()
        {
            if (_fboID != 0) {
                GL.DeleteFramebuffer(_fboID);
                _fboID = 0;
            }
        }
    }
}
