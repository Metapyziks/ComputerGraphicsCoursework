using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public class VertexBuffer : IDisposable
    {
        private int __sride;

        private bool _DataSet = false;

        private int _UnitSize;
        private int _VboID;
        private int _Length;

        private int VboID
        {
            get
            {
                if ( _VboID == 0 )
                    GL.GenBuffers( 1, out _VboID );

                return _VboID;
            }
        }

        public int _sride
        {
            get { return __sride; }
        }

        public VertexBuffer( int _sride )
        {
            __sride = _sride;
        }

        public void SetData<T>( T[] vertices ) where T : struct
        {
            _UnitSize = Marshal.SizeOf( typeof( T ) );
            _Length = vertices.Length / __sride;

            GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );
            GL.BufferData( BufferTarget.ArrayBuffer, new IntPtr( vertices.Length * _UnitSize ), vertices, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            CheckForError();

            _DataSet = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if ( error != ErrorCode.NoError )
                throw new Exception( "OpenGL hates your guts: " + error.ToString() );
        }

        public void startBatch( ShaderProgram shader )
        {
            GL.BindBuffer( BufferTarget.ArrayBuffer, VboID );

            foreach ( AttributeInfo info in shader.Attributes )
            {
                GL.VertexAttribPointer( info.Location, info.Size, info.PointerType,
                    info.Normalize, shader.VertexData_sride, info.Offset );
                GL.EnableVertexAttribArray( info.Location );
            }
        }

        public void Render( ShaderProgram shader, int fir_s = 0, int count = -1 )
        {
            if ( _DataSet )
            {
                if ( count == -1 )
                    count = _Length - fir_s;

                GL.DrawArrays( shader.BeginMode, fir_s, count );
            }
        }

        public void EndBatch( ShaderProgram shader )
        {
            foreach ( AttributeInfo info in shader.Attributes )
                GL.DisableVertexAttribArray( info.Location );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
        }

        public void Dispose()
        {
            if ( _DataSet )
                GL.DeleteBuffers( 1, ref _VboID );

            _DataSet = false;
        }
    }
}