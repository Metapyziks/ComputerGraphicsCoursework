using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class Program : GameWindow
    {
        private bool _captureMouse;

        private Camera _camera;

        private ModelShader _modelShader;
        private DepthClipShader _depthClipShader;
        private Ship _ship;

        private WaterShader _waterShader;
        private Water _water;

        private Random _rand;

        private Stopwatch _timer;
        private double _lastFPSUpdate;
        private int _frameCount;

        static void Main(String[] args)
        {
            var program = new Program();
            program.Run();
            program.Dispose();
        }

        public Program() : base(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 0), 16, 0, 2))
        {
            this.Title = "Computer Graphics Coursework";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color4.White);

            VSync = VSyncMode.Off;

            _rand = new Random();

            _timer = new Stopwatch();
            _timer.Start();

            _camera = new Camera(Width, Height);
            _camera.Pitch = 0.0f;
            _camera.Yaw = 0.0f;
            _camera.Position = new Vector3(-8f, 0f, 0f);
            _camera.UpdateViewMatrix();

            _modelShader = new ModelShader();
            _modelShader.Camera = _camera;
            _depthClipShader = new DepthClipShader();
            _depthClipShader.Camera = _camera;
            _ship = new Ship();

            _waterShader = new WaterShader();
            _waterShader.Camera = _camera;
            _water = new Water(64f);

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
            
            _modelShader.StartBatch();
            _ship.Render(_modelShader);
            _modelShader.EndBatch();

            _depthClipShader.StartBatch();
            _ship.Render(_depthClipShader);
            _depthClipShader.EndBatch();

            _waterShader.StartBatch();
            _water.Render(_waterShader);
            _waterShader.EndBatch();

            SwapBuffers();
            ++_frameCount;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (_timer.Elapsed.TotalSeconds - _lastFPSUpdate > 0.5) {
                Title = string.Format("FPS: {0:F2}", _frameCount / (_timer.Elapsed.TotalSeconds - _lastFPSUpdate));
                _lastFPSUpdate = _timer.Elapsed.TotalSeconds;
                _frameCount = 0;
            }

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
                    _camera.Position += movement * (float) (16d * e.Time);
                    _camera.UpdateViewMatrix();
                }
            }

            _ship.Update(_timer.Elapsed.TotalSeconds, _water);

            _camera.Position = _ship.Position - _camera.ViewVector * 12f;
            _camera.UpdateViewMatrix();

            _water.SimulateWater(_timer.Elapsed.TotalSeconds);
        }
    }
}
