using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
{
    class Water : IRenderable<WaterShader>
    {
        public readonly float Size;

        private VertexBuffer _vb;
        private LumTexture2D _wavemap;

        public Water(float size)
        {
            Size = size;

            _vb = new VertexBuffer(3);
            int resolution = 64;
            int mid = resolution >> 1;

            float[] data = new float[4 * 3 * 64 * 64];
            int i = 0;
            for (int x = 0; x < resolution; ++x) {
                for (int y = 0; y < resolution; ++y) {
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

            Random rand = new Random();

            _wavemap = new LumTexture2D(64, 64);
            for (int x = 0; x < 64; ++x) {
                for (int y = 0; y < 64; ++y) {
                    _wavemap[x, y] = (byte) rand.Next(0, 256);
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
