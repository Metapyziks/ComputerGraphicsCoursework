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
        private float[,] _velocity;
        private Random _rand;
        private double _lastSim;

        public Water(float size)
        {
            Size = size;

            _vb = new VertexBuffer(3);
            Resolution = 64;
            int mid = Resolution >> 1;

            float[] data = new float[4 * 3 * 64 * 64];
            int i = 0;
            for (int x = 0; x < Resolution; ++x) {
                for (int y = 0; y < Resolution; ++y) {
                    data[i++] = (x - mid) * size / 64f;
                    data[i++] = 0f;
                    data[i++] = (y - mid) * size / 64f;
                    data[i++] = (x - mid + 1) * size / 64f;
                    data[i++] = 0f;
                    data[i++] = (y - mid) * size / 64f;
                    data[i++] = (x - mid + 1) * size / 64f;
                    data[i++] = 0f;
                    data[i++] = (y - mid + 1) * size / 64f;
                    data[i++] = (x - mid) * size / 64f;
                    data[i++] = 0f;
                    data[i++] = (y - mid + 1) * size / 64f;
                }
            }

            _vb.SetData(data);

            _rand = new Random();
            _wavemap = new LumTexture2D(Resolution, Resolution);
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
            _velocity[x, y] = -magnitude;
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
                    float av = (_wavemap[l, y] + _wavemap[r, y] + _wavemap[x, t] + _wavemap[x, b]);
                    av += 0.7f * (_wavemap[l, t] + _wavemap[r, t] + _wavemap[l, b] + _wavemap[r, b]);
                    _velocity[x, y] += ((av / 6.8f) - _wavemap[x, y]) / 32f;
                }
            }

            for (int x = 0; x < Resolution; ++x) {
                for (int y = 0; y < Resolution; ++y) {
                    _wavemap[x, y] += _velocity[x, y];
                    _velocity[x, y] *= 0.99f;
                }
            }
        }

        public void Render(WaterShader shader)
        {
            shader.SetTexture("wavemap", _wavemap);
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }
    }
}
