using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class Water : IRenderable<WaterShader>
    {
        public const int Resolution = 512;
        public const double SimulationPeriod = 1.0 / 60.0;

        private static readonly WaterSimulateSprayShader _sSimSprayShader;
        private static readonly WaterSimulateVelocityShader _sSimVelocityShader;
        private static readonly WaterSimulateHeightShader _sSimHeightShader;

        private static readonly WaterSplashVelocityShader _sSplashVelocityShader;

        private static readonly float[] _sVerts;
        private static readonly VertexBuffer _sVB;

        static Water()
        {
            _sSimSprayShader = new WaterSimulateSprayShader();
            _sSimVelocityShader = new WaterSimulateVelocityShader();
            _sSimHeightShader = new WaterSimulateHeightShader();

            _sSplashVelocityShader = new WaterSplashVelocityShader();

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

            _heightmapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution, 0.5f));
            _velocitymapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution, 0.5f));
            _spraymapBuffer = new FrameBuffer(new LumTexture2D(Resolution, Resolution, 0.0f));
        }

        public void Splash(Vector2 pos, float magnitude)
        {
            _sSplashVelocityShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            _sSplashVelocityShader.SplashPosition = pos;
            _sSplashVelocityShader.SplashMagnitude = magnitude;

            _velocitymapBuffer.Begin();
            _sSplashVelocityShader.Begin();
            _sSplashVelocityShader.Render();
            _sSplashVelocityShader.End();
            _velocitymapBuffer.End();
        }

        public void SimulateWater(double time)
        {
            if (time - _lastSim < SimulationPeriod) return;
            _lastSim = time;

            _sSimSprayShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            
            _spraymapBuffer.Begin();
            _sSimSprayShader.Begin();
            _sSimSprayShader.Render();
            _sSimSprayShader.End();
            _spraymapBuffer.End();

            _sSimVelocityShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);

            _velocitymapBuffer.Begin();
            _sSimVelocityShader.Begin();
            _sSimVelocityShader.Render();
            _sSimVelocityShader.End();
            _velocitymapBuffer.End();

            _sSimHeightShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);

            _heightmapBuffer.Begin();
            _sSimHeightShader.Begin();
            _sSimHeightShader.Render();
            _sSimHeightShader.End();
            _heightmapBuffer.End();
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
