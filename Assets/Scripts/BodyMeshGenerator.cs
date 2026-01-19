using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ胴体メッシュ生成
    /// </summary>
    public static class BodyMeshGenerator
    {
        // 基本サイズ（メートル単位）
        private const float BASE_TORSO_HEIGHT = 0.55f;
        private const float BASE_SHOULDER_WIDTH = 0.40f;
        private const float BASE_HIP_WIDTH = 0.32f;
        private const float BASE_TORSO_DEPTH = 0.22f;

        /// <summary>
        /// 上半身メッシュを生成（胸から腰まで）
        /// </summary>
        public static Mesh GenerateUpperBody(HumanParameters param)
        {
            MeshBuilder builder = new MeshBuilder();

            float torsoHeight = BASE_TORSO_HEIGHT * param.torsoLength * param.height;
            float shoulderWidth = BASE_SHOULDER_WIDTH * param.shoulderWidth;
            float hipWidth = BASE_HIP_WIDTH * param.hipWidth;
            float depth = BASE_TORSO_DEPTH;

            // 上半身は首から腰まで
            float upperHeight = torsoHeight * 0.55f;

            // Y座標レベル（上から下）
            float[] yLevels = {
                upperHeight * 0.5f,      // 首元
                upperHeight * 0.35f,     // 肩
                upperHeight * 0.1f,      // 胸
                -upperHeight * 0.2f,     // 腹部上
                -upperHeight * 0.5f      // 腹部下（腰）
            };

            // 各レベルでの幅
            float[] widths = {
                shoulderWidth * 0.35f,   // 首
                shoulderWidth,            // 肩
                shoulderWidth * 0.9f,    // 胸
                shoulderWidth * 0.75f,   // 腹部上
                hipWidth * 0.95f         // 腰
            };

            // 各レベルでの奥行き
            float[] depths = {
                depth * 0.4f,
                depth,
                depth * 1.1f,
                depth * 0.95f,
                depth * 0.9f
            };

            GenerateTorsoSection(builder, yLevels, widths, depths, true);

            return builder.ToMesh("UpperBodyMesh");
        }

        /// <summary>
        /// 下半身メッシュを生成（腰から股まで）
        /// </summary>
        public static Mesh GenerateLowerBody(HumanParameters param)
        {
            MeshBuilder builder = new MeshBuilder();

            float torsoHeight = BASE_TORSO_HEIGHT * param.torsoLength * param.height;
            float hipWidth = BASE_HIP_WIDTH * param.hipWidth;
            float depth = BASE_TORSO_DEPTH;

            // 下半身（腰から股）
            float lowerHeight = torsoHeight * 0.35f;

            float[] yLevels = {
                lowerHeight * 0.5f,      // 腰上
                lowerHeight * 0.1f,      // 腰
                -lowerHeight * 0.3f,     // 尻
                -lowerHeight * 0.5f      // 股
            };

            float[] widths = {
                hipWidth * 0.95f,
                hipWidth,
                hipWidth * 0.95f,
                hipWidth * 0.7f
            };

            float[] depths = {
                depth * 0.9f,
                depth * 1.0f,
                depth * 1.1f,
                depth * 0.8f
            };

            GenerateTorsoSection(builder, yLevels, widths, depths, false);

            return builder.ToMesh("LowerBodyMesh");
        }

        /// <summary>
        /// 胴体セクションを生成
        /// </summary>
        private static void GenerateTorsoSection(MeshBuilder builder, float[] yLevels, float[] widths, float[] depths, bool isUpper)
        {
            int segments = 6; // PS1風のローポリ

            int[][] levelVertices = new int[yLevels.Length][];

            for (int level = 0; level < yLevels.Length; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float w = widths[level];
                float d = depths[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * w * 0.5f;
                    float z = Mathf.Sin(angle) * d * 0.5f;

                    // UV: 衣服テクスチャ用
                    float u = (float)seg / segments;
                    float v = (float)level / (yLevels.Length - 1);

                    levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, 1f - v));
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

            // 上面を閉じる（上半身の場合は首元）
            if (isUpper)
            {
                int topCenter = builder.AddVertex(new Vector3(0, yLevels[0] + 0.01f, 0), new Vector2(0.5f, 1f));
                for (int seg = 0; seg < segments; seg++)
                {
                    int nextSeg = (seg + 1) % segments;
                    builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
                }
            }

            // 下面を閉じる
            int bottomCenter = builder.AddVertex(new Vector3(0, yLevels[yLevels.Length - 1] - 0.01f, 0), new Vector2(0.5f, 0f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(bottomCenter, levelVertices[yLevels.Length - 1][seg], levelVertices[yLevels.Length - 1][nextSeg]);
            }
        }

        /// <summary>
        /// 首メッシュを生成
        /// </summary>
        public static Mesh GenerateNeck(HumanParameters param)
        {
            MeshBuilder builder = new MeshBuilder();

            float neckHeight = 0.06f * param.height;
            float neckRadius = 0.045f;

            builder.AddCylinder(Vector3.zero, neckRadius, neckRadius * 0.9f, neckHeight, 6,
                new Vector2(0, 0), new Vector2(1, 1));

            return builder.ToMesh("NeckMesh");
        }
    }
}
