using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using ComputerGraphicsCoursework.Scene;
using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework
{
    /// <summary>
    /// Main window class for the app. Includes the entry point for the application.
    /// </summary>
    public class Program : GameWindow
    {
        private static String _sResourceDirectory = "res";

        /// <summary>
        /// Prepends a resource file name with the resource directory path.
        /// </summary>
        /// <param name="fileName">File name of a resource to locate</param>
        /// <returns>Path to the resource</returns>
        public static String GetResourcePath(String fileName)
        {
            return _sResourceDirectory + "/" + fileName;
        }

        /// <summary>
        /// Entry point of the application.
        /// 
        /// Creates a new instance of Program, runs it until the application ends,
        /// and then disposes any used resources.
        /// </summary>
        /// <param name="args">Accepts at most one argument; a path to the resource directory</param>
        static void Main(String[] args)
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            if (args.Length > 0) {
                _sResourceDirectory = args[0].TrimEnd('/', '\\');
            } else {
                int depth = 0;
                while (!Directory.Exists(_sResourceDirectory) && depth++ < 4) {
                    _sResourceDirectory = "../" + _sResourceDirectory;
                }
            }

            var program = new Program();
            program.Run();
            program.Dispose();
        }

        private bool _captureMouse;

        private Camera _camera;

        private ModelShader _modelShader;
        private DepthClipShader _depthClipShader;
        private WaterShader _waterShader;
        private SkyShader _skyShader;

        private World _world;

        private Stopwatch _timer;
        private double _lastFPSUpdate;
        private int _frameCount;

        private bool _wireframe;
        private bool _drawShip;
        private bool _firstPerson;

        private float _cameraDist;

        /// <summary>
        /// Constructor to create a new Program instance.
        /// Sets the default resolution, colour depth, and sample quality of the app.
        /// </summary>
        public Program() : base(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 0), 16, 0, 2))
        {
            this.Title = "Computer Graphics Coursework";
        }

        /// <summary>
        /// Creates a new instance of the specified shader type, and gives it
        /// references to any needed information about the environment.
        /// </summary>
        /// <typeparam name="T">Shader type to set up</typeparam>
        /// <returns>New instance of T</returns>
        private T SetupShader<T>()
            where T : ShaderProgram, new()
        {
            T shader = new T();

            // If the shader is a ShaderProgram3D, give it a reference to the camera
            if (shader is ShaderProgram3D) {
                ((ShaderProgram3D) (ShaderProgram) shader).Camera = _camera;
            }

            // If the shader is a WorldAwareShader, give it a reference to the world
            if (shader is WorldAwareShader) {
                ((WorldAwareShader) (ShaderProgram) shader).World = _world;
            }

            return shader;
        }

        /// <summary>
        /// Method override that is invoked when the OpenGL context loads.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Don't limit the frame rate to the monitor's
            VSync = VSyncMode.Off;

            // Create and start a stopwatch for timekeeping
            _timer = new Stopwatch();
            _timer.Start();
            
            // Start with wireframe mode off, the ship visible, and with a
            // first person camera
            _wireframe = false;
            _drawShip = true;
            _firstPerson = true;

            // Start with the camera 24 units away from the ship when in
            // third person mode
            _cameraDist = 24f;

            // Initialize a camera which holds the perspective and
            // view matrices, to be used when drawing the scene
            _camera = new Camera(Width, Height);

            // Create a world object to store all scene elements
            _world = new World();

            // Set up each shader to be used when drawing the scene
            _depthClipShader = SetupShader<DepthClipShader>();
            _modelShader = SetupShader<ModelShader>();
            _waterShader = SetupShader<WaterShader>();
            _skyShader = SetupShader<SkyShader>();

            // Initially ignore mouse input
            _captureMouse = false;

            // Mouse look implementation
            Mouse.Move += (sender, me) => {
                // Find the middle of the window
                Point centre = new Point(Bounds.Left + Width / 2, Bounds.Top + Height / 2);

                // If the window is not in focus, the mouse is not locked, or the cursor
                // hasn't moved since the last movement, return without moving the camera
                if (!Focused || !_captureMouse) return;
                if (Cursor.Position.X == centre.X && Cursor.Position.Y == centre.Y) return;

                // Rotate the camera's yaw and pitch proportionally to how much the mouse has
                // moved in the X and Y axis respectively
                _camera.Yaw += (Cursor.Position.X - centre.X) / 360f;
                _camera.Pitch += (Cursor.Position.Y - centre.Y) / 360f;

                // Make sure the camera doesn't go upside-down
                _camera.Pitch = Tools.Clamp(_camera.Pitch, -MathHelper.PiOver2, MathHelper.PiOver2);

                // Instruct the camera to rebuild its view matrix with the new rotation
                _camera.UpdateViewMatrix();

                // Move the cursor back to the middle of the window
                Cursor.Position = centre;
            };

            // Third person zoom implementation
            Mouse.WheelChanged += (sender, mwe) => {
                // Don't zoom if the view is in first person mode
                if (_firstPerson) return;

                // Zoom in proportionally to how much the mouse wheel has scrolled
                _cameraDist -= mwe.DeltaPrecise;
                    
                // Don't zoom in closer than 6 units, or further than 28
                _cameraDist = Tools.Clamp(_cameraDist, 6f, 28f);
            };

            // Enable mouse look by clicking on the window
            Mouse.ButtonUp += (sender, me) => {
                // Ignore if mouse look is aleady enabled
                if (_captureMouse) return;

                // Enable mouse look and hide the cursor
                _captureMouse = true;
                Cursor.Hide();
            };

            Keyboard.KeyDown += (sender, ke) => {
                switch (ke.Key) {
                    case Key.L:
                        _wireframe = !_wireframe; break;
                    case Key.B:
                        _drawShip = !_drawShip; break;
                    case Key.V:
                        _firstPerson = !_firstPerson; break;
                    case Key.Escape:
                        _captureMouse = !_captureMouse;
                        if (_captureMouse) Cursor.Hide(); else Cursor.Show();
                        break;
                    case Key.Enter:
                        if (Keyboard[Key.AltLeft]) {
                            if (WindowState != WindowState.Fullscreen) {
                                WindowBorder = WindowBorder.Hidden;
                                WindowState = WindowState.Fullscreen;
                            } else {
                                WindowBorder = WindowBorder.Resizable;
                                WindowState = WindowState.Normal;
                            }
                        }
                        break;
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

        /// <summary>
        /// Method override that is invoked when the window is resized.
        /// </summary>=
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle);
            if (_camera != null) {
                _camera.SetScreenSize(Width, Height);
            }
        }

        /// <summary>
        /// Method override that is invoked when a frame is drawn.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _skyShader.Render();

            if (_drawShip) {
                _world.Render(_modelShader);
                _world.Render(_depthClipShader);
            }

            _world.Render(_waterShader);
            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            SwapBuffers();
            ++_frameCount;
        }

        /// <summary>
        /// Method override that is invoked when a frame is updated.
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (_timer.Elapsed.TotalSeconds - _lastFPSUpdate > 0.5) {
                Title = string.Format("FPS: {0:F2}", _frameCount / (_timer.Elapsed.TotalSeconds - _lastFPSUpdate));
                _lastFPSUpdate = _timer.Elapsed.TotalSeconds;
                _frameCount = 0;
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
