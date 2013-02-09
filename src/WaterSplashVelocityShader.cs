using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework
{
    class WaterSplashVelocityShader : WaterEffectShader
    {
        private Vector3 _splash;
        private int _splashLoc;

        public Vector2 SplashPosition
        {
            get { return _splash.Xy * 128f; }
            set
            {
                _splash.X = value.X / 128f + 0.5f;
                _splash.Y = value.Y / 128f + 0.5f;

                while (_splash.X < 0f) _splash.X += 1f;
                while (_splash.X >= 1f) _splash.X -= 1f;
                while (_splash.Y < 0f) _splash.Y += 1f;
                while (_splash.Y >= 1f) _splash.Y -= 1f;
            }
        }

        public float SplashMagnitude
        {
            get { return _splash.Z; }
            set
            {
                _splash.Z = value;
            }
        }

        protected override void OnAddShaderVariables(ShaderBuilder frag)
        {
            base.OnAddShaderVariables(frag);

            frag.AddUniform(ShaderVarType.Vec3, "splash");
        }

        protected override void OnAddShaderLogic(ShaderBuilder frag)
        {
            frag.Logic = @"
                float diff(float a, float b)
                {
                    float d = b - a;
                    if (d < -0.5) return d + 1.0;
                    if (d >= 0.5) return d - 1.0;
                    return d;
                }

                void main(void)
                {
                    vec2 diff = vec2(diff(splash.x, tex_pos.x), diff(splash.y, tex_pos.y));
                    float dist = length(diff) * 128.0;
                    if (dist < 1.0) {
                        float scale = (1.0 - dist) * splash.z;
                        float cur = texture(velocitymap, tex_pos).a;
                        float mag = cur - scale;
                        out_frag_colour = vec4(mag, mag, mag, mag);
                    } else {
                        discard;
                    }
                }
            ";
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _splashLoc = GL.GetUniformLocation(Program, "splash");
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Uniform3(_splashLoc, ref _splash);
        }
    }
}
