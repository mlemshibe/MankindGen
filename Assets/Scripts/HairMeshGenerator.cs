using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ頭髪メッシュ生成
    /// </summary>
    public static class HairMeshGenerator
    {
        private const float BASE_HEAD_WIDTH = 0.18f;
        private const float BASE_HEAD_HEIGHT = 0.22f;
        private const float BASE_HEAD_DEPTH = 0.20f;

        /// <summary>
        /// 髪メッシュを生成
        /// </summary>
        public static Mesh Generate(HumanParameters param)
        {
            if (param.hairStyle == HairStyle.Bald)
                return null;

            MeshBuilder builder = new MeshBuilder();

            float width = BASE_HEAD_WIDTH * param.headWidth;
            float height = BASE_HEAD_HEIGHT * param.headHeight;
            float depth = BASE_HEAD_DEPTH * param.headDepth;
            float volume = param.hairVolume;

            switch (param.hairStyle)
            {
                case HairStyle.Short:
                    GenerateShortHair(builder, width, height, depth, volume, param.hairLength);
                    break;
                case HairStyle.Medium:
                    GenerateMediumHair(builder, width, height, depth, volume, param.hairLength);
                    break;
                case HairStyle.Long:
                    GenerateLongHair(builder, width, height, depth, volume, param.hairLength);
                    break;
                case HairStyle.Spiky:
                    GenerateSpikyHair(builder, width, height, depth, volume, param.hairLength);
                    break;
                case HairStyle.Slicked:
                    GenerateSlickedHair(builder, width, height, depth, volume, param.hairLength);
                    break;
            }

            return builder.ToMesh("HairMesh");
        }

        /// <summary>
        /// 短髪（頭部を薄く覆う）
        /// </summary>
        private static void GenerateShortHair(MeshBuilder builder, float width, float height, float depth, float volume, float length)
        {
            float hairOffset = 0.01f * volume;
            float topY = height * 0.5f + hairOffset;
            // lengthで短髪の下端位置を調整（0.0で耳上、1.0で耳下まで）
            float bottomExtend = length * 0.08f;

            // 頭頂部を覆うキャップ形状
            int segments = 8;
            float[] yLevels = { topY + 0.02f, height * 0.35f, height * 0.15f - bottomExtend };
            float[] widthFactors = { 0.5f, 0.95f, 1.0f };
            float[] depthFactors = { 0.6f, 0.9f, 0.95f };

            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = (width + hairOffset * 2) * widthFactors[level];
                float d = (depth + hairOffset * 2) * depthFactors[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;
                    // 前面（顔側）は覆わない
                    if (angle > -Mathf.PI * 0.3f && angle < Mathf.PI * 0.3f)
                    {
                        // 前頭部は額のラインに留める
                        float x = Mathf.Cos(angle) * w * 0.5f;
                        float z = depth * 0.3f; // 額のあたりで止める
                        float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
                        float v = (y + height * 0.5f) / height;
                        levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, v));
                    }
                    else
                    {
                        float x = Mathf.Cos(angle) * w * 0.5f;
                        float z = Mathf.Sin(angle) * d * 0.5f;
                        float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
                        float v = (y + height * 0.5f) / height;
                        levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, v));
                    }
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
            int topCenter = builder.AddVertex(new Vector3(0, topY + 0.03f, -depth * 0.1f), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }
        }

        /// <summary>
        /// 中髪（耳下まで）
        /// </summary>
        private static void GenerateMediumHair(MeshBuilder builder, float width, float height, float depth, float volume, float length)
        {
            float hairOffset = 0.015f * volume;
            float hairLength = 0.08f + length * 0.06f;

            int segments = 8;
            float[] yLevels = { height * 0.55f, height * 0.35f, height * 0.1f, -height * 0.1f, -height * 0.3f - hairLength };
            float[] widthFactors = { 0.55f, 1.0f, 1.05f, 1.0f, 0.9f };
            float[] depthFactors = { 0.65f, 0.95f, 1.0f, 1.0f, 0.95f };

            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = (width + hairOffset * 2) * widthFactors[level];
                float d = (depth + hairOffset * 2) * depthFactors[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;

                    // 前面は額で止める
                    float zLimit = (level <= 2) ? depth * 0.35f : depth * 0.2f;

                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f;

                    if (z > zLimit && level <= 2)
                        z = zLimit;

                    float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
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
            int topCenter = builder.AddVertex(new Vector3(0, height * 0.58f, -depth * 0.1f), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }
        }

        /// <summary>
        /// 長髪（肩まで）
        /// </summary>
        private static void GenerateLongHair(MeshBuilder builder, float width, float height, float depth, float volume, float length)
        {
            float hairOffset = 0.02f * volume;
            float hairLength = 0.15f + length * 0.15f;

            int segments = 10;
            float[] yLevels = { height * 0.55f, height * 0.35f, height * 0.1f, -height * 0.15f, -height * 0.35f, -height * 0.5f - hairLength * 0.5f, -height * 0.5f - hairLength };
            float[] widthFactors = { 0.5f, 1.0f, 1.1f, 1.1f, 1.05f, 0.95f, 0.8f };
            float[] depthFactors = { 0.6f, 0.95f, 1.0f, 1.0f, 0.95f, 0.85f, 0.7f };

            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = (width + hairOffset * 2) * widthFactors[level];
                float d = (depth + hairOffset * 2) * depthFactors[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;

                    float zLimit = (level <= 2) ? depth * 0.4f : depth * 0.25f;

                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f;

                    if (z > zLimit && level <= 2)
                        z = zLimit;

                    float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
                    float v = (y + height * 0.5f + hairLength) / (height + hairLength);
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
            int topCenter = builder.AddVertex(new Vector3(0, height * 0.58f, -depth * 0.1f), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }

            // 髪の先端を閉じる
            int bottomCenter = builder.AddVertex(new Vector3(0, yLevels[yLevels.Length - 1] - 0.01f, -depth * 0.2f), new Vector2(0.5f, 0f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(bottomCenter, levelVertices[yLevels.Length - 1][seg], levelVertices[yLevels.Length - 1][nextSeg]);
            }
        }

        /// <summary>
        /// スパイキーヘア（PS1らしい尖った髪）
        /// </summary>
        private static void GenerateSpikyHair(MeshBuilder builder, float width, float height, float depth, float volume, float length)
        {
            float hairOffset = 0.01f * volume;
            // lengthでスパイクの高さを調整
            float spikeHeight = 0.04f * volume + length * 0.04f;

            // ベース部分
            int segments = 8;
            float baseY = height * 0.3f;
            float topY = height * 0.5f + hairOffset;

            // ベースリング
            int[] baseVerts = new int[segments];
            for (int seg = 0; seg < segments; seg++)
            {
                float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;
                float w = (width + hairOffset * 2) * 0.95f;
                float d = (depth + hairOffset * 2) * 0.9f;

                float zLimit = depth * 0.35f;
                float x = Mathf.Cos(angle) * w * 0.5f;
                float z = Mathf.Sin(angle) * d * 0.5f;
                if (z > zLimit) z = zLimit;

                float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
                baseVerts[seg] = builder.AddVertex(new Vector3(x, baseY, z), new Vector2(u, 0.3f));
            }

            // スパイクを追加
            int spikeCount = 6;
            for (int i = 0; i < spikeCount; i++)
            {
                float angle = (i / (float)spikeCount) * Mathf.PI * 1.5f - Mathf.PI * 0.75f;
                float baseRadius = width * 0.4f;

                float spikeX = Mathf.Cos(angle) * baseRadius * 0.3f;
                float spikeZ = Mathf.Sin(angle) * depth * 0.25f - depth * 0.1f;

                // スパイクの頂点
                Vector3 spikeTip = new Vector3(
                    spikeX + Mathf.Cos(angle) * spikeHeight * 0.5f,
                    topY + spikeHeight,
                    spikeZ + Mathf.Sin(angle) * spikeHeight * 0.3f
                );

                // スパイクのベース（三角形）
                float baseSize = 0.025f;
                Vector3 base1 = new Vector3(spikeX - baseSize, topY, spikeZ);
                Vector3 base2 = new Vector3(spikeX + baseSize, topY, spikeZ);
                Vector3 base3 = new Vector3(spikeX, topY, spikeZ - baseSize);

                Vector2 uvTip = new Vector2(0.5f, 1f);
                Vector2 uvBase = new Vector2(0.5f, 0.5f);

                builder.AddTriFace(spikeTip, base2, base1, uvTip, uvBase, uvBase);
                builder.AddTriFace(spikeTip, base3, base2, uvTip, uvBase, uvBase);
                builder.AddTriFace(spikeTip, base1, base3, uvTip, uvBase, uvBase);
                builder.AddTriFace(base1, base2, base3, uvBase, uvBase, uvBase);
            }

            // 頭頂部のベースキャップ
            int topCenter = builder.AddVertex(new Vector3(0, topY, -depth * 0.05f), new Vector2(0.5f, 0.8f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, baseVerts[nextSeg], baseVerts[seg]);
            }
        }

        /// <summary>
        /// オールバック（後ろに流した髪）
        /// </summary>
        private static void GenerateSlickedHair(MeshBuilder builder, float width, float height, float depth, float volume, float length)
        {
            float hairOffset = 0.01f * volume;
            // lengthで後ろ髪の長さを調整
            float backExtend = length * 0.1f;

            int segments = 8;
            float[] yLevels = { height * 0.52f, height * 0.4f, height * 0.2f, height * 0.0f, -height * 0.15f - backExtend };
            float[] widthFactors = { 0.6f, 0.9f, 1.0f, 0.95f, 0.85f };
            float[] depthFactors = { 0.7f, 0.95f, 1.05f, 1.1f, 1.0f + length * 0.1f };
            float[] zOffsets = { -0.02f, -0.01f, 0f, 0.01f, 0.02f - backExtend * 0.3f };

            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = (width + hairOffset * 2) * widthFactors[level];
                float d = (depth + hairOffset * 2) * depthFactors[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f - Mathf.PI * 0.5f;

                    // 前面は額のラインで止める
                    float zLimit = depth * 0.4f;

                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f + zOffsets[level];

                    if (z > zLimit && level <= 1)
                        z = zLimit;

                    float u = (angle + Mathf.PI * 0.5f) / (Mathf.PI * 2f);
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
            int topCenter = builder.AddVertex(new Vector3(0, height * 0.54f, -depth * 0.15f), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }
        }
    }
}
