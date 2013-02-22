using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class representing an infinite plane of dynamic water.
    /// </summary>
    public sealed class Water : IRenderable<WaterShader>, IDisposable
    {
        /// <summary>
        /// Width and height of the height, velocity and spray textures used to
        /// simulate the water dynamics.
        /// </summary>
        public const int Resolution = 512;

        #region Private Static Fields
        private static readonly WaterSimulateSprayShader _sSimSprayShader;
        private static readonly WaterSimulateVelocityShader _sSimVelocityShader;
        private static readonly WaterSimulateHeightShader _sSimHeightShader;

        private static readonly WaterDepressVelocityShader _sDepressVelocityShader;

        private static readonly float[] _sVerts;
        private static readonly VertexBuffer _sVB;
        #endregion

        /// <summary>
        /// Static constructor for the Water class. Creates the shaders used
        /// to simulate the water, and builds the water mesh.
        /// </summary>
        static Water()
        {
            // Set up the three simulation shaders
            _sSimSprayShader = new WaterSimulateSprayShader();
            _sSimVelocityShader = new WaterSimulateVelocityShader();
            _sSimHeightShader = new WaterSimulateHeightShader();

            // Set up the splash shader
            _sDepressVelocityShader = new WaterDepressVelocityShader();

            // Function deciding the minimum size of a quad given its position
            Func<int, int, int> sizeCalc = (x, y) => {
                // If the quad is behind the camera, discard
                if (x < -32) return 0;

                // If the quad is in front of the area seen when looking
                // directly down...
                if (x > 32) {
                    // If the quad is outside the player's field of view, discard
                    if (Math.Abs(Math.Atan2(y, x + 24)) > Math.PI / 4.0) return 0;

                    // Otherwise, return a number that gets gradually smaller
                    // for quads further from the camera
                    return Math.Max(1, ((x - 32) * (x - 32)) >> 11);
                }

                // If the quad is outside the area seen when looking directly
                // down, discard
                if (Math.Abs(Math.Atan2(y, 32 + 24)) > Math.PI / 4.0) return 0;

                // Otherwise, use the smallest size possible for a high
                // detail area
                return 1;
            };

            // Width and height of the initial water quad before subdivision
            int meshDetail = 1024;

            // Find the number of quads given a size and size criteria
            int length = FindWaterDataLength(meshDetail, sizeCalc);
            
            // Find the actual quad vertex data
            _sVerts = new float[4 * 2 * length];
            FindWaterData(meshDetail, sizeCalc, _sVerts);

            // Store in a VBO for speedy rendering
            _sVB = new VertexBuffer(2);
            _sVB.SetData(_sVerts);
        }

        /// <summary>
        /// Given an initial quad size and size calculation function, find the number
        /// of quads to be created when generating a water mesh.
        /// </summary>
        /// <param name="size">Size of the initial water mesh quad</param>
        /// <param name="sizeCalc">Function deciding the minimum size of a quad given its position</param>
        /// <returns>The number of quads to be created</returns>
        private static int FindWaterDataLength(int size, Func<int, int, int> sizeCalc)
        {
            // Start recursing using the initial quad
            return FindWaterDataLength(size, -(size >> 1), -(size >> 1), sizeCalc);
        }

        /// <summary>
        /// Given a quad size, position, and size calculation function, find
        /// the number of quads to be created when generating a water mesh.
        /// </summary>
        /// <param name="size">Size of the current water mesh quad</param>
        /// <param name="x">Horizontal position of the quad</param>
        /// <param name="y">Vertical position of the quad</param>
        /// <param name="sizeCalc">Function deciding the minimum size of a quad given its position</param>
        /// <returns>The number of quads to be created</returns>
        private static int FindWaterDataLength(int size, int x, int y, Func<int, int, int> sizeCalc)
        {
            int half = size >> 1;

            // Determine the maximum size for this quad
            int desired = sizeCalc(x < 0 ? x + size : x, y < 0 ? y + size : y);

            // If the quad is too big, and should be subdivided...
            if (size > 1 && size > desired) {
                // Recurse for the four child quadrants
                return FindWaterDataLength(half, x, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y, sizeCalc)
                    + FindWaterDataLength(half, x + half, y + half, sizeCalc)
                    + FindWaterDataLength(half, x, y + half, sizeCalc);
            }

            // If the size calculation returned 0, discard
            if (desired <= 0) return 0;

            // Otherwise, this is a leaf of the quadtree
            return 1;
        }

        /// <summary>
        /// Given an initial quad size and size calculation function, find the vertex
        /// data for the quads in a water mesh.
        /// </summary>
        /// <param name="size">Size of the initial water mesh quad</param>
        /// <param name="sizeCalc">Function deciding the minimum size of a quad given its position</param>
        /// <param name="buffer">Array to store the vertex data in</param>
        private static void FindWaterData(int size, Func<int, int, int> sizeCalc, float[] buffer)
        {
            int i = 0;

            // Start recursing using the initial quad
            FindWaterData(size, size, -(size >> 1), -(size >> 1), sizeCalc, buffer, ref i);
        }

        /// <summary>
        /// Given a quad size, position, and size calculation function, find
        /// the vertex data for the quads in a water mesh.
        /// </summary>
        /// <param name="totalSize">Size of the initial water mesh quad</param>
        /// <param name="size">Size of the current water mesh quad</param>
        /// <param name="x">Horizontal position of the quad</param>
        /// <param name="y">Vertical position of the quad</param>
        /// <param name="sizeCalc">Function deciding the minimum size of a quad given its position</param>
        /// <param name="buffer">Array to store the vertex data in</param>
        /// <param name="i">Index pointing to where to write in <paramref name="buffer"/></param>
        private static void FindWaterData(int totalSize, int size, int x, int y, Func<int, int, int> sizeCalc, float[] buffer, ref int i)
        {
            int half = size >> 1;

            // Determine the maximum size for this quad
            int desired = sizeCalc(x < 0 ? x + size : x, y < 0 ? y + size : y);

            // If the quad is too big, and should be subdivided...
            if (size > 1 && size > desired) {
                // Recurse for the four child quadrants
                FindWaterData(totalSize, half, x + half, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x + half, y + half, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x, y, sizeCalc, buffer, ref i);
                FindWaterData(totalSize, half, x, y + half, sizeCalc, buffer, ref i);
                return;
            }

            // If the size calculation returned 0, discard
            if (desired <= 0) return;

            // Add the position for the front left vertex
            buffer[i++] = (float) (x + 0000) / totalSize;
            buffer[i++] = (float) (y + 0000) / totalSize;

            // If the quad doesn't need to be stitched...
            if ((x & size) == 0 || sizeCalc(x + size, y) < (size << 1)) {
                // Add the position for the back left vertex
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y + 0000) / totalSize;

                // Add the position for the back right vertex
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y + size) / totalSize;
            } else {
                // Determine if the quad should be stitched left or right
                int join = y & size;

                // Add the position for the back left vertex
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y - join) / totalSize;

                // Add the position for the back right vertex
                buffer[i++] = (float) (x + size) / totalSize;
                buffer[i++] = (float) (y + join) / totalSize;
            }

            // Add the position for the front right vertex
            buffer[i++] = (float) (x + 0000) / totalSize;
            buffer[i++] = (float) (y + size) / totalSize;
        }

        #region Private Fields
        private FrameBuffer _heightmapBuffer;
        private FrameBuffer _velocitymapBuffer;
        private FrameBuffer _spraymapBuffer;
        private Random _rand;

        private float[,] _heightBuffer;
        #endregion

        /// <summary>
        /// Constructor to create a new Water instance.
        /// </summary>
        public Water()
        {
            // Set up random number generator for random surface disturbance
            _rand = new Random();

            // Set up the three water texture frame buffers for use in the water
            // physics simulation
            _heightmapBuffer = new FrameBuffer(new AlphaTexture2D(Resolution, Resolution, 0.5f));
            _velocitymapBuffer = new FrameBuffer(new AlphaTexture2D(Resolution, Resolution, 0.5f));
            _spraymapBuffer = new FrameBuffer(new AlphaTexture2D(Resolution, Resolution, 0.0f));

            // Set up height buffer for use in finding surface data so that it doesn't
            // need to be constructed for each request
            _heightBuffer = new float[4, 4];
        }

        /// <summary>
        /// Given an X and Z coordinate in world-space, find the wrapped
        /// position in water texture space.
        /// </summary>
        /// <param name="x">Horizontal component of the position</param>
        /// <param name="z">Depth component of the position</param>
        private void NormalizePosition(ref float x, ref float z)
        {
            // Scale down to texture space
            x = (x / 128f + 0.5f) * Resolution;
            z = (z / 128f + 0.5f) * Resolution;

            // Wrap to be in the range of 0.0 to 1.0
            x -= (float) (Math.Floor(x / Resolution) * Resolution);
            z -= (float) (Math.Floor(z / Resolution) * Resolution);
        }

        /// <summary>
        /// Find the surface height and gradient at a given position.
        /// </summary>
        /// <param name="pos">The position in world-space</param>
        /// <returns>Surface height and gradient</returns>
        public Vector3 GetSurfaceInfo(Vector3 pos)
        {
            return GetSurfaceInfo(pos.X, pos.Z);
        }

        /// <summary>
        /// Find the surface height and gradient at a given position.
        /// </summary>
        /// <param name="pos">The X and Z coordinates in world-space</param>
        /// <returns>Surface height and gradient</returns>
        public Vector3 GetSurfaceInfo(Vector2 pos)
        {
            return GetSurfaceInfo(pos.X, pos.Y);
        }

        /// <summary>
        /// Given a subtexture from the height map, find a height value that
        /// is linearly interpolated from neighbouring heights.
        /// </summary>
        /// <param name="x">Horizontal index</param>
        /// <param name="z">Depth index</param>
        /// <returns>Linearly interpolated height</returns>
        private float InterpolateHeight(float x, float z)
        {
            // Find the integer indices in the height array
            int xi = (int) Math.Floor(x), zi = (int) Math.Floor(z);

            // Reduce X and Z to ratios between 0.0 and 1.0
            x -= xi; z -= zi;

            // Find the interpolated left and right heights
            float l = (1f - z) * _heightBuffer[xi, zi] + z * _heightBuffer[xi, zi + 1];
            float r = (1f - z) * _heightBuffer[xi + 1, zi] + z * _heightBuffer[xi + 1, zi + 1];

            // Interpolate between the left and right heights, normalize, and return
            return ((1f - x) * l + x * r) * 2f - 1f;
        }

        /// <summary>
        /// Find the surface height and gradient at a given position.
        /// </summary>
        /// <param name="x">Horizontal position in world-space</param>
        /// <param name="z">Depth position in world-space</param>
        /// <returns>Surface height and gradient</returns>
        public Vector3 GetSurfaceInfo(float x, float z)
        {
            // Transform x and z to texture space
            NormalizePosition(ref x, ref z);

            // Read a chunk from the height map to work out the interpolated
            // height at the given point, and the gradient at that point
            _heightmapBuffer.Begin();
            GL.ReadPixels((int) x - 1, (int) z - 1, 4, 4, PixelFormat.Alpha, PixelType.Float, _heightBuffer);
            _heightmapBuffer.End();

            // Reduce x and z to positions relative to the 4x4 height buffer
            x -= (float) Math.Floor(x) - 1f;
            z -= (float) Math.Floor(z) - 1f;

            // Find the height at the given position, and the four neighbouring
            // positions in each of the cardinal directions
            float c = InterpolateHeight(x, z);
            float l = InterpolateHeight(x - 1f, z);
            float t = InterpolateHeight(x, z - 1f);
            float r = InterpolateHeight(x + 1f, z);
            float b = InterpolateHeight(x, z + 1f);

            // Return the X and Z gradients, and height
            return new Vector3(t - b, c, l - r);
        }

        /// <summary>
        /// Depress the water at a given position by a specified amount.
        /// </summary>
        /// <param name="pos">Position to depress the water at</param>
        /// <param name="magnitude"></param>
        public void Depress(Vector3 pos, float magnitude)
        {
            Depress(new Vector2(pos.X, pos.Z), magnitude);
        }

        /// <summary>
        /// Depress the water at a given position by a specified amount.
        /// </summary>
        /// <param name="pos">Position to depress the water at</param>
        /// <param name="magnitude"></param>
        public void Depress(Vector2 pos, float magnitude)
        {
            // Prepare the depression shader with the three input textures and
            // information about where to depress the water and by how much
            _sDepressVelocityShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);
            _sDepressVelocityShader.DepressPosition = pos;
            _sDepressVelocityShader.DepressionMagnitude = magnitude;

            // Run a shader pass on the velocity map
            _velocitymapBuffer.Begin();
            _sDepressVelocityShader.Begin(true);
            _sDepressVelocityShader.Render();
            _sDepressVelocityShader.End();
            _velocitymapBuffer.End();
        }

        /// <summary>
        /// Simulate water dynamics.
        /// </summary>
        /// <param name="time"></param>
        public void SimulateWater()
        {
            // Randomly disturb some points on the surface to make some
            // high frequency turbulence
            for (int i = 0; i < 16; ++i) {
                Depress(new Vector2((float) _rand.NextDouble() * 512f, (float) _rand.NextDouble() * 512f), (float) _rand.NextDouble() / 8f);
            }         

            // Prepare the input textures for the spray dissipation shader
            _sSimSprayShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);

            // Run a shader pass on the spray map
            _spraymapBuffer.Begin();
            _sSimSprayShader.Begin(true);
            _sSimSprayShader.Render();
            _sSimSprayShader.End();
            _spraymapBuffer.End();
            
            // Prepare the input textures for the water acceleration shader
            _sSimVelocityShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);

            // Run a shader pass on the velocity map
            _velocitymapBuffer.Begin();
            _sSimVelocityShader.Begin(true);
            _sSimVelocityShader.Render();
            _sSimVelocityShader.End();
            _velocitymapBuffer.End();

            // Prepare the input textures for the water velocity shader
            _sSimHeightShader.SetTextures(_heightmapBuffer.Texture, _velocitymapBuffer.Texture, _spraymapBuffer.Texture);

            // Run a shader pass on the height map
            _heightmapBuffer.Begin();
            _sSimHeightShader.Begin(true);
            _sSimHeightShader.Render();
            _sSimHeightShader.End();
            _heightmapBuffer.End();
        }

        /// <summary>
        /// Draw the water to the screen using a given WaterShader.
        /// </summary>
        /// <param name="shader">Shader to use when drawing the water</param>
        public void Render(WaterShader shader)
        {
            // Set the two textures used when drawing the water
            shader.SetTexture("heightmap", _heightmapBuffer.Texture);
            shader.SetTexture("spraymap", _spraymapBuffer.Texture);

            // Draw the water mesh from the VBO
            _sVB.Begin(shader);
            _sVB.Render();
            _sVB.End();
        }

        /// <summary>
        /// Dispose of any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _sVB.Dispose();
            _heightmapBuffer.Dispose();
            _velocitymapBuffer.Dispose();
            _spraymapBuffer.Dispose();
        }
    }
}
