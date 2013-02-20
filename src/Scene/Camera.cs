using System;

using OpenTK;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class containing the camera's current position and rotation, along
    /// with the perspective and view matrices used when rendering the scene.
    /// </summary>
    public class Camera
    {
        #region Private Fields
        private bool _perspectiveChanged;
        private bool _viewChanged;

        private Matrix4 _perspectiveMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _combinedMatrix;
        private Vector3 _position;
        private Vector2 _rotation;
        #endregion

        /// <summary>
        /// Current width in pixels of the viewport being drawn to.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Current height in pixels of the viewport being drawn to.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Perspective matrix that encodes the transformation from
        /// eye-space to screen-space.
        /// </summary>
        public Matrix4 PerspectiveMatrix
        {
            get
            {
                if (_perspectiveChanged) UpdatePerspectiveMatrix();

                return _perspectiveMatrix;
            }
        }

        /// <summary>
        /// View matrix that encodes the transformation from world-space
        /// to eye-space.
        /// </summary>
        public Matrix4 Viewmatrix
        {
            get
            {
                if (_perspectiveChanged) UpdatePerspectiveMatrix();
                else if (_viewChanged) UpdateViewMatrix();

                return _viewMatrix;
            }
        }

        /// <summary>
        /// Combined view and perspective matrix that encodes the
        /// transformation from world-space to screen-space.
        /// </summary>
        public Matrix4 CombinedMatrix
        {
            get
            {
                if (_perspectiveChanged) UpdatePerspectiveMatrix();
                else if (_viewChanged) UpdateViewMatrix();

                return _combinedMatrix;
            }
        }

        /// <summary>
        /// Position of the camera in the world.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// Rotation of the camera, stored as the rotation on the
        /// X and Y axis (pitch and yaw).
        /// </summary>
        public Vector2 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// The pitch of the camera (rotation on the X axis).
        /// </summary>
        public float Pitch
        {
            get { return _rotation.X; }
            set
            {
                _rotation.X = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// The yaw of the camera (rotation on the Y axis).
        /// </summary>
        public float Yaw
        {
            get { return _rotation.Y; }
            set
            {
                _rotation.Y = value;
                _viewChanged = true;
            }
        }

        public Vector3 ViewVector
        {
            get
            {
                float cosYaw = (float) Math.Cos(Yaw);
                float sinYaw = (float) Math.Sin(Yaw);
                float cosPitch = (float) Math.Cos(Pitch);
                float sinPitch = (float) Math.Sin(Pitch);
                return new Vector3(sinYaw * cosPitch, -sinPitch, -cosYaw * cosPitch);
            }
            set
            {
                value.Normalize();
                Pitch = (float) Math.Asin(-value.Y);
                Yaw = (float) Math.Atan2(value.X, -value.Z);
            }
        }

        public Camera(int width, int height)
        {
            Width = width;
            Height = height;

            Position = new Vector3();
            Rotation = new Vector2();

            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void SetScreenSize(int width, int height)
        {
            Width = width;
            Height = height;

            UpdatePerspectiveMatrix();
        }

        private void UpdatePerspectiveMatrix()
        {
            _perspectiveChanged = false;

            _perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, (float) Width / Height, 1f / 64f, 256f);

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            _viewChanged = false;

            Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);
            Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);
            Matrix4 trns = Matrix4.CreateTranslation(-_position);

            _viewMatrix = Matrix4.Mult(trns, Matrix4.Mult(yRot, xRot));
            _combinedMatrix = Matrix4.Mult(_viewMatrix, _perspectiveMatrix);
        }
    }
}
