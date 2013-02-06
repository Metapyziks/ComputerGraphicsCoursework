using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

namespace ComputerGraphicsCoursework
{
    public class Camera
    {
        private bool _perspectiveChanged;
        private bool _viewChanged;

        private Matrix4 _perspectiveMatrix;
        private Matrix4 _viewMatrix;
        private Vector3 _position;
        private Vector2 _rotation;
        private float _scale;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int WrapWidth { get; private set; }
        public int WrapHeight { get; private set; }

        public Matrix4 PerspectiveMatrix
        {
            get { return _perspectiveMatrix; }
            set
            {
                _perspectiveMatrix = value;
                _perspectiveChanged = false;
            }
        }

        public Matrix4 ViewMatrix
        {
            get { return _viewMatrix; }
            set
            {
                _viewMatrix = value;
                _viewChanged = false;
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _viewChanged = true;
            }
        }

        public Vector2 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _viewChanged = true;
            }
        }

        public float Pitch
        {
            get { return _rotation.X; }
            set
            {
                _rotation.X = value;
                _perspectiveChanged = true;
            }
        }

        public float Yaw
        {
            get { return _rotation.Y; }
            set
            {
                _rotation.Y = value;
                _viewChanged = true;
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _perspectiveChanged = true;
            }
        }

        public Camera(int width, int height, float scale = 1.0f)
        {
            Width = width;
            Height = height;

            Position = new Vector3();
            Rotation = new Vector2();

            Scale = scale;

            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void SetScreenSize(int width, int height)
        {
            Width = width;
            Height = height;
            UpdatePerspectiveMatrix();
        }

        public void UpdatePerspectiveMatrix()
        {
            if (_perspectiveChanged) {
                _perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(90f * MathHelper.Pi / 180f, (float) Width / Height, 1f / 64f, 64f);

                UpdateViewMatrix();
            }
        }

        public void UpdateViewMatrix()
        {
            if (_viewChanged) {
                float rotOffset = (float) (Math.Tan(Math.PI / 2.0 - _rotation.X) * _position.Y);

                Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);
                Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);
                Matrix4 trns = Matrix4.CreateTranslation(-_position);
                Matrix4 offs = Matrix4.CreateTranslation(0.0f, 0.0f, -rotOffset);

                _viewMatrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Mult(Matrix4.Mult(trns, yRot), offs), xRot), _perspectiveMatrix);
            }
        }
    }
}
