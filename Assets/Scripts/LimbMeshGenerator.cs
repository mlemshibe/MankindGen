using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ四肢メッシュ生成
    /// </summary>
    public static class LimbMeshGenerator
    {
        // 基本サイズ
        private const float BASE_ARM_LENGTH = 0.55f;
        private const float BASE_LEG_LENGTH = 0.85f;
        private const float BASE_ARM_RADIUS = 0.035f;
        private const float BASE_LEG_RADIUS = 0.055f;

        /// <summary>
        /// 上腕メッシュを生成
        /// </summary>
        public static Mesh GenerateUpperArm(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_ARM_LENGTH * 0.45f * param.armLength * param.height;
            float radiusTop = BASE_ARM_RADIUS * 1.2f;
            float radiusBottom = BASE_ARM_RADIUS;

            GenerateLimbSegment(builder, length, radiusTop, radiusBottom, 5, true);

            return builder.ToMesh(isLeft ? "LeftUpperArmMesh" : "RightUpperArmMesh");
        }

        /// <summary>
        /// 前腕メッシュを生成
        /// </summary>
        public static Mesh GenerateForearm(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_ARM_LENGTH * 0.45f * param.armLength * param.height;
            float radiusTop = BASE_ARM_RADIUS;
            float radiusBottom = BASE_ARM_RADIUS * 0.85f;

            GenerateLimbSegment(builder, length, radiusTop, radiusBottom, 5, true);

            return builder.ToMesh(isLeft ? "LeftForearmMesh" : "RightForearmMesh");
        }

        /// <summary>
        /// 手メッシュを生成（シンプルなブロック形状）
        /// </summary>
        public static Mesh GenerateHand(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float handLength = 0.08f * param.height;
            float handWidth = 0.045f;
            float handThickness = 0.025f;

            // 手のひら（シンプルなボックス）
            builder.AddBox(
                new Vector3(0, -handLength * 0.4f, 0),
                new Vector3(handWidth, handLength * 0.6f, handThickness),
                new Vector2(0, 0), new Vector2(1, 0.6f)
            );

            // 指（PS1風に簡略化、1ブロック）
            builder.AddBox(
                new Vector3(0, -handLength * 0.85f, 0),
                new Vector3(handWidth * 0.9f, handLength * 0.35f, handThickness * 0.8f),
                new Vector2(0, 0.6f), new Vector2(1, 1)
            );

            return builder.ToMesh(isLeft ? "LeftHandMesh" : "RightHandMesh");
        }

        /// <summary>
        /// 大腿部メッシュを生成
        /// </summary>
        public static Mesh GenerateThigh(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_LEG_LENGTH * 0.45f * param.legLength * param.height;
            float radiusTop = BASE_LEG_RADIUS * 1.3f;
            float radiusBottom = BASE_LEG_RADIUS * 0.9f;

            GenerateLimbSegment(builder, length, radiusTop, radiusBottom, 5, false);

            return builder.ToMesh(isLeft ? "LeftThighMesh" : "RightThighMesh");
        }

        /// <summary>
        /// 下腿部メッシュを生成
        /// </summary>
        public static Mesh GenerateCalf(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_LEG_LENGTH * 0.45f * param.legLength * param.height;
            float radiusTop = BASE_LEG_RADIUS * 0.95f;
            float radiusBottom = BASE_LEG_RADIUS * 0.7f;

            GenerateLimbSegment(builder, length, radiusTop, radiusBottom, 5, false);

            return builder.ToMesh(isLeft ? "LeftCalfMesh" : "RightCalfMesh");
        }

        /// <summary>
        /// 足メッシュを生成
        /// </summary>
        public static Mesh GenerateFoot(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float footLength = 0.12f * param.height;
            float footWidth = 0.05f;
            float footHeight = 0.04f;

            // 足首部分
            builder.AddTaperedBox(
                new Vector3(0, 0, footLength * 0.1f),
                new Vector3(footWidth * 0.8f, 0, footWidth * 0.8f),
                new Vector3(footWidth, 0, footWidth),
                footHeight * 0.5f,
                new Vector2(0, 0), new Vector2(1, 0.3f)
            );

            // 足の甲と底
            builder.AddBox(
                new Vector3(0, -footHeight * 0.3f, footLength * 0.35f),
                new Vector3(footWidth, footHeight * 0.5f, footLength * 0.8f),
                new Vector2(0, 0.3f), new Vector2(1, 1)
            );

            // つま先
            Vector3 toeCenter = new Vector3(0, -footHeight * 0.35f, footLength * 0.85f);
            builder.AddTaperedBox(
                toeCenter,
                new Vector3(footWidth, 0, footLength * 0.15f),
                new Vector3(footWidth * 0.7f, 0, footLength * 0.05f),
                footHeight * 0.35f,
                new Vector2(0, 0), new Vector2(1, 1)
            );

            return builder.ToMesh(isLeft ? "LeftFootMesh" : "RightFootMesh");
        }

        /// <summary>
        /// 四肢セグメントを生成（テーパー付き円柱）
        /// </summary>
        private static void GenerateLimbSegment(MeshBuilder builder, float length, float radiusTop, float radiusBottom, int segments, bool isArm)
        {
            // Y軸方向に伸びる形状（上が広い）
            float halfLength = length * 0.5f;

            int[][] levelVertices = new int[3][];
            float[] yLevels = { halfLength, 0, -halfLength };
            float[] radii = { radiusTop, (radiusTop + radiusBottom) * 0.5f, radiusBottom };

            for (int level = 0; level < 3; level++)
            {
                levelVertices[level] = new int[segments];
                float y = yLevels[level];
                float radius = radii[level];

                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = (seg / (float)segments) * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;

                    float u = (float)seg / segments;
                    float v = (float)level / 2f;

                    levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, 1f - v));
                }
            }

            // 側面を接続
            for (int level = 0; level < 2; level++)
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

            // 上面キャップ
            int topCenter = builder.AddVertex(new Vector3(0, halfLength, 0), new Vector2(0.5f, 1f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
            }

            // 下面キャップ
            int bottomCenter = builder.AddVertex(new Vector3(0, -halfLength, 0), new Vector2(0.5f, 0f));
            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(bottomCenter, levelVertices[2][seg], levelVertices[2][nextSeg]);
            }
        }
    }
}
