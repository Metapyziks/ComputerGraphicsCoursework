﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class TestShader : ShaderProgram3D
    {
        private Color4 _colour = Color4.White;
        private int _colourLoc = -1;

        private Matrix4 _trans = Matrix4.Identity;
        private int _transLoc = -1;

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                if (_colourLoc != -1) {
                    GL.Uniform4(_colourLoc, value);
                }
            }
        }

        public Matrix4 Transform
        {
            get { return _trans; }
            set
            {
                _trans = value;
                if (_colourLoc != -1) {
                    GL.UniformMatrix4(_transLoc, false, ref value);
                }
            }
        }

        public TestShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "transform");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddAttribute(ShaderVarType.Vec3, "in_normal");
            vert.AddVarying(ShaderVarType.Vec3, "var_normal");
            vert.Logic = @"
                void main(void)
                {
                    var_normal = (transform * vec4(in_normal, 0.0)).xyz;
                    gl_Position = view_matrix * (transform * vec4(in_vertex, 1.0));
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.AddVarying(ShaderVarType.Vec3, "var_normal");
            frag.Logic = @"
                void main(void)
                {
                    const vec3 light = normalize(vec3(-3, -4, -8));
                    out_frag_colour = vec4(colour.rgb * (3.0 + dot(light, var_normal)) * 0.25, 1.0);
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Triangles;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_vertex", 3);
            AddAttribute("in_normal", 3);

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            _transLoc = GL.GetUniformLocation(Program, "transform");

            GL.Uniform4(_colourLoc, Colour);
            GL.UniformMatrix4(_transLoc, false, ref _trans);
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable(EnableCap.DepthTest);
        }

        public void Render(Vector3 vert, Vector3 norm)
        {
            GL.VertexAttrib3(Attributes[0].Location, vert);
            GL.VertexAttrib3(Attributes[1].Location, norm);
        }
    }
}
