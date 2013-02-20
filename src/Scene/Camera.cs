﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

namespace ComputerGraphicsCoursework.Scene
{
    public class Camera
    {
        private bool _perspectiveChanged;
        private bool _viewChanged;

        private Matrix4 _perspectiveMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _combinedMatrix;
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
        }

        public Matrix4 Viewmatrix
        {
            get { return _viewMatrix; }
        }

        public Matrix4 CombinedMatrix
        {
            get { return _combinedMatrix; }
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
                _perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, (float) Width / Height, 1f / 64f, 256f);

                UpdateViewMatrix();
            }
        }

        public void UpdateViewMatrix()
        {
            if (_viewChanged) {
                Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);
                Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);
                Matrix4 trns = Matrix4.CreateTranslation(-_position);

                _viewMatrix = Matrix4.Mult(trns, Matrix4.Mult(yRot, xRot));
                _combinedMatrix = Matrix4.Mult(_viewMatrix, _perspectiveMatrix);
            }
        }
    }
}