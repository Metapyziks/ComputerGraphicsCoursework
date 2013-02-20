using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework
{
    class World : IRenderable<ModelShader>, IRenderable<DepthClipShader>, IRenderable<WaterShader>
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

            float sunPitch = 35f;
            float sunYaw = 165f;

            LightDirection = new Vector3((float) (Math.Cos(sunPitch) * Math.Cos(sunYaw)),
                (float) Math.Sin(sunPitch), (float) (Math.Cos(sunPitch) *(float)  Math.Sin(sunYaw)));
            LightDirection /= LightDirection.Length;

            Skybox = _sDefaultSkyCubeMap = _sDefaultSkyCubeMap ?? CubeMapTexture.FromFiles("../../res/sky_{0}.png");
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
    }
}
