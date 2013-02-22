using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Interface for scene objects that accept key input.
    /// </summary>
    public interface IKeyControllable
    {
        void KeyDown(Key key);
        void KeyUp(Key key);

        void UpdateKeys(KeyboardDevice keyboard);
    }

    /// <summary>
    /// Interface for scene objects that may be drawn with a shader
    /// of a specified type.
    /// </summary>
    /// <typeparam name="T">Type of shader</typeparam>
    public interface IRenderable<T>
        where T : ShaderProgram
    {
        void Render(T shader);
    }

    /// <summary>
    /// Interface for scene objects that require a state update regularly.
    /// </summary>
    public interface IUpdateable
    {
        void Update(double time, Water water);
    }

    /// <summary>
    /// Class representing a scene containing a ship and a whole load of water.
    /// Also stores the current light direction and a skybox texture.
    /// </summary>
    public sealed class World : IDisposable
    {
        #region Private Static Fields
        private static CubeMapTexture _sDefaultSkyCubeMap;
        #endregion

        #region Private Fields
        private List<IRenderable<ModelShader>> _modelRenderables;
        private List<IRenderable<DepthClipShader>> _dcRenderables;
        
        private double _lastUpdate;

        private List<IUpdateable> _updateables;
        private List<IKeyControllable> _keyControllables;
        #endregion

        /// <summary>
        /// A player-controllable ship entity.
        /// </summary>
        public Ship Ship { get; private set; }

        /// <summary>
        /// A dynamic water plane.
        /// </summary>
        public Water Water { get; private set; }

        /// <summary>
        /// Current light direction as a normalized vector.
        /// </summary>
        public Vector3 LightDirection { get; set; }

        /// <summary>
        /// Current sky cubemap texture.
        /// </summary>
        public CubeMapTexture Skybox { get; set; }

        /// <summary>
        /// Constructor to create a new World instance.
        /// </summary>
        public World()
        {
            // Create lists of scene elements implementing the various
            // scene interfaces
            _modelRenderables = new List<IRenderable<ModelShader>>();
            _dcRenderables = new List<IRenderable<DepthClipShader>>();

            _updateables = new List<IUpdateable>();
            _keyControllables = new List<IKeyControllable>();

            // Add the ship and water plane to the world
            Ship = Add(new Ship());
            Water = new Water();

            // Set up light direction from a default pitch and yaw
            const float sunPitch = -60f * MathHelper.Pi / 180f;
            const float sunYaw = 225f * MathHelper.Pi / 180f;

            LightDirection = new Vector3((float) (Math.Cos(sunPitch) * Math.Cos(sunYaw)),
                (float) Math.Sin(sunPitch), (float) (Math.Cos(sunPitch) *(float)  Math.Sin(sunYaw)));
            LightDirection /= LightDirection.Length;

            // Load the default skybox if it hasn't already been, and set it as
            // the current one for this world
            Skybox = _sDefaultSkyCubeMap = _sDefaultSkyCubeMap ??
                CubeMapTexture.FromFiles(Program.GetResourcePath("stormydays_{0}.png"));
        }

        /// <summary>
        /// Add a scene element to any relevant interface lists.
        /// </summary>
        /// <typeparam name="T">Type of the scene element to add</typeparam>
        /// <param name="obj">Scene element to add</param>
        /// <returns>The given scene element, for convenience</returns>
        public T Add<T>(T obj)
        {
            // For each scene element interface the element implements,
            // add it to the corresponding list. I could probably use
            // reflection here if I was expecting to add more interfaces
            if (obj is IRenderable<ModelShader>) {
                _modelRenderables.Add((IRenderable<ModelShader>) obj);
            }
            if (obj is IRenderable<DepthClipShader>) {
                _dcRenderables.Add((IRenderable<DepthClipShader>) obj);
            }
            if (obj is IUpdateable) {
                _updateables.Add((IUpdateable) obj);
            }
            if (obj is IKeyControllable) {
                _keyControllables.Add((IKeyControllable) obj);
            }

            // Return the original object for convenience
            return obj;
        }

        /// <summary>
        /// Distribute a key down event to all scene elements that care.
        /// </summary>
        /// <param name="key">Recently pressed key</param>
        public void KeyDown(Key key)
        {
            foreach (var obj in _keyControllables) {
                obj.KeyDown(key);
            }
        }

        /// <summary>
        /// Distribute a key up event to all scene elements that care.
        /// </summary>
        /// <param name="key">Recently released key</param>
        public void KeyUp(Key key)
        {
            foreach (var obj in _keyControllables) {
                obj.KeyUp(key);
            }
        }
    
        /// <summary>
        /// Draw all scene elements that can be rendered with a ModelShader.
        /// </summary>
        /// <param name="shader">Shader to use when drawing</param>
        public void Render(ModelShader shader)
        {
            foreach (var obj in _modelRenderables) obj.Render(shader);
        }

        /// <summary>
        /// Draw all scene elements that can be rendered with a DepthClipShader.
        /// </summary>
        /// <param name="shader">Shader to use when drawing</param>
        public void Render(DepthClipShader shader)
        {
            foreach (var obj in _dcRenderables) obj.Render(shader);
        }

        /// <summary>
        /// Draw the world's water plane.
        /// </summary>
        /// <param name="shader">Shader to use when drawing</param>
        public void Render(WaterShader shader)
        {
            Water.Render(shader);
        }

        /// <summary>
        /// Update the states of all updateable elements in the scene.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="keyboard"></param>
        public void UpdateFrame(double time, KeyboardDevice keyboard)
        {
            // Check to see if an update is due
            if (time - _lastUpdate <= 1.0 / 60.0) return;
            _lastUpdate = time;

            // Update all updateable scene elements
            foreach (var obj in _updateables) obj.Update(time, Water);

            // Also alert all key controllable elements with as to the
            // current keyboard state
            foreach (var obj in _keyControllables) obj.UpdateKeys(keyboard);

            // Run a water physics simulation step
            Water.SimulateWater();
        }

        /// <summary>
        /// Dispose of any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Water.Dispose();
        }
    }
}
