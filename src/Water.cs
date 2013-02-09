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
        public const int Resolution = 512;
        public const double SimulationPeriod = 1.0 / 60.0;

        private static readonly WaterSprayDissipationShader _sSimShader;
        private static readonly WaterSplashShader _sSplashShader;

        private static readonly float[] _sVerts;
        private static readonly VertexBuffer _sVB;

        static Water()
        {
            _sSimShader = new WaterSprayDissipationShader();
            _sSplashShader = new WaterSplashShader();

            // TODO: Improve so is more detailed near centre and uses less verts
            _sVerts = new float[4 * 2 * Resolution * Resolution];
            int i = 0;
            for (int x = 0; x < Resolution; ++x) {
                float x0 = (float) x / Resolution - 0.5f;
                float x1 = (float) (x + 1) / Resolution - 0.5f;
                for (int y = 0; y < Resolution; ++y) {
                    float y0 = (float) y / Resolution - 0.5f;
                    float y1 = (float) (y + 1) / Resolution - 0.5f;
                    _sVerts[i++] = x0;
                    _sVerts[i++] = y0;
                    _sVerts[i++] = x1;
                    _sVerts[i++] = y0;
                    _sVerts[i++] = x1;
                    _sVerts[i++] = y1;
                    _sVerts[i++] = x0;
                    _sVerts[i++] = y1;
                }
            }

            _sVB = new VertexBuffer(2);
            _sVB.SetData(_sVerts);
        }

        public readonly float Size;
        
        private FrameBuffer _heightmapBuffer;
        private FrameBuffer _velocitymapBuffer;
        private FrameBuffer _spraymapBuffer;
        private Random _rand;
        private double _lastSim;

        public Water(float size)
        {
            Size = size;

            _rand = new Random();
            _heightmapBuffer = new FrameBuffer(new BitmapTexture2D(Resolution, Resolution));

            _heightmapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution));
            _velocitymapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution));
            _spraymapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution));
        }

        public void Splash(Vector2 pos, float magnitude)
        {
            _sSplashShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            _sSplashShader.SplashPosition = pos;
            _sSplashShader.SplashMagnitude = magnitude;

            _spraymapBuffer.Begin();
            _sSplashShader.Begin();
            _sSplashShader.Render();
            _sSplashShader.End();
            _spraymapBuffer.End();
        }

        public void SimulateWater(double time)
        {
            if (time - _lastSim < SimulationPeriod) return;
            _lastSim = time;

            _spraymapBuffer.Begin();
            _sSimShader.Begin();
            _sSimShader.Render();
            _sSimShader.End();
            _spraymapBuffer.End();
        }

        public void Render(WaterShader shader)
        {
            shader.SetTexture("heightmap", _heightmapBuffer.Texture);
            shader.SetTexture("spraymap", _spraymapBuffer.Texture);
            _sVB.StartBatch(shader);
            _sVB.Render(shader);
            _sVB.EndBatch(shader);
        }
    }
}
