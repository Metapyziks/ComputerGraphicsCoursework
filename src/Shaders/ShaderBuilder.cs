using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Shaders
{
    public enum ShaderVarType
    {
        Int,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Sampler2D,
        SamplerCube,
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

                    return _sr[0].ToString().ToLower()
                        + _sr.Substring(1);
                }
            }
        }

        private bool _twoDimensional;

        private List<String> _extensions;

        private List<ShaderVariable> _uniforms;
        private List<ShaderVariable> _attribs;
        private List<ShaderVariable> _varyings;

        public ShaderType Type { get; private set; }

        public String Logic { get; set; }
        public String FragOutIdentifier { get; set; }

        public ShaderBuilder(ShaderType type, bool twoDimensional, ShaderBuilder parent = null)
        {
            Type = type;
            _twoDimensional = twoDimensional;

            _extensions = new List<String>();

            _uniforms = new List<ShaderVariable>();
            _attribs = new List<ShaderVariable>();
            _varyings = new List<ShaderVariable>();

            if (parent != null) {
                foreach (var vary in parent._varyings) {
                    AddVarying(vary.Type, vary.Identifier);
                }
            }

            Logic = "";
            FragOutIdentifier = "out_frag_colour";
        }

        public void AddUniform(ShaderVarType type, String identifier)
        {
            if (type == ShaderVarType.Sampler2DArray) {
                String ext = "GL_EXT_texture_array";
                if (!_extensions.Contains(ext))
                    _extensions.Add(ext);
            }

            _uniforms.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        public void AddAttribute(ShaderVarType type, String identifier)
        {
            _attribs.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        public void AddVarying(ShaderVarType type, String identifier)
        {
            _varyings.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        public String Generate(bool gl3)
        {
            String nl = Environment.NewLine;

            String output = 
                "#version " + (gl3 ? "13" : "12") + "0" + nl + nl;

            if (_extensions.Count != 0) {
                foreach (String ext in _extensions)
                    output += "#extension " + ext + " : enable" + nl;
                output += nl;
            }

            output +=
                  (gl3 ? "precision highp float;" + nl + nl : "")
                + (Type == ShaderType.VertexShader && _twoDimensional
                    ? "uniform vec2 screen_resolution;" + nl + nl
                    : "");

            foreach (ShaderVariable var in _uniforms)
                output += "uniform "
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if (_uniforms.Count != 0)
                output += nl;

            foreach (ShaderVariable var in _attribs)
                output += (gl3 ? "in " : "attribute ")
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if (_attribs.Count != 0)
                output += nl;

            foreach (ShaderVariable var in _varyings)
                output += (gl3 ? Type == ShaderType.VertexShader
                    ? "out " : "in " : "varying ")
                    + var.TypeString
                    + " " + var.Identifier + ";" + nl;

            if (gl3 && Type == ShaderType.FragmentShader)
                output += "out vec4 " + FragOutIdentifier + ";" + nl + nl;

            int index = Logic.IndexOf("void") - 1;
            String indent = "";
            while (index >= 0 && Logic[index] == ' ')
                indent += Logic[index--];

            indent = new String(indent.Reverse().ToArray());

            String logic = indent.Length == 0 ? Logic.Trim() : Logic.Trim().Replace(indent, "");

            if (Type == ShaderType.FragmentShader) {
                if (gl3)
                    logic = logic.Replace("texture2DArray(", "texture(")
                        .Replace("texture2D(", "texture(");
                else
                    logic = logic.Replace(FragOutIdentifier, "gl_FragColor");
            } else if (_twoDimensional) {
                logic = logic.Replace("gl_Position", "vec2 _pos_");
                index = logic.IndexOf("_pos_");
                index = logic.IndexOf(';', index) + 1;
                logic = logic.Insert(index, nl
                    + "    _pos_ -= screen_resolution / 2.0;" + nl
                    + "    _pos_.x /= screen_resolution.x / 2.0;" + nl
                    + "    _pos_.y /= -screen_resolution.y / 2.0;" + nl
                    + "    gl_Position = vec4( _pos_, 0.0, 1.0 );");
            }

            output += logic;

            return output;
        }
    }
}