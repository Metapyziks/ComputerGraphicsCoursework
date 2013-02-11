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

            Func<int, int, int> sizeCalc = (x, y) => {
                if (x < -32) return 0;
                if (x > 32) {
                    if (Math.Abs(Math.Atan2(y, x + 24)) > Math.PI / 4.0) return 0;
                    return Math.Max(1, ((x - 32) * (x - 32)) >> 11);
                }
                if (Math.Abs(Math.Atan2(y, 32 + 24)) > Math.PI / 4.0) return 0;
                return 1;
            };

            int meshDetail = 1024;
            int length = FindWaterDataLength(meshDetail, sizeCalc);

            _sVerts = new float[4 * 2 * length];
            FindWaterData(meshDetail, sizeCalc, _sVerts);

            _sVB = new VertexBuffer(2);
            _sVB.SetData(_sVerts);
        }

        private static int FindWaterDataLength(int size, Func<int, int, int> sizeCalc)
        {
            return FindWaterDataLength(size, -(size >> 1), -(size >> 1), sizeCalc);
        }

        private static int FindWaterDataLength(int size, int x, int y, Func<int, int, int> sizeCalc)
        {
            int half = size >> 1;
            int desired = sizeCalc(x < 0 ? x + size : x, y < 0 ? y + size : y);
            if (size > 1 && size > desired) {
                return FindWaterDataLength(half, x, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y + half, sizeCalc)
                    + FindWaterDataLength(half, x, y + half, sizeCalc);
            }

            if (desired <= 0)
                return 0;

            return 1;
        }

        private static void FindWaterData(int size, Func<int, int, int> sizeCalc, float[] buffer)
        {
            int i = 0;
            FindWaterData(size, size, -(size >> 1), -(size >> 1), sizeCalc, buffer, ref i);
        }

        private static void FindWaterData(int totalSize, int size, int x, int y, Func<int, int, int> sizeCalc, float[] buffer, ref int i)
        {
            int half = size >> 1;
            int desired = sizeCalc(x < 0 ? x + size : x, y < 0 ? y + size : y);
            if (size > 1 && size > desired) {
                FindWaterData(totalSize, half, x + half, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x + half, y + half, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x, y + half, sizeCalc, buffer, ref i);
            } else if (desired > 0) {
                buffer[i++] = (float) (x + 0000) / totalSize;
                buffer[i++] = (float) (y + 0000) / totalSize;
                //if ((x & (size << 1)) == 0 || sizeCalc(x + size, y) < (size << 1)) {
                    buffer[i++] = (float) (x + size) / totalSize;
                    buffer[i++] = (float) (y + 0000) / totalSize;

                    buffer[i++] = (float) (x + size) / totalSize;
                    buffer[i++] = (float) (y + size) / totalSize;
                //} else {
                //    int join = y & size;

                //    buffer[i++] = (float) (x + size) / totalSize;
                //    buffer[i++] = (float) (y - join) / totalSize;

                //    buffer[i++] = (float) (x + size) / totalSize;
                //    buffer[i++] = (float) (y + join) / totalSize;
                //}

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

        private void NormalizePosition(ref float x, ref float z)
        {
            x = (x / 128f + 0.5f) * Resolution;
            z = (z / 128f + 0.5f) * Resolution;

            x -= (float) (Math.Floor(x / Resolution) * Resolution);
            z -= (float) (Math.Floor(z / Resolution) * Resolution);
        }

        public Vector3 GetSurfaceInfo(Vector3 pos)
        {
            return GetSurfaceInfo(pos.X, pos.Z);
        }

        public Vector3 GetSurfaceInfo(Vector2 pos)
        {
            return GetSurfaceInfo(pos.X, pos.Y);
        }

        private float InterpolateHeight(float[,] heights, float x, float z)
        {
            int xi = (int) Math.Floor(x), zi = (int) Math.Floor(z);
            x -= xi; z -= zi;

            float l = (1f - z) * heights[xi, zi] + z * heights[xi, zi + 1];
            float t = (1f - x) * heights[xi, zi] + x * heights[xi + 1, zi];
            float r = (1f - z) * heights[xi + 1, zi] + z * heights[xi + 1, zi + 1];
            float b = (1f - x) * heights[xi, zi + 1] + x * heights[xi + 1, zi + 1];

            return ((1f - x) * l + x * r) * 2f - 1f;
        }

        private float[,] heightBuffer = new float[4, 4];
        public Vector3 GetSurfaceInfo(float x, float z)
        {
            NormalizePosition(ref x, ref z);

            _heightmapBuffer.Begin();
            GL.ReadPixels((int) x - 1, (int) z - 1, 4, 4, PixelFormat.Alpha, PixelType.Float, heightBuffer);
            _heightmapBuffer.End();

            x -= (float) Math.Floor(x) - 1f;
            z -= (float) Math.Floor(z) - 1f;

            float c = InterpolateHeight(heightBuffer, x, z);
            float l = InterpolateHeight(heightBuffer, x - 1f, z);
            float t = InterpolateHeight(heightBuffer, x, z - 1f);
            float r = InterpolateHeight(heightBuffer, x + 1f, z);
            float b = InterpolateHeight(heightBuffer, x, z + 1f);

            return new Vector3(t - b, c, l - r);
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
