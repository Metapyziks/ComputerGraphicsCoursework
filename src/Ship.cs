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
        private Model.FaceGroup _waterclipFaceGroup;
        private Model.FaceGroup[] _visibleFaceGroups;
        private Matrix4 _trans;

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Position { get; private set; }

        public Ship()
        {
            _hull = Model.FromFile("../../res/boat.obj");
            _waterclipFaceGroup = _hull.FaceGroups.First(x => x.Name == "Waterclip_waterclip");
            _visibleFaceGroups = _hull.FaceGroups.Where(x => x != _waterclipFaceGroup).ToArray();

            _trans = Matrix4.Identity;
        }

        public void Update(double time, Water water)
        {
            float offset = (float) Math.Sin(Math.PI * time) * MathHelper.Pi / 24f;
            float offset2 = (float) Math.Sin(Math.PI * (0.725 + time)) * MathHelper.Pi / 24f;
            _trans = Matrix4.CreateTranslation(0f, 0.0f, offset2 * 16f + 128f);
            _trans = Matrix4.Mult(Matrix4.CreateRotationY(-offset), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationX(offset), _trans);
            _trans = Matrix4.Mult(_trans, Matrix4.CreateRotationY((float) (Math.PI * time / 15f)));

            Position = Vector4.Transform(new Vector4(0f, 0f, 0f, 1f), _trans).Xyz;
            Forward = Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            Right = Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            Up = Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;

            if (water != null) {
                var splashPos = Position + Forward * 2f;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
                splashPos += -Forward * 6f - Right;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
                splashPos += Right * 2f;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
            }
        }

        public void Render(ModelShader shader)
        {
            shader.Transform = _trans;
            _hull.Render(shader, _visibleFaceGroups);
        }

        public void Render(DepthClipShader shader)
        {
            shader.Transform = _trans;
            _hull.Render(shader, _waterclipFaceGroup);
        }
    }
}
