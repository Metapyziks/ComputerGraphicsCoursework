using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class Program : GameWindow
    {
        private SpriteShader _spriteShader;
        private Sprite _testSprite;

        private Camera _camera;

        private TestShader _testShader;
        private Model _testModel;

        static void Main(String[] args)
        {
            var program = new Program();
            program.Run();
            program.Dispose();
        }

        public Program() : base(800, 600)
        {
            this.Title = "Computer Graphics Coursework";

            GL.ClearColor(Color4.CornflowerBlue);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _spriteShader = new SpriteShader(Width, Height);
            var texture = new BitmapTexture2D((Bitmap) Bitmap.FromFile("../../res/test.png"));
            _testSprite = new Sprite(texture);

            _camera = new Camera(Width, Height);
            _camera.Position = new Vector3(-4f, 0f, 0f);

            _testShader = new TestShader();
            _testShader.Camera = _camera;
            _testModel = Model.FromFile("../../res/boat.obj");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _spriteShader.Begin();
            _testSprite.Render(_spriteShader);
            _spriteShader.End();

            _testShader.Begin();
            _testModel.Render(_testShader);
            _testShader.End();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            var rot = _camera.Rotation;
            rot.X += (float) e.Time * MathHelper.PiOver2;
            _camera.Rotation = rot;
        }
    }
}
