using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace ComputerGraphicsCoursework
{
    class Ship : IRenderable<ModelShader>
    {
        private Model _hull;
        private Matrix4 _trans;

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Position { get; private set; }

        public Ship()
        {
            _hull = Model.FromFile("../../res/boat.obj");
            _trans = Matrix4.Identity;
        }

        public void Update(double time)
        {
            _trans = Matrix4.CreateRotationY(MathHelper.Pi * (float) time / 10f);
            _trans = Matrix4.Mult(Matrix4.CreateTranslation(0f, 0f, 16f), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationX((float) Math.Sin(Math.PI * (float) time) * MathHelper.Pi / 16f), _trans);

            Position = Vector4.Transform(new Vector4(0f, 0f, 0f, 1f), _trans).Xyz;
            Forward = Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            Right = Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            Up = Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;
        }

        public void Render(ModelShader shader)
        {
            shader.Transform = _trans;
            _hull.Render(shader);
        }
    }
}
