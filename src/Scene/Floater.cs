using System;

using OpenTK;
using OpenTK.Graphics;

using ComputerGraphicsCoursework.Shaders;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class representing a bouyant point mass that reacts to the surface
    /// of the water.
    /// </summary>
    public class Floater :
        IRenderable<ModelShader>,
        IUpdateable
    {
        #region Private Static Fields
        private static Model _sModel;
        #endregion
        
        /// <summary>
        /// Current position of the floater in world-space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Current movement vector of the floater.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Constructor to create a new Floater instance.
        /// </summary>
        /// <param name="position">Initial position of the floater in the world</param>
        public Floater(Vector3 position)
        {
            // If the model has never been loaded, load the model and store it in a
            // static field so all floaters created after this one can use it
            if (_sModel == null) {
                // Load the sphere model
                _sModel = Model.FromFile(Program.GetResourcePath("sphere.obj"));
            }

            // Set the initial position and velocity
            Position = position;
            Velocity = new Vector3();
        }
        
        /// <summary>
        /// Simulate drag acting on the float, which is amplified if the float is
        /// moving near to perpendicular to some given direction.
        /// </summary>
        /// <param name="direction">Direction of streamlining</param>
        /// <param name="weight">How much the direction of movement affects drag</param>
        public void Streamline(Vector3 direction, float weight)
        {
            Vector3 normal = Velocity;
            normal.Normalize();
            Velocity *= (1f - weight) + weight * Math.Abs(Vector3.Dot(normal, direction));
        }

        /// <summary>
        /// Draw the floater at its current position in the world.
        /// </summary>
        /// <param name="shader">ModelShader to use to draw the floater</param>
        public void Render(ModelShader shader)
        {
            // Set the model position to be the current position of the floater
            shader.Transform = Matrix4.CreateTranslation(Position);

            // Set the material of the floater to be a shiny red surface
            shader.Colour = Color4.Red;
            shader.Shinyness = 2f;

            // Draw the floater
            _sModel.Render(shader);
        }

        /// <summary>
        /// Simulate the movement of the floater on the water surface.
        /// </summary>
        /// <param name="time">Time in seconds since the start of the application</param>
        /// <param name="water">Water instance to lookup the wave height at the floater's position</param>
        public void Update(double time, Water water)
        {
            // Get the height and surface gradient of the water at the floater's position
            Vector3 info = water.GetSurfaceInfo(Position);

            // Create an empty vector to store how much to accelerate the floater by
            Vector3 accel = new Vector3();

            // If the floater is submerged...
            if (info.Y > Position.Y) {
                // Find what proportion of the floater is submerged
                float depth = Math.Min(1.0f, info.Y - Position.Y);

                // Accelerate the floater with the surface gradient, and towards the surface
                accel.X += info.X * depth / 64f;
                accel.Y += depth / 128f;
                accel.Z += info.Z * depth / 64f;
                
                // Simulate water resistance to decelerate the floater
                Velocity *= 1f - depth * 0.03f;
            } else {
                // If the water is above the surface, simulate gravity pulling it down and
                // air resistance slowing it
                accel.Y -= 1f / 128f;
                Velocity *= 0.99f;
            }

            // Update the velocity and position of the floater
            Velocity += accel;
            Position += Velocity;
        }
    }
}
