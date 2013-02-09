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

        private static readonly WaterSprayDissipationShader _sSimSprayShader;
        private static readonly WaterSplashSprayShader _sSplashSprayShader;
        private static readonly WaterSplashHeightShader _sSplashHeightShader;

        private static readonly float[] _sVerts;
        private static readonly VertexBuffer _sVB;

        static Water()
        {
            _sSimSprayShader = new WaterSprayDissipationShader();
            _sSplashSprayShader = new WaterSplashSprayShader();
            _sSplashHeightShader = new WaterSplashHeightShader();

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
            _sSplashSprayShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            _sSplashSprayShader.SplashPosition = pos;
            _sSplashSprayShader.SplashMagnitude = magnitude;

            _spraymapBuffer.Begin();
            _sSplashSprayShader.Begin();
            _sSplashSprayShader.Render();
            _sSplashSprayShader.End();
            _spraymapBuffer.End();

            _sSplashHeightShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            _sSplashHeightShader.SplashPosition = pos;
            _sSplashHeightShader.SplashMagnitude = magnitude;

            _heightmapBuffer.Begin();
            _sSplashHeightShader.Begin();
            _sSplashHeightShader.Render();
            _sSplashHeightShader.End();
            _heightmapBuffer.End();
        }

        public void SimulateWater(double time)
        {
            if (time - _lastSim < SimulationPeriod) return;
            _lastSim = time;

            _spraymapBuffer.Begin();
            _sSimSprayShader.Begin();
            _sSimSprayShader.Render();
            _sSimSprayShader.End();
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
