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
        #region Private Static Fields
        private static String _sResourceDirectory = "res";
        #endregion

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
        /// </summary>
        /// <param name="args">Accepts at most one argument; a path to the resource directory</param>
        public static void Main(String[] args)
        {
            // Set the working directory to be the one containing this executable, so
            // that relative paths will be relative to the location of the app
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            if (args.Length > 0) {
                _sResourceDirectory = args[0].TrimEnd('/', '\\');
            } else {
                int depth = 0;
                while (!Directory.Exists(_sResourceDirectory) && depth++ < 4) {
                    _sResourceDirectory = "../" + _sResourceDirectory;
                }
            }

            // Create a new instance of the app
            var program = new Program();

            // Initiate the rendering and updating loop
            program.Run();

            // At program exit, dispose all unmanaged resources
            program.Dispose();
        }

        #region Private Fields
        private Camera _camera;

        private ModelShader _modelShader;
        private DepthClipShader _depthClipShader;
        private WaterShader _waterShader;
        private SkyShader _skyShader;

        private World _world;

        private Stopwatch _timer;
        private double _lastFPSUpdate;
        private int _frameCount;

        private bool _captureMouse;
        private bool _wireframe;
        private bool _drawModels;
        private bool _firstPerson;

        private float _cameraDist;
        #endregion

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

            // Initially ignore mouse input
            _captureMouse = false;

            // Start with wireframe mode off, all models visible, and with a
            // first person camera
            _wireframe = false;
            _drawModels = true;
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

            // Key press event handler
            Keyboard.KeyDown += (sender, ke) => {
                switch (ke.Key) {
                    // L toggles wireframe mode
                    case Key.L: 
                        _wireframe = !_wireframe; break;

                    // B toggles rendering of models
                    case Key.B: 
                        _drawModels = !_drawModels; break;

                    // V toggles first person view
                    case Key.V:
                        _firstPerson = !_firstPerson; break;

                    // Escape disables mouse look and shows the cursor
                    case Key.Escape:
                        _captureMouse = !_captureMouse;
                        if (_captureMouse) Cursor.Hide(); else Cursor.Show();
                        break;

                    // Alt+Enter toggles fullscreen mode
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

                    // Delegate any other keys to any scene elements that accept key input
                    default: 
                        _world.KeyDown(ke.Key);
                        break;
                }
            };

            // Key release event handler
            Keyboard.KeyUp += (sender, ke) => {
                // Delegate to any scene elements that accept key input
                _world.KeyUp(ke.Key);
            };
        }

        /// <summary>
        /// Method override that is invoked when the window is resized.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Update the size of the viewport OpenGL draws to
            GL.Viewport(ClientRectangle);
            if (_camera != null) {
                // Update the perspective matrix in the camera with the
                // new aspect ratio
                _camera.SetScreenSize(Width, Height);
            }
        }

        /// <summary>
        /// Method override that is invoked when a frame is drawn.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Clear the depth buffer
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // If wireframe mode is enabled, switch to only draw the perimiter lines
            // of each primitive
            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // Draw the sky
            _skyShader.Render();

            // If model drawing is enabled, draw any scene elements that contain a model
            // and also draw any depth clip faces that hide water drawn behind them
            if (_drawModels) {
                _world.Render(_modelShader);
                _world.Render(_depthClipShader);
            }

            // Draw the water
            _world.Render(_waterShader);

            // Make sure that line drawing mode is disabled if it was originally enabled
            if (_wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // Swap the back and front buffers to show the frame that was just drawn
            SwapBuffers();

            // Increment the FPS counter
            ++_frameCount;
        }

        /// <summary>
        /// Method override that is invoked when a frame is updated.
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // If it has been more than half a second since the last FPS update, calculate
            // the new FPS, display it in the window title, and reset the counter.
            if (_timer.Elapsed.TotalSeconds - _lastFPSUpdate > 0.5) {
                Title = string.Format("FPS: {0:F2}", _frameCount / (_timer.Elapsed.TotalSeconds - _lastFPSUpdate));
                _lastFPSUpdate = _timer.Elapsed.TotalSeconds;
                _frameCount = 0;
            }

            // To keep camera rotation relative to the ship's rotation, first rotate it
            // to be relative to the world before updating the ship
            _camera.Rotation += new Vector2(_world.Ship.Pitch, _world.Ship.Yaw);

            // Update any dynamic scene elements, which includes the ship and the water
            _world.UpdateFrame(_timer.Elapsed.TotalSeconds, Keyboard);

            // Now rotate the camera to be relative to the new ship rotation
            _camera.Rotation -= new Vector2(_world.Ship.Pitch, _world.Ship.Yaw);

            if (_firstPerson) {
                // In first person view, move the camera to be inside the ship
                _camera.Position = _world.Ship.Position + _world.Ship.Up * 3f - _world.Ship.Forward * 2f;
            } else {
                // In third person view, move the camera so it is orbiting the ship
                _camera.Position = _world.Ship.Position - _camera.ViewVector * _cameraDist;
                if (_camera.Position.Y < 1f) {
                    // Ensure the camera is above the water at all times
                    _camera.Position = new Vector3(_camera.Position.X, 1f, _camera.Position.Z);
                }
            }
        }
    }
}
