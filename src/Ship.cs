using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;

namespace ComputerGraphicsCoursework
{
    class Ship : IRenderable<ModelShader>, IUpdateable
    {
        private static Model _sModel;

        private Model.FaceGroup[] _waterclip;
        private Model.FaceGroup[] _innerHull;
        private Model.FaceGroup[] _outerHull;
        private Model.FaceGroup[] _trim;
        private Model.FaceGroup[] _motor;
        private Matrix4 _trans;

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Position { get; private set; }

        public Ship()
        {
            if (_sModel == null) {
                _sModel = Model.FromFile("../../res/boat.obj");
            }

            _waterclip = _sModel.GetFaceGroups("Waterclip");
            _innerHull = _sModel.GetFaceGroups("InnerHull");
            _outerHull = _sModel.GetFaceGroups("OuterHull");
            _trim = _sModel.GetFaceGroups("Trim");
            _motor = _sModel.GetFaceGroups("Motor");

            _trans = Matrix4.Identity;
        }

        public void Update(double time, Water water)
        {
            float offset = (float) Math.Sin(Math.PI * time) * MathHelper.Pi / 24f;
            float offset2 = (float) Math.Sin(Math.PI * (0.725 + time)) * MathHelper.Pi / 24f;
            _trans = Matrix4.CreateTranslation(0f, -0.5f, offset2 * 16f + 128f);
            _trans = Matrix4.Mult(Matrix4.CreateRotationY(-offset), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationX(offset), _trans);
            _trans = Matrix4.Mult(_trans, Matrix4.CreateRotationY((float) (Math.PI * time / 15f)));

            Position = Vector4.Transform(new Vector4(0f, 0f, 0f, 1f), _trans).Xyz;
            Forward = Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            Right = Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            Up = Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;

            if (water != null) {
                var splashPos = Position + Forward * 3.5f;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
                splashPos += -Forward * 7.5f - Right;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
                splashPos += Right * 2f;
                water.Splash(new Vector2(splashPos.X, splashPos.Z), 1f);
            }
        }

        public void Render(ModelShader shader)
        {
            shader.Transform = _trans;
            shader.Shinyness = 0f;
            shader.Colour = new Color4(121, 78, 47, 255);
            _sModel.Render(shader, _innerHull);
            shader.Colour = Color4.LightGray;
            shader.Shinyness = 8f;
            _sModel.Render(shader, _outerHull);
            shader.Colour = Color4.Gray;
            _sModel.Render(shader, _trim);
            shader.Colour = new Color4(32, 32, 32, 255);
            shader.Shinyness = 4f;
            _sModel.Render(shader, _motor);
        }

        public void Render(DepthClipShader shader)
        {
            shader.Transform = _trans;
            _sModel.Render(shader, _waterclip);
        }
    }
}
