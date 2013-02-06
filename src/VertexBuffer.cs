using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class VertexBuffer : IDisposable
    {
        private int myStride;

        private bool myDataSet = false;

        private int myUnitSize;
        private int myVboID;
        private int myLength;

        private int VboID
        {
            get
            {
                if (myVboID == 0)
                    GL.GenBuffers(1, out myVboID);

                return myVboID;
            }
        }

        public int Stride
        {
            get { return myStride; }
        }

        public VertexBuffer(int stride)
        {
            myStride = stride;
        }

        public void SetData<T>(T[] vertices) where T : struct
        {
            myUnitSize = Marshal.SizeOf(typeof(T));
            myLength = vertices.Length / myStride;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * myUnitSize), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            CheckForError();

            myDataSet = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if (error != ErrorCode.NoError)
                throw new Exception("OpenGL hates your guts: " + error.ToString());
        }

        public void StartBatch(ShaderProgram shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);

            foreach (AttributeInfo info in shader.Attributes) {
                GL.VertexAttribPointer(info.Location, info.Size, info.PointerType,
                    info.Normalize, shader.VertexDataStride, info.Offset);
                GL.EnableVertexAttribArray(info.Location);
            }
        }

        public void Render(ShaderProgram shader, int first = 0, int count = -1)
        {
            if (myDataSet) {
                if (count == -1)
                    count = myLength - first;

                GL.DrawArrays(shader.BeginMode, first, count);
            }
        }

        public void EndBatch(ShaderProgram shader)
        {
            foreach (AttributeInfo info in shader.Attributes)
                GL.DisableVertexAttribArray(info.Location);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            if (myDataSet)
                GL.DeleteBuffers(1, ref myVboID);

            myDataSet = false;
        }
    }
}