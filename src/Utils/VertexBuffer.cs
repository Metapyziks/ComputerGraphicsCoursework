using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Shaders;

namespace ComputerGraphicsCoursework.Utils
{
    public sealed class VertexBuffer : IDisposable
    {
        private int _stride;

        private bool _dataSet = false;

        private int _unitSize;
        private int _vboID;
        private int _length;

        private ShaderProgram _curShader;

        public int VboID
        {
            get
            {
                if (_vboID == 0) GL.GenBuffers(1, out _vboID);

                return _vboID;
            }
        }

        public int Stride
        {
            get { return _stride; }
        }

        public VertexBuffer(int stride)
        {
            _stride = stride;
        }

        public void SetData<T>(T[] vertices) where T : struct
        {
            _unitSize = Marshal.SizeOf(typeof(T));
            _length = vertices.Length / _stride;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * _unitSize), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            Tools.ErrorCheck("setdata");

            _dataSet = true;
        }

        public void Begin(ShaderProgram shader)
        {
            _curShader = shader;

            Tools.ErrorCheck("vboprebegin");

            shader.Begin(false);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            shader.BeginArrays();
        }

        public void Render(int first = 0, int count = -1)
        {
            if (_dataSet) {
                if (count == -1) {
                    count = _length - first;
                }

                GL.DrawArrays(_curShader.BeginMode, first, count);
            }
        }

        public void End()
        {
            _curShader.End();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            if (_vboID != 0) {
                GL.DeleteBuffers(1, ref _vboID);
                _vboID = 0;
            }

            _dataSet = false;
        }
    }
}