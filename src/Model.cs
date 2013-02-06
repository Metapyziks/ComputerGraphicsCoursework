using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

using OpenTK;

namespace ComputerGraphicsCoursework
{
    class Model
    {
        private enum VertData : byte
        {
            Vertex = 0,
            TextUV = 1,
            Normal = 2
        }

        private class Face
        {
            private static readonly Regex _sREGroup = new Regex("[0-9]*(/[0-9]*){2}");

            private int[,] _indices;

            public static Face Parse(String str)
            {
                var indices = new int[3, 3];

                int i = 0;
                var match = _sREGroup.Match(str);
                while (match.Success) {
                    var group = match.Value;
                    int prev = 0;
                    int next = -1;
                    int j = 0;
                    while (j < 3) {
                        prev = next + 1;
                        next = group.IndexOf('/', prev);
                        next = next == -1 ? group.Length : next;

                        if (next - prev > 0) {
                            indices[i, j] = Int32.Parse(group.Substring(prev, next - prev)) - 1;
                        }
                        ++j;
                    }
                    match = match.NextMatch();
                    ++i;
                }

                return new Face(indices);
            }

            public int this[int vert, VertData type]
            {
                get { return _indices[vert, (int) type]; }
            }

            private Face(int[,] indices)
            {
                _indices = indices;
            }
        }

        private static readonly Regex _sRENumb = new Regex("-?[0-9]+(\\.[0-9]+)?");
        private static readonly Regex _sREVert = new Regex("^v(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sRENorm = new Regex("^vn(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sREFace = new Regex("^f(\\s+[0-9]*(/[0-9]*){2}){3}$");

        private static Vector3 ParseVector3(String str)
        {
            var match = _sRENumb.Match(str);
            var vector = new Vector3();
            vector.X = Single.Parse(match.Value); match = match.NextMatch();
            vector.Y = Single.Parse(match.Value); match = match.NextMatch();
            vector.Z = Single.Parse(match.Value);
            return vector;
        }
        
        public static Model FromFile(String path)
        {
            var lines = File.ReadAllLines(path);
            int vertCount = 0, normCount = 0, faceCount = 0;

            foreach (var line in lines) {
                if (_sREVert.IsMatch(line)) {
                    ++vertCount; continue;
                }
                if (_sRENorm.IsMatch(line)) {
                    ++normCount; continue;
                }
                if (_sREFace.IsMatch(line)) {
                    ++faceCount; continue;
                }
            }

            var verts = new Vector3[vertCount]; int vi = 0;
            var norms = new Vector3[normCount]; int ni = 0;
            var faces = new Face[faceCount]; int fi = 0;

            var model = new Model(verts, norms, faces);

            foreach (var line in lines) {
                var data = line.Substring(line.IndexOf(' ') + 1);
                if (_sREVert.IsMatch(line)) {
                    verts[vi++] = ParseVector3(data);
                    continue;
                }
                if (_sRENorm.IsMatch(line)) {
                    norms[ni++] = ParseVector3(data);
                    continue;
                }
                if (_sREFace.IsMatch(line)) {
                    faces[fi++] = Face.Parse(data);
                    continue;
                }
            }

            model.UpdateVertices();
            return model;
        }

        private readonly Vector3[] _verts;
        private readonly Vector3[] _norms;
        private readonly Face[] _faces;

        private VertexBuffer _vb;

        private Model(Vector3[] verts, Vector3[] norms, Face[] faces)
        {
            _verts = verts;
            _norms = norms;
            _faces = faces;

            _vb = new VertexBuffer(6);
        }

        public void UpdateVertices()
        {
            float[] raw = new float[6 * 3 * _faces.Length];
            int i = 0;
            foreach (var face in _faces) {
                for (int j = 0; j < 3; ++j) {
                    raw[i++] = _verts[face[j, VertData.Vertex]].X;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Y;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Z;
                    raw[i++] = _norms[face[j, VertData.Normal]].X;
                    raw[i++] = _norms[face[j, VertData.Normal]].Y;
                    raw[i++] = _norms[face[j, VertData.Normal]].Z;
                }
            }
            _vb.SetData(raw);
        }

        public void Render(TestShader shader)
        {
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }
    }
}
