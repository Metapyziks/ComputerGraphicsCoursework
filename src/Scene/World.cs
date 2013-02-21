using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework.Scene
{
    public interface IKeyControllable
    {
        void KeyDown(Key key);
        void KeyUp(Key key);

        void UpdateKeys(KeyboardDevice keyboard);
    }

    public interface IRenderable<T>
        where T : ShaderProgram
    {
        void Render(T shader);
    }

    public interface IUpdateable
    {
        void Update(double time, Water water);
    }

    public sealed class World : IDisposable
    {
        private static CubeMapTexture _sDefaultSkyCubeMap;

        private List<IRenderable<ModelShader>> _modelRenderables;
        private List<IRenderable<DepthClipShader>> _dcRenderables;
        
        private double _lastUpdate;

        private List<IUpdateable> _updateables;
        private List<IKeyControllable> _keyControllables;

        public Ship Ship { get; private set; }
        private Water _water;

        public Vector3 LightDirection { get; set; }
        public CubeMapTexture Skybox { get; set; }

        public World()
        {
            _modelRenderables = new List<IRenderable<ModelShader>>();
            _dcRenderables = new List<IRenderable<DepthClipShader>>();

            _updateables = new List<IUpdateable>();
            _keyControllables = new List<IKeyControllable>();

            Ship = Add(new Ship());
            _water = new Water(64f);

            float sunPitch = -60f * MathHelper.Pi / 180f;
            float sunYaw = 225f * MathHelper.Pi / 180f;

            LightDirection = new Vector3((float) (Math.Cos(sunPitch) * Math.Cos(sunYaw)),
                (float) Math.Sin(sunPitch), (float) (Math.Cos(sunPitch) *(float)  Math.Sin(sunYaw)));
            LightDirection /= LightDirection.Length;

            Skybox = _sDefaultSkyCubeMap = _sDefaultSkyCubeMap ?? CubeMapTexture.FromFiles(Program.GetResourcePath("stormydays_{0}.png"));
        }

        public T Add<T>(T obj)
        {
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
            return obj;
        }

        public void KeyDown(Key key)
        {
            foreach (var obj in _keyControllables) {
                obj.KeyDown(key);
            }
        }

        public void KeyUp(Key key)
        {
            foreach (var obj in _keyControllables) {
                obj.KeyUp(key);
            }
        }
    
        public void Render(ModelShader shader)
        {
            foreach (var obj in _modelRenderables) obj.Render(shader);
        }

        public void Render(DepthClipShader shader)
        {
            foreach (var obj in _dcRenderables) obj.Render(shader);
        }

        public void Render(WaterShader shader)
        {
            _water.Render(shader);
        }

        public void UpdateFrame(double time, KeyboardDevice keyboard)
        {
            if (time - _lastUpdate > 1.0 / 60.0) {
                _lastUpdate = time;
                foreach (var obj in _updateables) obj.Update(time, _water);
                foreach (var obj in _keyControllables) obj.UpdateKeys(keyboard);
            }

            _water.SimulateWater(time);
        }

        public void Dispose()
        {
            _water.Dispose();
        }
    }
}
