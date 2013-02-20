using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework
{
    public class Program : GameWindow
    {
        private bool _captureMouse;

        private Camera _camera;

        private ModelShader _modelShader;
        private DepthClipShader _depthClipShader;
        private WaterShader _waterShader;
        private SkyShader _skyShader;

        private World _world;

        private Random _rand;

        private Stopwatch _timer;
        private double _lastFPSUpdate;
        private int _frameCount;

        private bool _wireframe;
        private bool _drawShip;
        private bool _firstPerson;

        private float _cameraDist;

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

            VSync = VSyncMode.Off;

            _rand = new Random();

            _timer = new Stopwatch();
            _timer.Start();

            _wireframe = false;
            _drawShip = true;
            _firstPerson = false;

            _cameraDist = 24f;

            _camera = new Camera(Width, Height);
            _camera.Pitch = 0.0f;
            _camera.Yaw = 0.0f;
            _camera.Position = new Vector3(-8f, 0f, 0f);
            _camera.UpdateViewMatrix();

            _world = new World();

            _depthClipShader = new DepthClipShader();
            _depthClipShader.Camera = _camera;
            _modelShader = new ModelShader();
            _modelShader.Camera = _camera;
            _modelShader.World = _world;
            _waterShader = new WaterShader();
            _waterShader.Camera = _camera;
            _waterShader.World = _world;
            _skyShader = new SkyShader();
            _skyShader.Camera = _camera;
            _skyShader.World = _world;

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

            Mouse.WheelChanged += (sender, mwe) => {
                if (!_firstPerson) {
                    _cameraDist -= mwe.DeltaPrecise;
                    _cameraDist = Tools.Clamp(_cameraDist, 6f, 28f);
                }
            };

            Keyboard.KeyDown += (sender, ke) => {
                switch (ke.Key) {
                    case Key.L:
                        _wireframe = !_wireframe; break;
                    case Key.B:
                        _drawShip = !_drawShip; break;
                    case Key.V:
                        _firstPerson = !_firstPerson; break;
                    default:
                        _world.KeyDown(ke.Key);
                        break;
                }
            };

            Keyboard.KeyUp += (sender, ke) => {
                _world.KeyUp(ke.Key);
            };

            GL.ClearColor(Color4.White);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _skyShader.Render();

            if (_drawShip) {
                _modelShader.StartBatch();
                _world.Render(_modelShader);
                _modelShader.EndBatch();

                _depthClipShader.StartBatch();
                _world.Render(_depthClipShader);
                _depthClipShader.EndBatch();
            }

            _waterShader.StartBatch();
            _world.Render(_waterShader);
            _waterShader.EndBatch();
            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

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
            }

            _camera.Rotation += new Vector2(_world.Ship.Pitch, _world.Ship.Yaw);
            _world.UpdateFrame(_timer.Elapsed.TotalSeconds, Keyboard);
            _camera.Rotation -= new Vector2(_world.Ship.Pitch, _world.Ship.Yaw);

            if (_firstPerson) {
                _camera.Position = _world.Ship.Position + _world.Ship.Up * 3f - _world.Ship.Forward * 2f;
            } else {
                _camera.Position = _world.Ship.Position - _camera.ViewVector * _cameraDist;
                if (_camera.Position.Y < 1f) {
                    _camera.Position = new Vector3(_camera.Position.X, 1f, _camera.Position.Z);
                }
            }
            _camera.UpdateViewMatrix();
        }
    }
}
