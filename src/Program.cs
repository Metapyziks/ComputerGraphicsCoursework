using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class Program : GameWindow
    {
        private bool _captureMouse;

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
            _camera.Pitch = 0.0f;
            _camera.Yaw = 0.0f;
            _camera.Position = new Vector3(-8f, 0f, 0f);

            _testShader = new TestShader();
            _testShader.Camera = _camera;
            _testModel = Model.FromFile("../../res/boat.obj");

            _captureMouse = true;

            var ignoreMouse = false;
            Mouse.Move += (sender, me) => {
                if (!Focused || !_captureMouse) {
                    return;
                }

                if (ignoreMouse) {
                    ignoreMouse = false;
                    return;
                }

                Vector2 rot = _camera.Rotation;

                _camera.Yaw += me.XDelta / 180.0f;
                _camera.Pitch += me.YDelta / 180.0f;
                _camera.Pitch = Tools.Clamp(rot.X, -MathHelper.PiOver2, (float) MathHelper.PiOver2);

                ignoreMouse = true;
                Cursor.Position = new System.Drawing.Point(Bounds.Left + Width / 2, Bounds.Top + Height / 2);
            };
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

            if (!Focused || !_captureMouse) {
                Cursor.Show();
            } else {
                Cursor.Hide();
            }
        }
    }
}
