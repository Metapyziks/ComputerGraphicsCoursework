using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

using OpenTK;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework
{
    public class Model : IRenderable<ModelShader>, IRenderable<DepthClipShader>
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
                        } else {
                            indices[i, j] = -1;
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

        public class FaceGroup
        {
            public readonly String Name;
            public int StartIndex { get; internal set; }
            public int Length { get; internal set; }

            public FaceGroup(String name)
            {
                Name = name;
            }
        }

        private static readonly Regex _sRENumb = new Regex("-?[0-9]+(\\.[0-9]+)?");
        private static readonly Regex _sREObjc = new Regex("^o\\s.*$");
        private static readonly Regex _sREVert = new Regex("^v(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sRETxUV = new Regex("^vt(\\s+-?[0-9]+(\\.[0-9]+)?){2}$");
        private static readonly Regex _sRENorm = new Regex("^vn(\\s+-?[0-9]+(\\.[0-9]+)?){3}$");
        private static readonly Regex _sREFace = new Regex("^f(\\s+[0-9]*(/[0-9]*){2}){3}$");

        private static Vector2 ParseVector2(String str)
        {
            var match = _sRENumb.Match(str);
            var vector = new Vector2();
            vector.X = Single.Parse(match.Value); match = match.NextMatch();
            vector.Y = Single.Parse(match.Value);
            return vector;
        }

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
            int vertCount = 0, txuvCount = 0, normCount = 0, faceCount = 0;

            var vertGroups = new List<FaceGroup>();
            FaceGroup lastGroup = null;

            foreach (var line in lines) {
                if (_sREObjc.IsMatch(line)) {
                    if (lastGroup != null) {
                        lastGroup.Length = faceCount - lastGroup.StartIndex;
                    }
                    lastGroup = new FaceGroup(line.Substring(line.IndexOf(' ') + 1)) { StartIndex = faceCount };
                    vertGroups.Add(lastGroup);
                }
                if (_sREVert.IsMatch(line)) {
                    ++vertCount; continue;
                }
                if (_sRETxUV.IsMatch(line)) {
                    ++txuvCount; continue;
                }
                if (_sRENorm.IsMatch(line)) {
                    ++normCount; continue;
                }
                if (_sREFace.IsMatch(line)) {
                    ++faceCount; continue;
                }
            }
            if (lastGroup != null) {
                lastGroup.Length = faceCount - lastGroup.StartIndex;
            }

            var verts = new Vector3[vertCount]; int vi = 0;
            var txuvs = new Vector2[txuvCount]; int ti = 0;
            var norms = new Vector3[normCount]; int ni = 0;
            var faces = new Face[faceCount]; int fi = 0;

            var model = new Model(vertGroups.ToArray(), verts, txuvs, norms, faces);

            foreach (var line in lines) {
                var data = line.Substring(line.IndexOf(' ') + 1);
                if (_sREVert.IsMatch(line)) {
                    verts[vi++] = ParseVector3(data);
                    continue;
                }
                if (_sRETxUV.IsMatch(line)) {
                    txuvs[ti++] = ParseVector2(data);
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

        public readonly FaceGroup[] FaceGroups;

        private readonly Vector3[] _verts;
        private readonly Vector2[] _txuvs;
        private readonly Vector3[] _norms;
        private readonly Face[] _faces;

        private VertexBuffer _vb;

        private Model(FaceGroup[] faceGroups, Vector3[] verts, Vector2[] txuvs, Vector3[] norms, Face[] faces)
        {
            FaceGroups = faceGroups;

            _verts = verts;
            _txuvs = txuvs;
            _norms = norms;
            _faces = faces;

            _vb = new VertexBuffer(8);
        }

        public void UpdateVertices()
        {
            float[] raw = new float[8 * 3 * _faces.Length];
            int i = 0;
            foreach (var face in _faces) {
                for (int j = 0; j < 3; ++j) {
                    raw[i++] = _verts[face[j, VertData.Vertex]].X;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Y;
                    raw[i++] = _verts[face[j, VertData.Vertex]].Z;
                    if (face[j, VertData.TextUV] > -1) {
                        raw[i++] = _txuvs[face[j, VertData.TextUV]].X;
                        raw[i++] = _txuvs[face[j, VertData.TextUV]].Y;
                    } else {
                        raw[i++] = 0f;
                        raw[i++] = 0f;
                    }
                    raw[i++] = _norms[face[j, VertData.Normal]].X;
                    raw[i++] = _norms[face[j, VertData.Normal]].Y;
                    raw[i++] = _norms[face[j, VertData.Normal]].Z;
                }
            }
            _vb.SetData(raw);
        }

        public FaceGroup[] GetFaceGroups(String prefix)
        {
            prefix = prefix + "_";
            return FaceGroups.Where(x => x.Name.StartsWith(prefix)).ToArray();
        }

        public void Render(ModelShader shader)
        {
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }

        public void Render(ModelShader shader, params FaceGroup[] facegroups)
        {
            _vb.StartBatch(shader);
            foreach (var facegroup in facegroups) {
                _vb.Render(shader, facegroup.StartIndex * 3, facegroup.Length * 3);
            }
            _vb.EndBatch(shader);
        }

        public void Render(DepthClipShader shader)
        {
            _vb.StartBatch(shader);
            _vb.Render(shader);
            _vb.EndBatch(shader);
        }

        public void Render(DepthClipShader shader, params FaceGroup[] facegroups)
        {
            _vb.StartBatch(shader);
            foreach (var facegroup in facegroups) {
                _vb.Render(shader, facegroup.StartIndex * 3, facegroup.Length * 3);
            }
            _vb.EndBatch(shader);
        }
    }
}
