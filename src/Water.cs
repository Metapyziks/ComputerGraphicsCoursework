using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace ComputerGraphicsCoursework
{
    class Water : IRenderable<WaterShader>
    {
        public const double SimulationPeriod = 1.0 / 60.0;

        public readonly float Size;
        public readonly int Resolution;

        private WaterSimulationShader _simShader;
        private WaterSplashShader _splashShader;

        private VertexBuffer _vb;
        private FrameBuffer _wavemap;
        private LumTexture2D _spraymap;
        private float[,] _velocity;
        private Random _rand;
        private double _lastSim;

        public Water(float size)
        {
            Size = size;

            _vb = new VertexBuffer(2);
            Resolution = 512;
            int mid = Resolution >> 1;

            float[] data = new float[4 * 2 * Resolution * Resolution];
            int i = 0;
            for (int x = 0; x < Resolution; ++x) {
                for (int y = 0; y < Resolution; ++y) {
                    float x0 = (float) x / Resolution - 0.5f;
                    float y0 = (float) y / Resolution - 0.5f;
                    float x1 = (float) (x + 1) / Resolution - 0.5f;
                    float y1 = (float) (y + 1) / Resolution - 0.5f;
                    data[i++] = x0;
                    data[i++] = y0;
                    data[i++] = x1;
                    data[i++] = y0;
                    data[i++] = x1;
                    data[i++] = y1;
                    data[i++] = x0;
                    data[i++] = y1;
                }
            }

            _vb.SetData(data);

            _rand = new Random();
            _wavemap = new FrameBuffer(new BitmapTexture2D(Resolution, Resolution));
            _spraymap = new LumTexture2D(Resolution, Resolution);
            _velocity = new float[Resolution, Resolution];

            _simShader = new WaterSimulationShader(Resolution, Resolution);
            _simShader.WaveMap = _wavemap.Texture;

            _splashShader = new WaterSplashShader(Resolution, Resolution);
            _splashShader.WaveMap = _wavemap.Texture;
        }

        public void Splash(Vector2 pos, float magnitude)
        {
            _splashShader.SplashPosition = pos;
            _splashShader.SplashMagnitude = magnitude;

            _wavemap.Begin();
            _splashShader.Begin();
            _splashShader.Render();
            _splashShader.End();
            _wavemap.End();
        }

        public void SimulateWater(double time)
        {
            if (time - _lastSim < SimulationPeriod) return;
            _lastSim = time;

            _wavemap.Begin();
            _simShader.Begin();
            _simShader.Render();
            _simShader.End();
            _wavemap.End();
        }

        public void Render(WaterShader shader)
        {
            shader.SetTexture("wavemap", _wavemap.Texture);
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }
    }
}
