using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;

namespace ComputerGraphicsCoursework
{
    class Floater : IRenderable<ModelShader>, IUpdateable
    {
        private static Model _sModel;

        private bool _inWater;
        
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }

        public Floater(Vector3 position)
        {
            if (_sModel == null) {
                _sModel = Model.FromFile("../../res/sphere.obj");
            }

            _inWater = false;

            Position = position;
            Velocity = new Vector3();
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

                accel.X += info.X * depth / 8f;
                accel.Y += depth / 64f;
                accel.Z += info.Z * depth / 8f;
                
                Velocity *= 1f - depth * 0.12f;

                if (!_inWater) {
                    water.Splash(new Vector2(Position.X, Position.Z), Math.Min(1.0f, Math.Abs(Velocity.Y) / 4f + 1f / 16f));
                }
                _inWater = true;
            } else {
                accel.Y -= 1f / 128f;
                Velocity *= 0.99f;

                _inWater = false;
            }
            Velocity += accel;

            Position += Velocity;
        }
    }
}
