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

        private VertexBuffer _vb;
        private LumTexture2D _wavemap;
        private LumTexture2D _spraymap;
        private float[,] _velocity;
        private Random _rand;
        private double _lastSim;

        public Water(float size)
        {
            Size = size;

            _vb = new VertexBuffer(2);
            Resolution = 128;
            int mid = Resolution >> 1;

            float[] data = new float[4 * 2 * Resolution * Resolution];
            int i = 0;
            for (int x = 0; x < Resolution; ++x) {
                for (int y = 0; y < Resolution; ++y) {
                    float xv = (float) x / Resolution;
                    float yv = (float) y / Resolution;
                    data[i++] = xv + 0;
                    data[i++] = yv + 0;
                    data[i++] = xv + 1;
                    data[i++] = yv + 0;
                    data[i++] = xv + 1;
                    data[i++] = yv + 1;
                    data[i++] = xv + 0;
                    data[i++] = yv + 1;
                }
            }

            _vb.SetData(data);

            _rand = new Random();
            _wavemap = new LumTexture2D(Resolution, Resolution);
            _spraymap = new LumTexture2D(Resolution, Resolution);
            _velocity = new float[Resolution, Resolution];
            for (int x = 0; x < Size; ++x) {
                for (int y = 0; y < Size; ++y) {
                    _wavemap[x, y] = 0.5f;
                    _velocity[x, y] = (float) (_rand.NextDouble() - 0.5) / 16f;
                }
            }
        }

        public void Splash(Vector2 pos, float magnitude)
        {
            int x = (int) Math.Round((pos.X / Size + 0.5f) * Resolution);
            int y = (int) Math.Round((pos.Y / Size + 0.5f) * Resolution);
            while (x < 0) x += Resolution;
            while (x >= Resolution) x -= Resolution;
            while (y < 0) y += Resolution;
            while (y >= Resolution) y -= Resolution;
            //_velocity[x, y] = -magnitude;
            _wavemap[x, y] = -magnitude;
            _spraymap[x, y] = Math.Abs(magnitude) * 8f;
        }

        public void SimulateWater(double time)
        {
            if (time - _lastSim < SimulationPeriod) return;
            _lastSim = time;

            for (int x = 0; x < Resolution; ++x) {
                int l = (x - 1 < 0 ? Resolution - 1 : x - 1);
                int r = (x + 1 == Resolution ? 0 : x + 1);
                for (int y = 0; y < Resolution; ++y) {
                    int t = (y - 1 < 0 ? Resolution - 1 : y - 1);
                    int b = (y + 1 == Resolution ? 0 : y + 1);
                    float av = 0.5f + _wavemap[l, y] + _wavemap[r, y] + _wavemap[x, t] + _wavemap[x, b];
                    av += 0.7f * (_wavemap[l, t] + _wavemap[r, t] + _wavemap[l, b] + _wavemap[r, b]);
                    _velocity[x, y] += ((av / 7.8f) - _wavemap[x, y]) / 16f;
                    _velocity[x, y] = Tools.Clamp(_velocity[x, y], -1f / 32f, 1f / 32f);
                    av = 0.3f + _spraymap[l, y] + _spraymap[r, y] + _spraymap[x, t] + _spraymap[x, b];
                    av += 0.7f * (_spraymap[l, t] + _spraymap[r, t] + _spraymap[l, b] + _spraymap[r, b]);
                    _spraymap[x, y] += Math.Max((av / 7.8f - _spraymap[x, y]), 0f);
                }
            }

            for (int x = 0; x < Resolution; ++x) {
                for (int y = 0; y < Resolution; ++y) {
                    _wavemap[x, y] += _velocity[x, y];
                    _velocity[x, y] *= 0.993f;
                    _spraymap[x, y] *= 0.996f;
                }
            }
        }

        public void Render(WaterShader shader)
        {
            shader.SetTexture("wavemap", _wavemap);
            shader.SetTexture("spraymap", _spraymap);
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }
    }
}
