using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Input;
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

            var lastMouseX = Cursor.Position.X;
            var lastMouseY = Cursor.Position.Y;
            Mouse.Move += (sender, me) => {
                if (!Focused || !_captureMouse) return;
                if (lastMouseX == Cursor.Position.X && lastMouseY == Cursor.Position.Y) return;

                _camera.Yaw += (Cursor.Position.X - lastMouseX) / 360f;
                _camera.Pitch += (Cursor.Position.Y - lastMouseY) / 360f;
                _camera.Pitch = Tools.Clamp(_camera.Pitch, -MathHelper.PiOver2, MathHelper.PiOver2);
                _camera.UpdateViewMatrix();

                Cursor.Position = new System.Drawing.Point(Bounds.Left + Width / 2, Bounds.Top + Height / 2);
                lastMouseX = Cursor.Position.X;
                lastMouseY = Cursor.Position.Y;
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
            
            if (!Focused || !_captureMouse) {
                Cursor.Show();
            } else {
                Cursor.Hide();

                Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);
                float angleY = _camera.Yaw;
                float angleX = _camera.Pitch;

                if (Keyboard[Key.D]) {
                    movement.X += (float) Math.Cos(angleY);
                    movement.Z += (float) Math.Sin(angleY);
                }
                if (Keyboard[Key.A]) {
                    movement.X -= (float) Math.Cos(angleY);
                    movement.Z -= (float) Math.Sin(angleY);
                }
                if (Keyboard[Key.S]) {
                    movement.Z += (float) Math.Cos(angleY) * (float) Math.Cos(angleX);
                    movement.Y += (float) Math.Sin(angleX);
                    movement.X -= (float) Math.Sin(angleY) * (float) Math.Cos(angleX);
                }
                if (Keyboard[Key.W]) {
                    movement.Z -= (float) Math.Cos(angleY) * (float) Math.Cos(angleX);
                    movement.Y -= (float) Math.Sin(angleX);
                    movement.X += (float) Math.Sin(angleY) * (float) Math.Cos(angleX);
                }

                if (movement.Length != 0) {
                    movement.Normalize();
                    _camera.Position += movement / 8f;
                    _camera.UpdateViewMatrix();
                }
            }
        }
    }
}
