using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace ComputerGraphicsCoursework
{
    public class AttributeInfo
    {
        public ShaderProgram Shader { get; private set; }
        public String Identifier { get; private set; }
        public int Location { get; private set; }
        public int Size { get; private set; }
        public int Offset { get; private set; }
        public int Divisor { get; private set; }
        public int InputOffset { get; private set; }
        public VertexAttribPointerType PointerType { get; private set; }
        public bool Normalize { get; private set; }

        public int Length
        {
            get
            {
                switch (PointerType) {
                    case VertexAttribPointerType.Byte:
                    case VertexAttribPointerType.UnsignedByte:
                        return Size * sizeof(byte);

                    case VertexAttribPointerType.Short:
                    case VertexAttribPointerType.UnsignedShort:
                        return Size * sizeof(short);

                    case VertexAttribPointerType.Int:
                    case VertexAttribPointerType.UnsignedInt:
                        return Size * sizeof(int);

                    case VertexAttribPointerType.HalfFloat:
                        return Size * sizeof(float) / 2;

                    case VertexAttribPointerType.Float:
                        return Size * sizeof(float);

                    case VertexAttribPointerType.Double:
                        return Size * sizeof(double);

                    default:
                        return 0;
                }
            }
        }

        public AttributeInfo(ShaderProgram shader, String identifier,
            int size, int offset, int divisor, int inputOffset,
            VertexAttribPointerType pointerType =
                VertexAttribPointerType.Float,
            bool normalize = false)
        {
            Shader = shader;
            Identifier = identifier;
            Location = GL.GetAttribLocation(shader.Program, Identifier);
            Size = size;
            Offset = offset;
            Divisor = divisor;
            InputOffset = inputOffset;
            PointerType = pointerType;
            Normalize = normalize;
        }

        public override String ToString()
        {
            return Identifier + " @" + Location + ", Size: " + Size + ", Offset: " + Offset;
        }
    }

    public class ShaderProgram
    {
        private class TextureInfo
        {
            public ShaderProgram Shader { get; private set; }
            public String Identifier { get; private set; }
            public int UniformLocation { get; private set; }
            public TextureUnit TextureUnit { get; private set; }
            public Texture CurrentTexture { get; private set; }

            public TextureInfo(ShaderProgram shader, String identifier,
                TextureUnit textureUnit = TextureUnit.Texture0)
            {
                Shader = shader;
                Identifier = identifier;
                UniformLocation = GL.GetUniformLocation(Shader.Program, Identifier);
                TextureUnit = textureUnit;

                Shader.Use();

                int val = int.Parse(TextureUnit.ToString().Substring("Texture".Length));

                GL.Uniform1(UniformLocation, val);

                CurrentTexture = null;
            }

            public void SetCurrentTexture(Texture texture)
            {
                CurrentTexture = texture;

                GL.ActiveTexture(TextureUnit);
                CurrentTexture.Bind();
            }
        }

        private static bool _sVersionChecked;
        private static bool _sGL3;
        private static bool _sNVidiaCard = false;

        private static ShaderProgram _sCurProgram;

        public static bool GL3
        {
            get
            {
                if (!_sVersionChecked)
                    CheckGLVersion();

                return _sGL3;
            }
        }

        public static bool NVidiaCard
        {
            get
            {
                if (!_sVersionChecked)
                    CheckGLVersion();

                return _sNVidiaCard;
            }
        }

        private static void CheckGLVersion()
        {
            String _sr = GL.GetString(StringName.Version);
            _sGL3 = _sr.StartsWith("3.") || _sr.StartsWith("4.");

            _sr = GL.GetString(StringName.Vendor);
            _sNVidiaCard = _sr.ToUpper().StartsWith("NVIDIA");

            _sVersionChecked = true;
        }

        public int VertexDataStride;
        public int VertexDataSize;
        private List<AttributeInfo> _Attributes;
        private Dictionary<String, TextureInfo> _Textures;

        public int Program { get; private set; }

        public AttributeInfo[] Attributes
        {
            get { return _Attributes.ToArray(); }
        }

        public BeginMode BeginMode;
        public String VertexSource;
        public String FragmentSource;

        public bool Active
        {
            get { return _sCurProgram == this; }
        }

        public bool started;

        public ShaderProgram()
        {
            BeginMode = BeginMode.Triangles;
            _Attributes = new List<AttributeInfo>();
            _Textures = new Dictionary<String, TextureInfo>();
            VertexDataStride = 0;
            VertexDataSize = 0;
            started = false;
        }

        public void ErrorCheck(String loc = "unknown")
        {
#if DEBUG
            ErrorCode ec = GL.GetError();

            if (ec != ErrorCode.NoError)
                Debug.WriteLine(ec.ToString() + " at " + loc);
#endif
        }

        public void Create()
        {
            Program = GL.CreateProgram();

            int vert = GL.CreateShader(ShaderType.VertexShader);
            int frag = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vert, VertexSource);
            GL.ShaderSource(frag, FragmentSource);

            GL.CompileShader(vert);
            GL.CompileShader(frag);
#if DEBUG
            Debug.WriteLine(GetType().FullName + Environment.NewLine);
            Debug.WriteLine(GL.GetShaderInfoLog(vert));
            Debug.WriteLine(GL.GetShaderInfoLog(frag));
#endif

            GL.AttachShader(Program, vert);
            GL.AttachShader(Program, frag);

            GL.LinkProgram(Program);
#if DEBUG
            Debug.WriteLine(GL.GetProgramInfoLog(Program));
            Debug.WriteLine("----------------");
#endif
            Use();

            if (GL3)
                GL.BindFragDataLocation(Program, 0, "out_frag_colour");

            OnCreate();

            ErrorCheck("create");
        }

        protected virtual void OnCreate()
        {
            return;
        }

        public void Use()
        {
            if (!Active) {
                _sCurProgram = this;
                GL.UseProgram(Program);
            }
        }

        public void AddAttribute(String identifier, int size, int divisor = 0, int inputOffset = -1,
            VertexAttribPointerType pointerType = VertexAttribPointerType.Float,
            bool normalize = false)
        {
            if (inputOffset == -1)
                inputOffset = VertexDataSize;

            AttributeInfo info = new AttributeInfo(this, identifier, size, VertexDataStride,
                divisor, inputOffset - VertexDataSize, pointerType, normalize);

            VertexDataStride += info.Length;
            VertexDataSize += info.Size;
            _Attributes.Add(info);

            ErrorCheck("addattrib:" + identifier);
        }

        public void AddTexture(String identifier, TextureUnit unit)
        {
            _Textures.Add(identifier, new TextureInfo(this, identifier,
                unit));

            ErrorCheck("addtexture");
        }

        public void SetTexture(String identifier, Texture texture)
        {
            if (started) {
                GL.End();
                ErrorCheck("end");
            }

            _Textures[identifier].SetCurrentTexture(texture);

            ErrorCheck("settexture");

            if (started)
                GL.Begin(BeginMode);
        }

        public void Begin()
        {
            StartBatch();

            foreach (AttributeInfo info in _Attributes)
                GL.VertexAttribPointer(info.Location, info.Size,
                    info.PointerType, info.Normalize, VertexDataStride, info.Offset);

            ErrorCheck("begin");
            GL.Begin(BeginMode);

            started = true;
        }

        public void StartBatch()
        {
            Use();

            OnStartBatch();
        }

        protected virtual void OnStartBatch()
        {

        }

        public void End()
        {
            started = false;
            GL.End();

            EndBatch();

            ErrorCheck("end");
        }

        public void EndBatch()
        {
            OnEndBatch();
        }

        protected virtual void OnEndBatch()
        {

        }

        public virtual void Render(float[] data)
        {
            if (!started)
                throw new Exception("Mu_s call Begin() fir_s!");

            int i = 0;
            while (i < data.Length) {
                foreach (AttributeInfo attr in _Attributes) {
                    int offset = attr.InputOffset;

                    switch (attr.Size) {
                        case 1:
                            GL.VertexAttrib1(attr.Location,
                                data[i++ + offset]);
                            break;
                        case 2:
                            GL.VertexAttrib2(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                        case 3:
                            GL.VertexAttrib3(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                        case 4:
                            GL.VertexAttrib4(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                    }
                }
            }
        }
    }

    public class ShaderProgram2D : ShaderProgram
    {
        public ShaderProgram2D()
            : base()
        {

        }

        public ShaderProgram2D(int width, int height)
            : base()
        {
            Create();
            SetScreenSize(width, height);
        }

        public void SetScreenSize(int width, int height)
        {
            int loc = GL.GetUniformLocation(Program, "screen_resolution");
            GL.Uniform2(loc, (float) width, (float) height);

            ErrorCheck("screensize");
        }
    }
}
