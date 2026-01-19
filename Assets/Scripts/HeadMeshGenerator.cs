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

            // 頭部の輪郭レベル（上から下）
            // 頭頂、額上、額、目、頬上、頬、顎上、顎先
            float[] yLevels = {
                height * 0.5f,      // 頭頂
                height * 0.38f,     // 額上
                height * 0.25f,     // 額
                height * 0.08f,     // 目の高さ
                -height * 0.08f,    // 頬上
                -height * 0.22f,    // 頬（エラ位置）
                -height * 0.38f,    // 顎上
                -height * 0.5f      // 顎先
            };

            // jawWidthでエラの張り具合を制御（0=細い、1=張っている）
            float jawFactor = param.jawWidth;
            // chinPointinessで顎の尖り具合（0=丸い、1=尖っている）
            float chinFactor = param.chinPointiness;

            // 各レベルでの幅係数（エラと顎の形状を個性的に）
            float[] widthFactors = {
                0.55f,                                          // 頭頂
                0.85f,                                          // 額上
                0.95f,                                          // 額
                1.0f,                                           // 目
                0.95f + jawFactor * 0.08f,                      // 頬上
                0.75f + jawFactor * 0.25f,                      // 頬（エラ）- jawWidthで大きく変化
                0.45f + jawFactor * 0.15f - chinFactor * 0.1f, // 顎上
                0.15f + (1f - chinFactor) * 0.15f              // 顎先 - chinPointinessで変化
            };

            // 各レベルでの奥行き係数
            float[] depthFactors = {
                0.65f,                          // 頭頂
                0.88f,                          // 額上
                0.95f,                          // 額
                1.0f,                           // 目
                0.98f,                          // 頬上
                0.85f + jawFactor * 0.1f,       // 頬
                0.65f,                          // 顎上
                0.35f + (1f - chinFactor) * 0.1f // 顎先
            };

            // プロファイル角度による前後の調整
            float profileOffset = param.faceProfileAngle * depth * 0.3f;

            // 頭部の輪郭を構築（8セグメント - PS1らしい角ばり）
            int segments = 8;
            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = width * widthFactors[level];
                float d = depth * depthFactors[level];

                // 前面のオフセット（プロファイル）
                float faceOffset = (level >= 2 && level <= 6) ? profileOffset : 0;

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;

                    // 基本形状
                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f;

                    // エラ部分（横向き頂点）を強調
                    if (level >= 4 && level <= 6)
                    {
                        // 横向きの頂点（angle ≈ 0 or π）でエラを強調
                        float sideInfluence = Mathf.Abs(Mathf.Cos(angle));
                        if (sideInfluence > 0.7f)
                        {
                            float elaBonus = jawFactor * 0.015f * sideInfluence;
                            x += Mathf.Sign(x) * elaBonus;
                        }
                    }

                    // 前面寄りの頂点にプロファイルオフセットを適用
                    if (z > 0)
                        z += faceOffset * (z / (d * 0.5f + 0.001f));

                    // 顎部分を前に出す（角ばった顎）
                    if (level >= 6 && z > 0)
                    {
                        z += chinFactor * 0.01f;
                    }

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
            int topCenter = builder.AddVertex(new Vector3(0, height * 0.5f + 0.005f, -depth * 0.05f), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }

            // 顎の底を閉じる
            float chinZ = depth * (0.05f + chinFactor * 0.08f);
            int bottomCenter = builder.AddVertex(new Vector3(0, -height * 0.5f - 0.005f, chinZ), new Vector2(0.5f, 0f));
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
            float noseLength = 0.025f * param.noseLength;
            float noseWidth = 0.022f * param.noseWidth;
            float noseHeight = 0.035f * param.noseLength;

            // 鼻の位置（顔の中央やや下）
            float noseY = headHeight * (param.noseHeight * 0.15f - 0.05f);
            float noseZ = headDepth * 0.48f;

            // 鼻の頂点
            Vector3 noseTip = new Vector3(0, noseY - noseHeight * 0.3f, noseZ + noseLength);
            Vector3 noseTop = new Vector3(0, noseY + noseHeight * 0.5f, noseZ + noseLength * 0.4f);
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

            // 鼻の底面
            builder.AddTriFace(noseTip, noseRightBase, noseLeftBase, uvTip, uvRightBase, uvLeftBase);
        }
    }
}
