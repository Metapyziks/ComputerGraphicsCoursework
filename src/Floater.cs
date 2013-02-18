using System;

using OpenTK;
using OpenTK.Graphics;

using ComputerGraphicsCoursework.Shaders;

namespace ComputerGraphicsCoursework
{
    class Floater : IRenderable<ModelShader>, IUpdateable
    {
        private static Model _sModel;
        
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; private set; }

        public Floater(Vector3 position)
        {
            if (_sModel == null) {
                _sModel = Model.FromFile("../../res/sphere.obj");
            }

            Position = position;
            Velocity = new Vector3();
        }

        public void Accelerate(Vector3 accel)
        {
            Velocity += accel;
        }

        public void Streamline(Vector3 direction, float weight)
        {
            Vector3 normal = Velocity;
            normal.Normalize();
            Velocity *= (1f - weight) + weight * Math.Abs(Vector3.Dot(normal, direction));
        }

        public void Render(ModelShader shader)
        {
            shader.Transform = Matrix4.CreateTranslation(Position);
            shader.Colour = Color4.Red;
            shader.Shinyness = 2f;
            _sModel.Render(shader);
        }

        public void Update(double time, Water water)
        {
            Vector3 info = water.GetSurfaceInfo(Position);
            Vector3 accel = new Vector3();
            if (info.Y > Position.Y) {
                float depth = Math.Min(1.0f, info.Y - Position.Y);

                accel.X += info.X * depth / 64f;
                accel.Y += depth / 128f;
                accel.Z += info.Z * depth / 64f;
                
                Velocity *= 1f - depth * 0.03f;
            } else {
                accel.Y -= 1f / 128f;
                Velocity *= 0.99f;
            }
            Accelerate(accel);

            Position += Velocity;
        }
    }
}
