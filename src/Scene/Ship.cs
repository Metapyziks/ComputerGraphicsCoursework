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
    /// Class representing the moveable ship. The physics of the ship is simulated
    /// by using three boyant point masses (floats) that interact with the water and
    /// try and form a triangle with a specific shape on the surface.
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
        public Vector3 Forward
        {
            get
            {
                return Vector4.Transform(new Vector4(1f, 0f, 0f, 0f), _trans).Xyz;
            }
        }

        /// <summary>
        /// Normalized right-hand vector of the ship.
        /// </summary>
        public Vector3 Right
        {
            get
            {
                return Vector4.Transform(new Vector4(0f, 0f, 1f, 0f), _trans).Xyz;
            }
        }

        /// <summary>
        /// Normalized up vector of the ship.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return Vector4.Transform(new Vector4(0f, 1f, 0f, 0f), _trans).Xyz;
            }
        }

        /// <summary>
        /// Position of the ship in the world.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Current velocity of the ship.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                // The ship velocity is the average velocity of the three invisible floats
                return (_frontFloat.Velocity + _leftFloat.Velocity + _rightFloat.Velocity) / 3f;
            }
        }

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

            // Calculate the current speed of the ship by averaging the velocities of the three floats
            float speed = Velocity.Length;

            // If the rudder is at a non-zero angle, cause the boat to rotate by accelerating a
            // rear float laterally
            if (_rudderAng < 0) {
                _leftFloat.Velocity += Right * speed * (-_rudderAng / MathHelper.PiOver4) / 64f;
            } else if (_rudderAng > 0) {
                _rightFloat.Velocity -= Right * speed * (_rudderAng / MathHelper.PiOver4) / 64f;
            }

            // Accelerate the floats towards being in their correct positions relative to the ship
            _frontFloat.Velocity += (Position + Forward * 6f - _frontFloat.Position) / 128f;
            _leftFloat.Velocity += (Position - Forward * 6f - Right * 4f - _leftFloat.Position) / 128f;
            _rightFloat.Velocity += (Position - Forward * 6f + Right * 4f - _rightFloat.Position) / 128f;

            // Simulate drag which is increased when the floats are moving in a direction other
            // than the way the ship is pointing
            _frontFloat.Streamline(Forward, 0.02f);
            _leftFloat.Streamline(Forward, 0.01f);
            _rightFloat.Streamline(Forward, 0.01f);

            // Rotate the propeller
            _propAng += _propSpeed / 60f;

            // Calculate the amount to depress the water under the ship
            float mag = Math.Min(4f, Velocity.Length);

            // Depress the water under the ship in positions down the length of the vessel
            Vector3 splashPos;
            for (float i = 0f; i < 8f; i += 0.5f) {
                splashPos = Position + Forward * (3.5f - i);
                water.Splash(new Vector2(splashPos.X, splashPos.Z), mag);
            }

            // Now depress the water under the two rear corners of the ship
            splashPos = Position - Forward * 3.5f - Right;
            water.Splash(new Vector2(splashPos.X, splashPos.Z), mag);
            splashPos += Right * 2f;
            water.Splash(new Vector2(splashPos.X, splashPos.Z), mag);
        }

        /// <summary>
        /// Draw the ship to the screen.
        /// </summary>
        /// <param name="shader">ModelShader to use to draw each face of the ship</param>
        public void Render(ModelShader shader)
        {
            // Use the ship's transformation when drawing the model
            shader.Transform = _trans;

            // Inner hull texture, specular shinyness and colour
            shader.Texture = _sPlanksTexture;
            shader.Shinyness = 2f;
            shader.Colour = Color4.White;

            // Draw the inner hull
            _sModel.Render(shader, _sInnerHull);

            // Outer hull colour and specular shinyness (same texture as the inner hull)
            shader.Colour = Color.CornflowerBlue;
            shader.Shinyness = 8f;

            // Draw the outer hull
            _sModel.Render(shader, _sOuterHull);

            // Trim texture and colour
            shader.Texture = _sFiberglassTexture;
            shader.Colour = Color4.White;

            // Draw the trim
            _sModel.Render(shader, _sTrim);

            // The tiller and motor transformation is an aditional rotation and translation 
            // before the main ship transformation
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationY(_rudderAng), Matrix4.CreateTranslation(-4f, 0f, 0f)),
                shader.Transform);

            // Tiller and motor colour and specular shinyness
            shader.Colour = new Color4(32, 32, 32, 255);
            shader.Shinyness = 4f;

            // Draw the tiller and motor
            _sModel.Render(shader, _sMotor);

            // The propeller transformation is an aditional rotation and translation before
            // the motor and tiller transformation
            shader.Transform = Matrix4.Mult(Matrix4.Mult(
                Matrix4.CreateRotationX(_propAng), Matrix4.CreateTranslation(0f, -1f / 8f, 0f)),
                shader.Transform);

            // Propeller colour and specular shinyness
            shader.Colour = Color4.Gray;
            shader.Shinyness = 8f;

            // Draw the propeller
            _sModel.Render(shader, _sProp);

            // Draw the otherwise invisible floats
            /*
                _frontFloat.Render(shader);
                leftFloat.Render(shader);
                rightFloat.Render(shader);
            */
        }

        /// <summary>
        /// Draw the invisible depth clip plane that hides any water
        /// behind it. Used to fix the ship having water inside it.
        /// </summary>
        /// <param name="shader">DepthClipShader to use when drawing the clip plane</param>
        public void Render(DepthClipShader shader)
        {
            shader.Transform = _trans;
            _sModel.Render(shader, _sWaterclip);
        }

        /// <summary>
        /// Event handler for when a key is pressed.
        /// Implementation of IKeyControllable.KeyDown(Key)
        /// </summary>
        /// <param name="key">The key that has just been pressed</param>
        public void KeyDown(Key key) { }

        /// <summary>
        /// Event handler for when a key is released.
        /// Implementation of IKeyControllable.KeyUp(Key)
        /// </summary>
        /// <param name="key">The key that has just been released</param>
        public void KeyUp(Key key) { }

        /// <summary>
        /// Method invoked once per update to check the keyboard state.
        /// </summary>
        /// <param name="keyboard">KeyboardDevice to read input from</param>
        public void UpdateKeys(KeyboardDevice keyboard)
        {
            // Decelerate the propeller
            _propSpeed *= 0.99f;

            // If the W key is being held, accelerate forwards
            if (keyboard[Key.W]) {
                _leftFloat.Velocity += Forward / 128f;
                _rightFloat.Velocity += Forward / 128f;
                _propSpeed += 1f / 4f;
            }

            // If the S key is being held, accelerate backwards
            if (keyboard[Key.S]) {
                _leftFloat.Velocity -= Forward / 256f;
                _rightFloat.Velocity -= Forward / 256f;
                _propSpeed -= 1f / 8f;
            }

            // Find the amount to move the rudder by, with the base
            // movement being towards the neutral position with a speed
            // relative to the speed of the ship
            float rudderVel = -_rudderAng * Velocity.Length / 8f;

            // If the A key is being held, rotate the rudder left
            if (keyboard[Key.A]) {
                rudderVel -= RudderMoveSpeed;
            }

            // If the D key is being held, rotate the rudder right
            if (keyboard[Key.D]) {
                rudderVel += RudderMoveSpeed;
            }

            // Rotate the rudder by the amount calculated, and keep it
            // within a bounds of +/- 45 degrees
            _rudderAng = Tools.Clamp(_rudderAng + rudderVel, -MathHelper.PiOver4, MathHelper.PiOver4);
        }
    }
}
