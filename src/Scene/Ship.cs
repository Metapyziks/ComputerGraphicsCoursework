using System;
using System.Linq;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework.Scene
{
    public class Ship : IRenderable<ModelShader>, IRenderable<DepthClipShader>, IUpdateable, IKeyControllable
    {
        private const float RudderMoveSpeed = MathHelper.Pi / 120f;

        private static Model _sModel;
        private static BitmapTexture2D _sPlanksTexture;
        private static BitmapTexture2D _sFiberglassTexture;

        private Model.FaceGroup[] _waterclip;
        private Model.FaceGroup[] _innerHull;
        private Model.FaceGroup[] _outerHull;
        private Model.FaceGroup[] _trim;
        private Model.FaceGroup[] _motor;
        private Model.FaceGroup[] _prop;
        private Matrix4 _trans;

        private Floater _frontFloat;
        private Floater _leftFloat;
        private Floater _rightFloat;

        private float _rudderAng;
        private float _propSpeed;
        private float _propAng;

        public float Pitch { get; private set; }
        public float Yaw { get; private set; }
        public float Roll { get; private set; }
        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Position { get; private set; }

        public Ship()
        {
            if (_sModel == null) {
                _sModel = Model.FromFile("../../res/boat.obj");
                _sPlanksTexture = new BitmapTexture2D((Bitmap) Bitmap.FromFile("../../res/planks.png"));
                _sFiberglassTexture = new BitmapTexture2D((Bitmap) Bitmap.FromFile("../../res/fiberglass.png"));
            }

            _waterclip = _sModel.GetFaceGroups("Waterclip");
            _innerHull = _sModel.GetFaceGroups("InnerHull");
            _outerHull = _sModel.GetFaceGroups("OuterHull");
            _trim = _sModel.GetFaceGroups("Trim");
            _motor = _sModel.GetFaceGroups("Motor").Union(_sModel.GetFaceGroups("Tiller")).ToArray();
            _prop = _sModel.GetFaceGroups("Prop");

            _frontFloat = new Floater(new Vector3(6f, 0f, 0f));
            _leftFloat = new Floater(new Vector3(-6f, 0f, -4f));
            _rightFloat = new Floater(new Vector3(-6f, 0f, 4f));

            _trans = Matrix4.Identity;
        }

        public void Update(double time, Water water)
        {
            _frontFloat.Update(time, water);
            _leftFloat.Update(time, water);
            _rightFloat.Update(time, water);

            Vector3 aft = (_leftFloat.Position + _rightFloat.Position) / 2f;
            Position = (aft + _frontFloat.Position) / 2f;

            Vector3 diff = Position - aft;
            Pitch = (float) Math.Atan2(diff.Y, Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z));

            diff = _rightFloat.Position - _leftFloat.Position;
            Roll = (float) Math.Atan2(diff.Y, Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z));

            diff = _frontFloat.Position - aft;
            Yaw = (float) -Math.Atan2(diff.Z, diff.X);

            _trans = Matrix4.CreateTranslation(Position);
            _trans = Matrix4.Mult(Matrix4.CreateRotationY(Yaw), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationX(Roll), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationZ(Pitch), _trans);

            Forward = Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            Right = Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            Up = Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;

            float vel = (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity).Length / 3f;
            if (_rudderAng < 0) {
                _leftFloat.Accelerate(Right * vel * (-_rudderAng / MathHelper.PiOver4) / 64f);
            } else if (_rudderAng > 0) {
                _rightFloat.Accelerate(-Right * vel * (_rudderAng / MathHelper.PiOver4) / 64f);
            }

            _frontFloat.Accelerate((Position + Forward * 6f - _frontFloat.Position) / 128f);
            _leftFloat.Accelerate((Position - Forward * 6f - Right * 4f - _leftFloat.Position) / 128f);
            _rightFloat.Accelerate((Position - Forward * 6f + Right * 4f - _rightFloat.Position) / 128f);

            _frontFloat.Streamline(Forward, 0.02f);
            _leftFloat.Streamline(Forward, 0.01f);
            _rightFloat.Streamline(Forward, 0.01f);

            _propAng += _propSpeed / 60f;

            float mag = Math.Min(1f, (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity).Length / 12f);

            var splashPos = Position + Forward * 3.5f;
            for (int i = 0; i < 8; ++i) {
                splashPos = Position + Forward * (3.5f - i);
                water.Splash(new Vector2(splashPos.X, splashPos.Z), mag / 2f);
            }
            splashPos = Position - Forward * 3.5f - Right;
            water.Splash(new Vector2(splashPos.X, splashPos.Z), mag);
            splashPos += Right * 2f;
            water.Splash(new Vector2(splashPos.X, splashPos.Z), mag);
        }

        public void Render(ModelShader shader)
        {
            shader.Texture = _sPlanksTexture;
            shader.Transform = _trans;
            shader.Shinyness = 2f;
            shader.Colour = Color4.White;
            _sModel.Render(shader, _innerHull);
            shader.Colour = Color.CornflowerBlue;
            shader.Shinyness = 8f;
            _sModel.Render(shader, _outerHull);
            shader.Texture = _sFiberglassTexture;
            shader.Colour = Color4.White; // new Color4(64, 64, 64, 255);
            _sModel.Render(shader, _trim);
            shader.Colour = new Color4(32, 32, 32, 255);
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationY(_rudderAng), Matrix4.CreateTranslation(-4f, 0f, 0f)), _trans);
            shader.Shinyness = 4f;
            _sModel.Render(shader, _motor);
            shader.Colour = Color4.Gray;
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationX(_propAng), Matrix4.CreateTranslation(0f, -1f / 8f, 0f)),
                shader.Transform);
            shader.Shinyness = 4f;
            _sModel.Render(shader, _prop);

            //_frontFloat.Render(shader);
            //_leftFloat.Render(shader);
            //_rightFloat.Render(shader);
        }

        public void Render(DepthClipShader shader)
        {
            shader.Transform = _trans;
            _sModel.Render(shader, _waterclip);
        }

        public void KeyDown(Key key) { }

        public void KeyUp(Key key) { }

        public void UpdateKeys(KeyboardDevice keyboard)
        {
            _propSpeed *= 0.99f;

            if (keyboard[Key.W]) {
                _leftFloat.Accelerate(Forward / 128f);
                _rightFloat.Accelerate(Forward / 128f);
                _propSpeed += 1f / 4f;
            }

            if (keyboard[Key.S]) {
                _leftFloat.Accelerate(-Forward / 256f);
                _rightFloat.Accelerate(-Forward / 256f);
                _propSpeed -= 1f / 8f;
            }

            float vel = (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity).Length / 3f;
            float rudderVel = -_rudderAng * vel / 8f;
            if (keyboard[Key.A]) {
                rudderVel -= RudderMoveSpeed;
            }
            if (keyboard[Key.D]) {
                rudderVel += RudderMoveSpeed;
            }

            _rudderAng = Tools.Clamp(_rudderAng + rudderVel, -MathHelper.PiOver4, MathHelper.PiOver4);
        }
    }
}
