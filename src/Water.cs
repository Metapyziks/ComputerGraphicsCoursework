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

            Func<double, int> sizeCalc = x => Math.Max(1, (int) Math.Floor((x - 64.0) / 16.0));
            int length = FindWaterDataLength(1024, sizeCalc);

            _sVerts = new float[4 * 2 * length];
            FindWaterData(1024, sizeCalc, _sVerts);

            _sVB = new VertexBuffer(2);
            _sVB.SetData(_sVerts);
        }

        private static int FindWaterDataLength(int size, Func<double, int> sizeCalc)
        {
            return FindWaterDataLength(size, -(size >> 1), -(size >> 1), sizeCalc);
        }

        private static int FindWaterDataLength(int size, int x, int y, Func<double, int> sizeCalc)
        {
            int half = size >> 1;
            double dist = Math.Sqrt((x + half) * (x + half) + (y + half) * (y + half));
            int desired = sizeCalc(dist);
            if (size > 1 && size > desired) {
                return FindWaterDataLength(half, x, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y + half, sizeCalc)
                    + FindWaterDataLength(half, x, y + half, sizeCalc);
            }

            return 1;
        }

        private static void FindWaterData(int size, Func<double, int> sizeCalc, float[] buffer)
        {
            int i = 0;
            FindWaterData(size, size, -(size >> 1), -(size >> 1), sizeCalc, buffer, ref i);
        }

        private static void FindWaterData(int totalSize, int size, int x, int y, Func<double, int> sizeCalc, float[] buffer, ref int i)
        {
            int half = size >> 1;
            double dist = Math.Sqrt((x + half) * (x + half) + (y + half) * (y + half));
            int desired = sizeCalc(dist);
            if (size > 1 && size > desired) {
                FindWaterData(totalSize, half, x, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x + half, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x + half, y + half, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x, y + half, sizeCalc, buffer, ref i);
            } else {
                buffer[i++] = (float) (x + 0000) / totalSize;
                buffer[i++] = (float) (y + 0000) / totalSize;
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y + 0000) / totalSize;
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y + size) / totalSize;
                buffer[i++] = (float) (x + 0000) / totalSize;
                buffer[i++] = (float) (y + size) / totalSize;
            }
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

            for (int i = 0; i < 16; ++i) {
                Splash(new Vector2((float) _rand.NextDouble() * 512f, (float) _rand.NextDouble() * 512f), (float) _rand.NextDouble() / 8f);
            }         

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
