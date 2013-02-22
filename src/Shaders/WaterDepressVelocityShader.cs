using OpenTK;

using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class WaterDepressVelocityShader : WaterEffectShader
    {
        private Vector3 _depression;

        public Vector2 DepressPosition
        {
            get { return _depression.Xy * 128f; }
            set
            {
                _depression.X = value.X / 128f + 0.5f;
                _depression.Y = value.Y / 128f + 0.5f;

                while (_depression.X < 0f) _depression.X += 1f;
                while (_depression.X >= 1f) _depression.X -= 1f;
                while (_depression.Y < 0f) _depression.Y += 1f;
                while (_depression.Y >= 1f) _depression.Y -= 1f;
            }
        }

        public float DepressionMagnitude
        {
            get { return _depression.Z; }
            set { _depression.Z = value; }
        }

        protected override void OnAddShaderVariables(ShaderBuilder frag)
        {
            base.OnAddShaderVariables(frag);

            frag.AddUniform(ShaderVarType.Vec3, "depression");
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
                    vec2 diff = vec2(diff(depression.x, tex_pos.x), diff(depression.y, tex_pos.y));
                    float dist = length(diff) * 128.0;
                    if (dist < 1.0) {
                        float scale = min(1.0, (1.0 - dist) * depression.z);
                        float cur = texture2D(velocitymap, tex_pos).a;
                        float mag = max(cur - scale, 0.5 - scale);
                        out_colour = vec4(mag, mag, mag, mag);
                    } else {
                        discard;
                    }
                }
            ";
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddUniform("depression");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            SetUniform("depression", _depression);
        }
    }
}
