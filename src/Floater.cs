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

        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }

        public Floater(Vector3 position)
        {
            if (_sModel == null) {
                _sModel = Model.FromFile("../../res/sphere.obj");
            }

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
                accel.X += info.X / 4f;
                accel.Y += 1f / 64f;
                accel.Z += info.Z / 4f;
                
                Velocity *= 0.93f;
            } else {
                accel.Y -= 1f / 128f;
                Velocity *= 0.97f;
            }
            Velocity += accel;

            Position += Velocity;
        }
    }
}
