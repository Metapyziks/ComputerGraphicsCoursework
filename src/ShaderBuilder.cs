using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    public enum ShaderVarType
    {
        Int,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Sampler2D,
        Sampler2DArray,
        Mat4
    }

    public class ShaderBuilder
    {
        private struct ShaderVariable
        {
            public ShaderVarType Type;
            public String Identifier;

            public String TypeString
            {
                get
                {
                    String _sr = Type.ToString();

                    return _sr[ 0 ].ToString().ToLower()
                        + _sr.Substring( 1 );
                }
            }
        }

        private bool _TwoDimensional;

        private List<String> _Extensions;

        private List<ShaderVariable> _Uniforms;
        private List<ShaderVariable> _Attribs;
        private List<ShaderVariable> _Varyings;

        public ShaderType Type { get; private set; }

        public String Logic { get; set; }
        public String FragOutIdentifier { get; set; }

        public ShaderBuilder( ShaderType type, bool twoDimensional )
        {
            Type = type;
            _TwoDimensional = twoDimensional;

            _Extensions = new List<String>();

            _Uniforms = new List<ShaderVariable>();
            _Attribs  = new List<ShaderVariable>();
            _Varyings = new List<ShaderVariable>();

            Logic = "";
            FragOutIdentifier = "out_frag_colour";
        }

        public void AddUniform( ShaderVarType type, String identifier )
        {
            if ( type == ShaderVarType.Sampler2DArray )
            {
                String ext = "GL_EXT_texture_array";
                if ( !_Extensions.Contains( ext ) )
                    _Extensions.Add( ext );
            }

            _Uniforms.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public void AddAttribute( ShaderVarType type, String identifier )
        {
            _Attribs.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public void AddVarying( ShaderVarType type, String identifier )
        {
            _Varyings.Add( new ShaderVariable { Type = type, Identifier = identifier } );
        }

        public String Generate( bool gl3 )
        {
            String nl = Environment.NewLine;

            String output = 
                "#version 1" + ( gl3 ? "3" : "2" ) + "0" + nl + nl;

            if ( _Extensions.Count != 0 )
            {
                foreach ( String ext in _Extensions )
                    output += "#extension " + ext + " : enable" + nl;
                output += nl;
            }

            output +=
                  ( gl3 ? "precision highp float;" + nl + nl : "" )
                + ( Type == ShaderType.VertexShader && _TwoDimensional
                    ? "uniform vec2 screen_resolution;" + nl + nl
                    : "" );

            foreach ( ShaderVariable var in _Uniforms )
                output += "uniform "
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if( _Uniforms.Count != 0 )
                output += nl;

            foreach ( ShaderVariable var in _Attribs )
                output += ( gl3 ? "in " : "attribute " )
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if( _Attribs.Count != 0 )
                output += nl;

            foreach ( ShaderVariable var in _Varyings )
                output += ( gl3 ? Type == ShaderType.VertexShader
                    ? "out " : "in " : "varying " )
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if ( gl3 && Type == ShaderType.FragmentShader )
                output += "out vec4 " + FragOutIdentifier + ";" + nl + nl;

            int index = Logic.IndexOf( "void" ) - 1;
            String indent = "";
            while ( index >= 0 && Logic[ index ] == ' ' )
                indent += Logic[ index-- ];

            indent = new String( indent.Reverse().ToArray() );

            String logic = indent.Length == 0 ? Logic.Trim() : Logic.Trim().Replace( indent, "" );

            if ( Type == ShaderType.FragmentShader )
            {
                if ( gl3 )
                    logic = logic.Replace( "texture2DArray(", "texture(" )
                        .Replace( "texture2D(", "texture(" );
                else
                    logic = logic.Replace( FragOutIdentifier, "gl_FragColor" );                        
            }
            else if( _TwoDimensional )
            {
                logic = logic.Replace( "gl_Position", "vec2 _pos_" );
                index = logic.IndexOf( "_pos_" );
                index = logic.IndexOf( ';', index ) + 1;
                logic = logic.Insert( index, nl
                    + "    _pos_ -= screen_resolution / 2.0;" + nl
                    + "    _pos_.x /= screen_resolution.x / 2.0;" + nl
                    + "    _pos_.y /= -screen_resolution.y / 2.0;" + nl
                    + "    gl_Position = vec4( _pos_, 0.0, 1.0 );" );
            }

            output += logic;

            return output;
        }
    }
}
