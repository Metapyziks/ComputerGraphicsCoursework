using System;
using System.Drawing;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class representing the moveable ship.
    /// </summary>
    public class Ship :
        IRenderable<ModelShader>,
        IRenderable<DepthClipShader>,
        IUpdateable, IKeyControllable
    {
        #region Private Static Fields
        private const float RudderMoveSpeed = MathHelper.Pi / 120f;

        private static Model _sModel;

        private Model.FaceGroup[] _sWaterclip;
        private Model.FaceGroup[] _sInnerHull;
        private Model.FaceGroup[] _sOuterHull;
        private Model.FaceGroup[] _sTrim;
        private Model.FaceGroup[] _sMotor;
        private Model.FaceGroup[] _sProp;

        private static BitmapTexture2D _sPlanksTexture;
        private static BitmapTexture2D _sFiberglassTexture;
        #endregion

        #region Private Fields
        private Matrix4 _trans;

        private Floater _frontFloat;
        private Floater _leftFloat;
        private Floater _rightFloat;

        private float _rudderAng;
        private float _propSpeed;
        private float _propAng;
        #endregion

        /// <summary>
        /// Rotation of the ship on the X axis.
        /// </summary>
        public float Pitch { get; private set; }

        /// <summary>
        /// Rotation of the ship on the Y axis.
        /// </summary>
        public float Yaw { get; private set; }

        /// <summary>
        /// Rotation of the ship on the Z axis.
        /// </summary>
        public float Roll { get; private set; }

        /// <summary>
        /// Normalized forward vector of the ship.
        /// </summary>
        public Vector3 Forward { get; private set; }

        /// <summary>
        /// Normalized right-hand vector of the ship.
        /// </summary>
        public Vector3 Right { get; private set; }

        /// <summary>
        /// Normalized up vector of the ship.
        /// </summary>
        public Vector3 Up { get; private set; }

        /// <summary>
        /// Position of the ship in the world.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Constructor to create a new Ship instance.
        /// </summary>
        public Ship()
        {
            // If the models and textures have never been loaded, load them and store them
            // in static fields for use in this ship and any others created later
            if (_sModel == null) {
                // Load the boat model
                _sModel = Model.FromFile(Program.GetResourcePath("boat.obj"));

                // Extract the various face groups from the ship model so they can be drawn
                // separately with different textures and material effects
                _sWaterclip = _sModel.GetFaceGroups("Waterclip");
                _sInnerHull = _sModel.GetFaceGroups("InnerHull");
                _sOuterHull = _sModel.GetFaceGroups("OuterHull");
                _sTrim = _sModel.GetFaceGroups("Trim");
                _sMotor = _sModel.GetFaceGroups("Motor", "Tiller");
                _sProp = _sModel.GetFaceGroups("Prop");

                // Load the textures used when drawing the ship
                _sPlanksTexture = BitmapTexture2D.FromFile(Program.GetResourcePath("planks.png"));
                _sFiberglassTexture = BitmapTexture2D.FromFile(Program.GetResourcePath("fiberglass.png"));
            }

            // Create the three invisible boyant floats used to simulate the ship's physics
            _frontFloat = new Floater(new Vector3(6f, 0f, 0f));
            _leftFloat = new Floater(new Vector3(-6f, 0f, -4f));
            _rightFloat = new Floater(new Vector3(-6f, 0f, 4f));

            // Use the identity matrix for the ship's transformation until a new one is calculated
            _trans = Matrix4.Identity;
        }

        /// <summary>
        /// Simulate the movement and rotation of the ship, and update the transformation
        /// matrix. Also updates the rotation of the propeller and triggers the water depression
        /// effect in the wake of the ship.
        /// </summary>
        /// <param name="time">Time in seconds since the start of the application</param>
        /// <param name="water">Water instance to lookup the wave height at the ship's position</param>
        public void Update(double time, Water water)
        {
            // Simulate the movement of the three invisible floats
            _frontFloat.Update(time, water);
            _leftFloat.Update(time, water);
            _rightFloat.Update(time, water);

            // Find the position of the point half way between the two rear floats
            Vector3 aft = (_leftFloat.Position + _rightFloat.Position) / 2f;
            
            // Find the desired position of the centre of the ship, which is mid-way
            // between the front float and the aft midpoint.
            Position = (aft + _frontFloat.Position) / 2f;

            // Find the current pitch of the ship as a function of the front float's position
            // and the midpoint between the two rear floats.
            Vector3 diff = _frontFloat.Position - aft;
            Pitch = (float) Math.Atan2(diff.Y, Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z));

            // Find the current roll of the ship as a function of the two rear floats' positions
            diff = _rightFloat.Position - _leftFloat.Position;
            Roll = (float) Math.Atan2(diff.Y, Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z));

            // Find the current yaw of the ship as a function of the front float's position
            // and the midpoint between the two read floats
            diff = _frontFloat.Position - aft;
            Yaw = (float) -Math.Atan2(diff.Z, diff.X);

            // Construct the transformation matrix to be used when drawing the ship from the
            // position, pitch, roll, and yaw
            _trans = Matrix4.CreateTranslation(Position);
            _trans = Matrix4.Mult(Matrix4.CreateRotationY(Yaw), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationX(Roll), _trans);
            _trans = Matrix4.Mult(Matrix4.CreateRotationZ(Pitch), _trans);

            // Find the forward, right and up normalized vectors
            Forward = Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            Right = Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            Up = Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;

            // Calculate the current speed of the ship by averaging the velocities of the three floats
            float vel = (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity).Length / 3f;

            // If the rudder is at a non-zero angle, cause the boat to rotate by accelerating a
            // rear float laterally
            if (_rudderAng < 0) {
                _leftFloat.Accelerate(Right * vel * (-_rudderAng / MathHelper.PiOver4) / 64f);
            } else if (_rudderAng > 0) {
                _rightFloat.Accelerate(-Right * vel * (_rudderAng / MathHelper.PiOver4) / 64f);
            }

            // Accelerate the floats towards being in their correct positions relative to the ship
            _frontFloat.Accelerate((Position + Forward * 6f - _frontFloat.Position) / 128f);
            _leftFloat.Accelerate((Position - Forward * 6f - Right * 4f - _leftFloat.Position) / 128f);
            _rightFloat.Accelerate((Position - Forward * 6f + Right * 4f - _rightFloat.Position) / 128f);

            // Simulate drag which is increased when the floats are moving in a direction other
            // than the way the ship is pointing
            _frontFloat.Streamline(Forward, 0.02f);
            _leftFloat.Streamline(Forward, 0.01f);
            _rightFloat.Streamline(Forward, 0.01f);

            // Rotate the propeller
            _propAng += _propSpeed / 60f;

            // Calculate the amount to depress the water under the ship
            float mag = Math.Min(1f, (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity).Length / 12f);

            // Depress the water under the ship in positions down the length of the vessel
            Vector3 splashPos;
            for (int i = 0; i < 8; ++i) {
                splashPos = Position + Forward * (3.5f - i);
                water.Splash(new Vector2(splashPos.X, splashPos.Z), mag / 2f);
            }

            // Now depress the water under the two rear corners of the ship
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
            _sModel.Render(shader, _sInnerHull);
            shader.Colour = Color.CornflowerBlue;
            shader.Shinyness = 8f;
            _sModel.Render(shader, _sOuterHull);
            shader.Texture = _sFiberglassTexture;
            shader.Colour = Color4.White; // new Color4(64, 64, 64, 255);
            _sModel.Render(shader, _sTrim);
            shader.Colour = new Color4(32, 32, 32, 255);
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationY(_rudderAng), Matrix4.CreateTranslation(-4f, 0f, 0f)), _trans);
            shader.Shinyness = 4f;
            _sModel.Render(shader, _sMotor);
            shader.Colour = Color4.Gray;
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationX(_propAng), Matrix4.CreateTranslation(0f, -1f / 8f, 0f)),
                shader.Transform);
            shader.Shinyness = 4f;
            _sModel.Render(shader, _sProp);

            //_frontFloat.Render(shader);
            //_leftFloat.Render(shader);
            //_rightFloat.Render(shader);
        }

        public void Render(DepthClipShader shader)
        {
            shader.Transform = _trans;
            _sModel.Render(shader, _sWaterclip);
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
