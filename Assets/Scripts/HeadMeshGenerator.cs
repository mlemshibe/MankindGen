using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ頭部メッシュ生成（顔輪郭と鼻のみモデリング）
    /// </summary>
    public static class HeadMeshGenerator
    {
        // 基本サイズ（メートル単位）
        private const float BASE_HEAD_WIDTH = 0.18f;
        private const float BASE_HEAD_HEIGHT = 0.22f;
        private const float BASE_HEAD_DEPTH = 0.20f;

        /// <summary>
        /// 頭部メッシュを生成
        /// </summary>
        public static Mesh Generate(HumanParameters param)
        {
            MeshBuilder builder = new MeshBuilder();

            float width = BASE_HEAD_WIDTH * param.headWidth;
            float height = BASE_HEAD_HEIGHT * param.headHeight;
            float depth = BASE_HEAD_DEPTH * param.headDepth;

            // 頭部は8角形ベースの輪郭を持つ
            // 上から: 頭頂部、額、目の高さ、頬、顎
            float[] yLevels = { height * 0.5f, height * 0.3f, height * 0.1f, -height * 0.15f, -height * 0.4f, -height * 0.5f };

            // 各レベルでの幅と奥行きの係数
            float[] widthFactors = { 0.6f, 0.95f, 1.0f, 0.9f, 0.5f * (0.5f + param.jawWidth * 0.5f), 0.2f + param.chinPointiness * 0.15f };
            float[] depthFactors = { 0.7f, 0.95f, 1.0f, 0.95f, 0.8f, 0.5f };

            // プロファイル角度による前後の調整
            float profileOffset = param.faceProfileAngle * depth * 0.3f;

            // 頭部の輪郭を構築（8セグメント）
            int segments = 8;
            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = width * widthFactors[level];
                float d = depth * depthFactors[level];

                // 前面のオフセット（プロファイル）
                float faceOffset = (level >= 1 && level <= 4) ? profileOffset : 0;

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;
                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f;

                    // 前面寄りの頂点にプロファイルオフセットを適用
                    if (z > 0)
                        z += faceOffset * (z / (d * 0.5f));

                    // UV: 顔正面を中央に配置
                    float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
                    if (u > 1f) u -= 1f;
                    float v = (y + height * 0.5f) / height;

                    levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, v));
                }
            }

            // 側面を接続
            for (int level = 0; level < yLevels.Length - 1; level++)
            {
                for (int seg = 0; seg < segments; seg++)
                {
                    int nextSeg = (seg + 1) % segments;
                    builder.AddQuad(
                        levelVertices[level][seg],
                        levelVertices[level][nextSeg],
                        levelVertices[level + 1][nextSeg],
                        levelVertices[level + 1][seg]
                    );
                }
            }

            // 頭頂部を閉じる
            int topCenter = builder.AddVertex(new Vector3(0, height * 0.5f + 0.01f, 0), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }

            // 顎の底を閉じる
            int bottomCenter = builder.AddVertex(new Vector3(0, -height * 0.5f - 0.01f, depth * 0.1f), new Vector2(0.5f, 0f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(bottomCenter, levelVertices[yLevels.Length - 1][seg], levelVertices[yLevels.Length - 1][nextSeg]);
            }

            // 鼻を追加
            AddNose(builder, param, height, depth);

            return builder.ToMesh("HeadMesh");
        }

        /// <summary>
        /// 鼻を追加（シンプルなピラミッド型）
        /// </summary>
        private static void AddNose(MeshBuilder builder, HumanParameters param, float headHeight, float headDepth)
        {
            float noseLength = 0.03f * param.noseLength;
            float noseWidth = 0.025f * param.noseWidth;
            float noseHeight = 0.04f * param.noseLength;

            // 鼻の位置（顔の中央やや下）
            float noseY = headHeight * (param.noseHeight * 0.2f - 0.1f);
            float noseZ = headDepth * 0.5f;

            // 鼻の頂点
            Vector3 noseTip = new Vector3(0, noseY - noseHeight * 0.3f, noseZ + noseLength);
            Vector3 noseTop = new Vector3(0, noseY + noseHeight * 0.5f, noseZ + noseLength * 0.3f);
            Vector3 noseLeftBase = new Vector3(-noseWidth * 0.5f, noseY - noseHeight * 0.5f, noseZ);
            Vector3 noseRightBase = new Vector3(noseWidth * 0.5f, noseY - noseHeight * 0.5f, noseZ);
            Vector3 noseLeftTop = new Vector3(-noseWidth * 0.3f, noseY + noseHeight * 0.3f, noseZ);
            Vector3 noseRightTop = new Vector3(noseWidth * 0.3f, noseY + noseHeight * 0.3f, noseZ);

            // UV（鼻は顔テクスチャの中央下部）
            Vector2 uvTip = new Vector2(0.5f, 0.4f);
            Vector2 uvTop = new Vector2(0.5f, 0.55f);
            Vector2 uvLeftBase = new Vector2(0.45f, 0.35f);
            Vector2 uvRightBase = new Vector2(0.55f, 0.35f);
            Vector2 uvLeftTop = new Vector2(0.47f, 0.5f);
            Vector2 uvRightTop = new Vector2(0.53f, 0.5f);

            // 鼻の上面（左）
            builder.AddTriFace(noseTop, noseTip, noseLeftTop, uvTop, uvTip, uvLeftTop);
            // 鼻の上面（右）
            builder.AddTriFace(noseTop, noseRightTop, noseTip, uvTop, uvRightTop, uvTip);

            // 鼻の側面（左）
            builder.AddTriFace(noseLeftTop, noseTip, noseLeftBase, uvLeftTop, uvTip, uvLeftBase);
            // 鼻の側面（右）
            builder.AddTriFace(noseRightTop, noseRightBase, noseTip, uvRightTop, uvRightBase, uvTip);

            // 鼻の底面（オプション、見えにくい）
            builder.AddTriFace(noseTip, noseRightBase, noseLeftBase, uvTip, uvRightBase, uvLeftBase);

            // 鼻梁
            builder.AddQuadFace(noseLeftTop, noseTop, noseRightTop, noseLeftTop,
                new Vector2(0, 0), new Vector2(1, 1));
        }

        /// <summary>
        /// 顔用UV座標を取得（テクスチャ上の目・鼻・口位置は固定）
        /// </summary>
        public static Vector2 GetFaceUV(float headX, float headY, float headWidth, float headHeight)
        {
            // 顔の正面部分をテクスチャの中央に配置
            // headX: -0.5 to 0.5 (左右)
            // headY: -0.5 to 0.5 (上下)
            float u = 0.5f + headX;
            float v = 0.5f + headY;
            return new Vector2(u, v);
        }
    }
}
