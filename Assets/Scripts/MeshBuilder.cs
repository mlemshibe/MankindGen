using UnityEngine;
using System.Collections.Generic;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリメッシュ生成のためのユーティリティクラス
    /// </summary>
    public class MeshBuilder
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> normals = new List<Vector3>();

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();
            normals.Clear();
        }

        public int VertexCount => vertices.Count;

        public int AddVertex(Vector3 position, Vector2 uv)
        {
            vertices.Add(position);
            uvs.Add(uv);
            return vertices.Count - 1;
        }

        public int AddVertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            vertices.Add(position);
            uvs.Add(uv);
            normals.Add(normal);
            return vertices.Count - 1;
        }

        public void AddTriangle(int v0, int v1, int v2)
        {
            triangles.Add(v0);
            triangles.Add(v1);
            triangles.Add(v2);
        }

        public void AddQuad(int v0, int v1, int v2, int v3)
        {
            AddTriangle(v0, v1, v2);
            AddTriangle(v0, v2, v3);
        }

        /// <summary>
        /// ローポリボックスを追加（PS1風のブロッキーな形状の基本）
        /// </summary>
        public void AddBox(Vector3 center, Vector3 size, Vector2 uvMin, Vector2 uvMax)
        {
            Vector3 half = size * 0.5f;

            // 8頂点のボックス
            Vector3[] corners = new Vector3[]
            {
                center + new Vector3(-half.x, -half.y, -half.z),
                center + new Vector3(half.x, -half.y, -half.z),
                center + new Vector3(half.x, -half.y, half.z),
                center + new Vector3(-half.x, -half.y, half.z),
                center + new Vector3(-half.x, half.y, -half.z),
                center + new Vector3(half.x, half.y, -half.z),
                center + new Vector3(half.x, half.y, half.z),
                center + new Vector3(-half.x, half.y, half.z),
            };

            // 各面を追加
            // Front (Z+)
            AddQuadFace(corners[3], corners[2], corners[6], corners[7], uvMin, uvMax);
            // Back (Z-)
            AddQuadFace(corners[1], corners[0], corners[4], corners[5], uvMin, uvMax);
            // Right (X+)
            AddQuadFace(corners[2], corners[1], corners[5], corners[6], uvMin, uvMax);
            // Left (X-)
            AddQuadFace(corners[0], corners[3], corners[7], corners[4], uvMin, uvMax);
            // Top (Y+)
            AddQuadFace(corners[7], corners[6], corners[5], corners[4], uvMin, uvMax);
            // Bottom (Y-)
            AddQuadFace(corners[0], corners[1], corners[2], corners[3], uvMin, uvMax);
        }

        /// <summary>
        /// テーパー付きボックス（上部が狭まる形状）
        /// </summary>
        public void AddTaperedBox(Vector3 center, Vector3 bottomSize, Vector3 topSize, float height,
            Vector2 uvMin, Vector2 uvMax)
        {
            Vector3 halfBottom = bottomSize * 0.5f;
            Vector3 halfTop = topSize * 0.5f;
            float halfHeight = height * 0.5f;

            Vector3[] corners = new Vector3[]
            {
                center + new Vector3(-halfBottom.x, -halfHeight, -halfBottom.z),
                center + new Vector3(halfBottom.x, -halfHeight, -halfBottom.z),
                center + new Vector3(halfBottom.x, -halfHeight, halfBottom.z),
                center + new Vector3(-halfBottom.x, -halfHeight, halfBottom.z),
                center + new Vector3(-halfTop.x, halfHeight, -halfTop.z),
                center + new Vector3(halfTop.x, halfHeight, -halfTop.z),
                center + new Vector3(halfTop.x, halfHeight, halfTop.z),
                center + new Vector3(-halfTop.x, halfHeight, halfTop.z),
            };

            // Front
            AddQuadFace(corners[3], corners[2], corners[6], corners[7], uvMin, uvMax);
            // Back
            AddQuadFace(corners[1], corners[0], corners[4], corners[5], uvMin, uvMax);
            // Right
            AddQuadFace(corners[2], corners[1], corners[5], corners[6], uvMin, uvMax);
            // Left
            AddQuadFace(corners[0], corners[3], corners[7], corners[4], uvMin, uvMax);
            // Top
            AddQuadFace(corners[7], corners[6], corners[5], corners[4], uvMin, uvMax);
            // Bottom
            AddQuadFace(corners[0], corners[1], corners[2], corners[3], uvMin, uvMax);
        }

        /// <summary>
        /// ローポリ円柱（四角柱ベース、PS1風）
        /// </summary>
        public void AddCylinder(Vector3 center, float radiusBottom, float radiusTop, float height,
            int segments, Vector2 uvMin, Vector2 uvMax)
        {
            segments = Mathf.Max(4, segments);
            float halfHeight = height * 0.5f;
            float angleStep = 360f / segments;

            int baseIndex = vertices.Count;

            // 底面と上面の頂点
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                // 底面
                Vector3 bottomPos = center + new Vector3(cos * radiusBottom, -halfHeight, sin * radiusBottom);
                float u = Mathf.Lerp(uvMin.x, uvMax.x, (float)i / segments);
                AddVertex(bottomPos, new Vector2(u, uvMin.y));

                // 上面
                Vector3 topPos = center + new Vector3(cos * radiusTop, halfHeight, sin * radiusTop);
                AddVertex(topPos, new Vector2(u, uvMax.y));
            }

            // 側面
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int b0 = baseIndex + i * 2;
                int t0 = baseIndex + i * 2 + 1;
                int b1 = baseIndex + next * 2;
                int t1 = baseIndex + next * 2 + 1;
                AddQuad(b0, b1, t1, t0);
            }

            // 底面キャップ
            int bottomCenter = AddVertex(center + Vector3.down * halfHeight, (uvMin + uvMax) * 0.5f);
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                AddTriangle(bottomCenter, baseIndex + next * 2, baseIndex + i * 2);
            }

            // 上面キャップ
            int topCenter = AddVertex(center + Vector3.up * halfHeight, (uvMin + uvMax) * 0.5f);
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                AddTriangle(topCenter, baseIndex + i * 2 + 1, baseIndex + next * 2 + 1);
            }
        }

        /// <summary>
        /// 四角面を追加
        /// </summary>
        public void AddQuadFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uvMin, Vector2 uvMax)
        {
            int i0 = AddVertex(v0, new Vector2(uvMin.x, uvMin.y));
            int i1 = AddVertex(v1, new Vector2(uvMax.x, uvMin.y));
            int i2 = AddVertex(v2, new Vector2(uvMax.x, uvMax.y));
            int i3 = AddVertex(v3, new Vector2(uvMin.x, uvMax.y));
            AddQuad(i0, i1, i2, i3);
        }

        /// <summary>
        /// 三角面を追加
        /// </summary>
        public void AddTriFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            int i0 = AddVertex(v0, uv0);
            int i1 = AddVertex(v1, uv1);
            int i2 = AddVertex(v2, uv2);
            AddTriangle(i0, i1, i2);
        }

        /// <summary>
        /// ローポリ球（六面体ベース、PS1風）
        /// </summary>
        public void AddLowPolySphere(Vector3 center, Vector3 radius, int subdivisions, Vector2 uvMin, Vector2 uvMax)
        {
            subdivisions = Mathf.Clamp(subdivisions, 1, 3);

            // 六面体から開始して細分化
            Vector3[] baseVerts = new Vector3[]
            {
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
            };

            int[][] baseFaces = new int[][]
            {
                new int[] { 0, 4, 2 },
                new int[] { 0, 2, 5 },
                new int[] { 0, 5, 3 },
                new int[] { 0, 3, 4 },
                new int[] { 1, 2, 4 },
                new int[] { 1, 5, 2 },
                new int[] { 1, 3, 5 },
                new int[] { 1, 4, 3 },
            };

            List<Vector3> sphereVerts = new List<Vector3>(baseVerts);
            List<int[]> sphereFaces = new List<int[]>(baseFaces);

            // 細分化
            for (int s = 0; s < subdivisions; s++)
            {
                List<int[]> newFaces = new List<int[]>();
                Dictionary<long, int> midpointCache = new Dictionary<long, int>();

                foreach (var face in sphereFaces)
                {
                    int m01 = GetMidpoint(face[0], face[1], sphereVerts, midpointCache);
                    int m12 = GetMidpoint(face[1], face[2], sphereVerts, midpointCache);
                    int m20 = GetMidpoint(face[2], face[0], sphereVerts, midpointCache);

                    newFaces.Add(new int[] { face[0], m01, m20 });
                    newFaces.Add(new int[] { m01, face[1], m12 });
                    newFaces.Add(new int[] { m20, m12, face[2] });
                    newFaces.Add(new int[] { m01, m12, m20 });
                }
                sphereFaces = newFaces;
            }

            int baseIndex = vertices.Count;

            // 頂点追加
            foreach (var v in sphereVerts)
            {
                Vector3 scaled = Vector3.Scale(v.normalized, radius);
                float u = Mathf.Lerp(uvMin.x, uvMax.x, (v.x + 1f) * 0.5f);
                float vCoord = Mathf.Lerp(uvMin.y, uvMax.y, (v.y + 1f) * 0.5f);
                AddVertex(center + scaled, new Vector2(u, vCoord));
            }

            // 面追加
            foreach (var face in sphereFaces)
            {
                AddTriangle(baseIndex + face[0], baseIndex + face[1], baseIndex + face[2]);
            }
        }

        private int GetMidpoint(int i0, int i1, List<Vector3> verts, Dictionary<long, int> cache)
        {
            long key = ((long)Mathf.Min(i0, i1) << 32) + Mathf.Max(i0, i1);
            if (cache.TryGetValue(key, out int mid))
                return mid;

            Vector3 p0 = verts[i0];
            Vector3 p1 = verts[i1];
            Vector3 midpoint = ((p0 + p1) * 0.5f).normalized;
            int idx = verts.Count;
            verts.Add(midpoint);
            cache[key] = idx;
            return idx;
        }

        /// <summary>
        /// 頂点を変形（スケール）
        /// </summary>
        public void ScaleVertices(int startIndex, int count, Vector3 center, Vector3 scale)
        {
            for (int i = startIndex; i < startIndex + count && i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                v = center + Vector3.Scale(v - center, scale);
                vertices[i] = v;
            }
        }

        /// <summary>
        /// 頂点を移動
        /// </summary>
        public void TranslateVertices(int startIndex, int count, Vector3 offset)
        {
            for (int i = startIndex; i < startIndex + count && i < vertices.Count; i++)
            {
                vertices[i] += offset;
            }
        }

        /// <summary>
        /// メッシュを生成
        /// </summary>
        public Mesh ToMesh(string name = "GeneratedMesh")
        {
            Mesh mesh = new Mesh();
            mesh.name = name;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            if (normals.Count == vertices.Count)
            {
                mesh.normals = normals.ToArray();
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// 別のMeshBuilderをマージ
        /// </summary>
        public void Merge(MeshBuilder other)
        {
            int baseIndex = vertices.Count;

            vertices.AddRange(other.vertices);
            uvs.AddRange(other.uvs);

            foreach (int tri in other.triangles)
            {
                triangles.Add(tri + baseIndex);
            }

            if (other.normals.Count == other.vertices.Count)
            {
                normals.AddRange(other.normals);
            }
        }
    }
}
