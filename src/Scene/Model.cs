using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using OpenTK;

using ComputerGraphicsCoursework.Shaders;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class containing a list of vertices grouped into faces.
    /// </summary>
    public class Model :
        IRenderable<ModelShader>,
        IRenderable<DepthClipShader>
    {
        #region Private Enum VertData
        private enum VertData : byte
        {
            Vertex = 0,
            TextUV = 1,
            Normal = 2
        }
        #endregion

        #region Private Class Face
        /// <summary>
        /// Class representing a single triangular face. Contains a position, normal,
        /// and UV coordinate for each of the three vertices in the face.
        /// </summary>
        private class Face
        {
            #region Private Static Fields
            private static readonly Regex _sREGroup = new Regex("[0-9]*(/[0-9]*){2}");
            #endregion

            #region Private Fields
            private int[,] _indices;
            #endregion

            /// <summary>
            /// Parses a face from a string.
            /// </summary>
            /// <param name="str">String of form "f p1/t1/n1 p2/t2/n2 p3/t3/n3"</param>
            /// <returns></returns>
            public static Face Parse(String str)
            {
                // Create an empty 3x3 array to store the vertex indices during parsing
                var indices = new int[3, 3];
               
                int i = 0;

                // Loop through each substring matching the regex for
                // a triplet of vertex indices
                var match = _sREGroup.Match(str);
                while (match.Success) {
                    var group = match.Value;
                    int prev = 0;
                    int next = -1;
                    int j = 0;

                    // Loop through each index, parse it to an integer and store it
                    // in the indices array
                    while (j < 3) {
                        prev = next + 1;
                        next = group.IndexOf('/', prev);
                        next = next == -1 ? group.Length : next;

                        // Some indices may be omitted, if they are store a default
                        // value of -1
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

                // Return a new face using the parsed indices
                return new Face(indices);
            }

            /// <summary>
            /// Gets a specific index when given a vertex number and the index type
            /// </summary>
            /// <param name="vert">Vertex number (0, 1, or 2)</param>
            /// <param name="type">Type of the index to get</param>
            /// <returns></returns>
            public int this[int vert, VertData type]
            {
                get { return _indices[vert, (int) type]; }
            }

            /// <summary>
            /// Private constructor to create a new Face instance.
            /// </summary>
            /// <param name="indices">3x3 array of indices for this face</param>
            private Face(int[,] indices)
            {
                _indices = indices;
            }
        }
        #endregion

        #region Public Class FaceGroup
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
        #endregion

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

        public FaceGroup[] GetFaceGroups(params String[] prefixes)
        {
            prefixes = prefixes.Select(x => x + "_").ToArray();
            return FaceGroups.Where(x => prefixes.Any(y => x.Name.StartsWith(y))).ToArray();
        }

        public void Render(ModelShader shader)
        {
            _vb.Begin(shader);
            _vb.Render();
            _vb.End();
        }

        public void Render(ModelShader shader, params FaceGroup[] facegroups)
        {
            _vb.Begin(shader);
            foreach (var facegroup in facegroups) {
                _vb.Render(facegroup.StartIndex * 3, facegroup.Length * 3);
            }
            _vb.End();
        }

        public void Render(DepthClipShader shader)
        {
            _vb.Begin(shader);
            _vb.Render();
            _vb.End();
        }

        public void Render(DepthClipShader shader, params FaceGroup[] facegroups)
        {
            _vb.Begin(shader);
            foreach (var facegroup in facegroups) {
                _vb.Render(facegroup.StartIndex * 3, facegroup.Length * 3);
            }
            _vb.End();
        }
    }
}
