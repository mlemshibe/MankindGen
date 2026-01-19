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
        private const float BASE_ARM_RADIUS = 0.032f;
        private const float BASE_LEG_RADIUS = 0.05f;

        /// <summary>
        /// 上腕メッシュを生成（より人間的な形状）
        /// </summary>
        public static Mesh GenerateUpperArm(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_ARM_LENGTH * 0.45f * param.armLength * param.height;
            float radiusTop = BASE_ARM_RADIUS * 1.4f;  // 肩側は太め
            float radiusMid = BASE_ARM_RADIUS * 1.1f;  // 二頭筋部分
            float radiusBottom = BASE_ARM_RADIUS * 0.9f; // 肘側

            GenerateArmSegment(builder, length, radiusTop, radiusMid, radiusBottom, 6);

            return builder.ToMesh(isLeft ? "LeftUpperArmMesh" : "RightUpperArmMesh");
        }

        /// <summary>
        /// 前腕メッシュを生成
        /// </summary>
        public static Mesh GenerateForearm(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_ARM_LENGTH * 0.43f * param.armLength * param.height;
            float radiusTop = BASE_ARM_RADIUS * 0.95f;  // 肘側
            float radiusMid = BASE_ARM_RADIUS * 0.85f;  // 前腕中央
            float radiusBottom = BASE_ARM_RADIUS * 0.7f; // 手首側

            GenerateArmSegment(builder, length, radiusTop, radiusMid, radiusBottom, 6);

            return builder.ToMesh(isLeft ? "LeftForearmMesh" : "RightForearmMesh");
        }

        /// <summary>
        /// 手メッシュを生成（より人間的な形状）
        /// </summary>
        public static Mesh GenerateHand(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float scale = param.height;
            float handLength = 0.075f * scale;
            float handWidth = 0.04f * scale;
            float handThickness = 0.02f * scale;
            float fingerLength = 0.04f * scale;

            // 手のひら（上端がY=0に来るように配置）
            float palmCenterY = -handLength * 0.5f;

            // 手のひらの頂点（8角形ベース）
            int segments = 6;
            float halfWidth = handWidth * 0.5f;
            float halfThick = handThickness * 0.5f;

            // 手首側（上）
            int[] wristVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * halfWidth * 0.85f;
                float z = Mathf.Sin(angle) * halfThick * 0.9f;
                wristVerts[i] = builder.AddVertex(new Vector3(x, 0, z), new Vector2((float)i / segments, 1f));
            }

            // 手のひら中央
            int[] palmVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * halfWidth;
                float z = Mathf.Sin(angle) * halfThick;
                palmVerts[i] = builder.AddVertex(new Vector3(x, palmCenterY, z), new Vector2((float)i / segments, 0.5f));
            }

            // 指の付け根
            int[] fingerBaseVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * halfWidth * 0.9f;
                float z = Mathf.Sin(angle) * halfThick * 0.8f;
                fingerBaseVerts[i] = builder.AddVertex(new Vector3(x, -handLength, z), new Vector2((float)i / segments, 0.3f));
            }

            // 指先（簡略化した1ブロック）
            int[] fingerTipVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * halfWidth * 0.6f;
                float z = Mathf.Sin(angle) * halfThick * 0.5f;
                fingerTipVerts[i] = builder.AddVertex(new Vector3(x, -handLength - fingerLength, z), new Vector2((float)i / segments, 0f));
            }

            // 側面を接続
            ConnectRings(builder, wristVerts, palmVerts, segments);
            ConnectRings(builder, palmVerts, fingerBaseVerts, segments);
            ConnectRings(builder, fingerBaseVerts, fingerTipVerts, segments);

            // 手首側キャップ
            int wristCenter = builder.AddVertex(new Vector3(0, 0, 0), new Vector2(0.5f, 1f));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                builder.AddTriangle(wristCenter, wristVerts[next], wristVerts[i]);
            }

            // 指先キャップ
            int tipCenter = builder.AddVertex(new Vector3(0, -handLength - fingerLength - 0.002f, 0), new Vector2(0.5f, 0f));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                builder.AddTriangle(tipCenter, fingerTipVerts[i], fingerTipVerts[next]);
            }

            return builder.ToMesh(isLeft ? "LeftHandMesh" : "RightHandMesh");
        }

        /// <summary>
        /// 肩関節メッシュを生成
        /// </summary>
        public static Mesh GenerateShoulder(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float radius = BASE_ARM_RADIUS * 1.5f;

            // シンプルな球形の肩関節
            builder.AddLowPolySphere(Vector3.zero, Vector3.one * radius, 1,
                new Vector2(0, 0), new Vector2(1, 1));

            return builder.ToMesh(isLeft ? "LeftShoulderMesh" : "RightShoulderMesh");
        }

        /// <summary>
        /// 大腿部メッシュを生成
        /// </summary>
        public static Mesh GenerateThigh(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_LEG_LENGTH * 0.45f * param.legLength * param.height;
            float radiusTop = BASE_LEG_RADIUS * 1.4f;    // 股関節側
            float radiusMid = BASE_LEG_RADIUS * 1.15f;   // 大腿中央
            float radiusBottom = BASE_LEG_RADIUS * 0.85f; // 膝側

            GenerateLegSegment(builder, length, radiusTop, radiusMid, radiusBottom, 6);

            return builder.ToMesh(isLeft ? "LeftThighMesh" : "RightThighMesh");
        }

        /// <summary>
        /// 下腿部メッシュを生成
        /// </summary>
        public static Mesh GenerateCalf(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float length = BASE_LEG_LENGTH * 0.43f * param.legLength * param.height;
            float radiusTop = BASE_LEG_RADIUS * 0.95f;   // 膝側
            float radiusMid = BASE_LEG_RADIUS * 0.85f;   // ふくらはぎ
            float radiusBottom = BASE_LEG_RADIUS * 0.55f; // 足首側

            GenerateLegSegment(builder, length, radiusTop, radiusMid, radiusBottom, 6);

            return builder.ToMesh(isLeft ? "LeftCalfMesh" : "RightCalfMesh");
        }

        /// <summary>
        /// 足メッシュを生成
        /// </summary>
        public static Mesh GenerateFoot(HumanParameters param, bool isLeft)
        {
            MeshBuilder builder = new MeshBuilder();

            float scale = param.height;
            float footLength = 0.11f * scale;
            float footWidth = 0.045f * scale;
            float footHeight = 0.035f * scale;
            float ankleRadius = BASE_LEG_RADIUS * 0.55f;

            // 足首接続部分（上端がY=0）
            int segments = 6;
            int[] ankleVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * ankleRadius;
                float z = Mathf.Sin(angle) * ankleRadius;
                ankleVerts[i] = builder.AddVertex(new Vector3(x, 0, z), new Vector2((float)i / segments, 1f));
            }

            // 足の甲（上面）
            int[] topVerts = new int[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * footWidth * 0.5f;
                float z = Mathf.Sin(angle) * footLength * 0.3f + footLength * 0.2f;
                topVerts[i] = builder.AddVertex(new Vector3(x, -footHeight * 0.3f, z), new Vector2((float)i / segments, 0.7f));
            }

            // 足底面
            Vector3 heelPos = new Vector3(0, -footHeight, -footLength * 0.15f);
            Vector3 midPos = new Vector3(0, -footHeight, footLength * 0.3f);
            Vector3 toePos = new Vector3(0, -footHeight * 0.8f, footLength * 0.8f);

            // かかと
            int heel = builder.AddVertex(heelPos, new Vector2(0.5f, 0.2f));
            int heelL = builder.AddVertex(heelPos + new Vector3(-footWidth * 0.4f, 0, 0), new Vector2(0.3f, 0.2f));
            int heelR = builder.AddVertex(heelPos + new Vector3(footWidth * 0.4f, 0, 0), new Vector2(0.7f, 0.2f));

            // 土踏まず
            int midL = builder.AddVertex(midPos + new Vector3(-footWidth * 0.45f, 0, 0), new Vector2(0.2f, 0.5f));
            int midR = builder.AddVertex(midPos + new Vector3(footWidth * 0.45f, 0, 0), new Vector2(0.8f, 0.5f));

            // つま先
            int toe = builder.AddVertex(toePos, new Vector2(0.5f, 0f));
            int toeL = builder.AddVertex(toePos + new Vector3(-footWidth * 0.35f, 0, -0.01f), new Vector2(0.3f, 0.05f));
            int toeR = builder.AddVertex(toePos + new Vector3(footWidth * 0.35f, 0, -0.01f), new Vector2(0.7f, 0.05f));

            // 足首から足の甲へ
            ConnectRings(builder, ankleVerts, topVerts, segments);

            // 足の甲から底面へ（簡略化）
            // 上面中央
            int topCenter = builder.AddVertex(new Vector3(0, -footHeight * 0.2f, footLength * 0.3f), new Vector2(0.5f, 0.6f));

            // 底面を構築
            builder.AddTriFace(new Vector3(-footWidth * 0.4f, -footHeight, -footLength * 0.15f),
                              new Vector3(footWidth * 0.4f, -footHeight, -footLength * 0.15f),
                              new Vector3(footWidth * 0.45f, -footHeight, footLength * 0.3f),
                              new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.2f), new Vector2(0.8f, 0.5f));
            builder.AddTriFace(new Vector3(-footWidth * 0.4f, -footHeight, -footLength * 0.15f),
                              new Vector3(footWidth * 0.45f, -footHeight, footLength * 0.3f),
                              new Vector3(-footWidth * 0.45f, -footHeight, footLength * 0.3f),
                              new Vector2(0.3f, 0.2f), new Vector2(0.8f, 0.5f), new Vector2(0.2f, 0.5f));
            builder.AddTriFace(new Vector3(-footWidth * 0.45f, -footHeight, footLength * 0.3f),
                              new Vector3(footWidth * 0.45f, -footHeight, footLength * 0.3f),
                              new Vector3(0, -footHeight * 0.8f, footLength * 0.8f),
                              new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(0.5f, 0f));

            // 側面を閉じる（簡略化）
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                float angle = (i / (float)segments) * Mathf.PI * 2f;

                // 足の甲から底面への接続
                Vector3 bottomPos;
                if (angle < Mathf.PI)
                {
                    // 前側
                    float t = angle / Mathf.PI;
                    bottomPos = Vector3.Lerp(
                        new Vector3(-footWidth * 0.45f, -footHeight, footLength * 0.3f),
                        new Vector3(footWidth * 0.45f, -footHeight, footLength * 0.3f),
                        t
                    );
                }
                else
                {
                    // 後側
                    float t = (angle - Mathf.PI) / Mathf.PI;
                    bottomPos = Vector3.Lerp(
                        new Vector3(footWidth * 0.4f, -footHeight, -footLength * 0.15f),
                        new Vector3(-footWidth * 0.4f, -footHeight, -footLength * 0.15f),
                        t
                    );
                }
                int bottomVert = builder.AddVertex(bottomPos, new Vector2(0.5f, 0.3f));
                int nextBottomVert = builder.AddVertex(bottomPos, new Vector2(0.5f, 0.3f));

                builder.AddTriangle(topVerts[i], topVerts[next], bottomVert);
            }

            // 足首キャップ
            int ankleCenter = builder.AddVertex(new Vector3(0, 0, 0), new Vector2(0.5f, 1f));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                builder.AddTriangle(ankleCenter, ankleVerts[next], ankleVerts[i]);
            }

            return builder.ToMesh(isLeft ? "LeftFootMesh" : "RightFootMesh");
        }

        /// <summary>
        /// 腕セグメントを生成（3段階の太さ変化）
        /// </summary>
        private static void GenerateArmSegment(MeshBuilder builder, float length, float radiusTop, float radiusMid, float radiusBottom, int segments)
        {
            float halfLength = length * 0.5f;

            int[][] levelVertices = new int[4][];
            float[] yLevels = { halfLength, halfLength * 0.3f, -halfLength * 0.3f, -halfLength };
            float[] radii = { radiusTop, radiusMid, radiusMid * 0.95f, radiusBottom };

            for (int level = 0; level < 4; level++)
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
                    float v = (float)level / 3f;

                    levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, 1f - v));
                }
            }

            // 側面を接続
            for (int level = 0; level < 3; level++)
            {
                ConnectRings(builder, levelVertices[level], levelVertices[level + 1], segments);
            }

            // キャップ
            int topCenter = builder.AddVertex(new Vector3(0, halfLength, 0), new Vector2(0.5f, 1f));
            int bottomCenter = builder.AddVertex(new Vector3(0, -halfLength, 0), new Vector2(0.5f, 0f));

            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
                builder.AddTriangle(bottomCenter, levelVertices[3][seg], levelVertices[3][nextSeg]);
            }
        }

        /// <summary>
        /// 脚セグメントを生成
        /// </summary>
        private static void GenerateLegSegment(MeshBuilder builder, float length, float radiusTop, float radiusMid, float radiusBottom, int segments)
        {
            float halfLength = length * 0.5f;

            int[][] levelVertices = new int[4][];
            float[] yLevels = { halfLength, halfLength * 0.2f, -halfLength * 0.3f, -halfLength };
            float[] radii = { radiusTop, radiusMid, radiusMid * 0.9f, radiusBottom };

            for (int level = 0; level < 4; level++)
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
                    float v = (float)level / 3f;

                    levelVertices[level][seg] = builder.AddVertex(new Vector3(x, y, z), new Vector2(u, 1f - v));
                }
            }

            // 側面を接続
            for (int level = 0; level < 3; level++)
            {
                ConnectRings(builder, levelVertices[level], levelVertices[level + 1], segments);
            }

            // キャップ
            int topCenter = builder.AddVertex(new Vector3(0, halfLength, 0), new Vector2(0.5f, 1f));
            int bottomCenter = builder.AddVertex(new Vector3(0, -halfLength, 0), new Vector2(0.5f, 0f));

            for (int seg = 0; seg < segments; seg++)
            {
                int nextSeg = (seg + 1) % segments;
                builder.AddTriangle(topCenter, levelVertices[0][nextSeg], levelVertices[0][seg]);
                builder.AddTriangle(bottomCenter, levelVertices[3][seg], levelVertices[3][nextSeg]);
            }
        }

        /// <summary>
        /// 2つの頂点リングを接続
        /// </summary>
        private static void ConnectRings(MeshBuilder builder, int[] ring1, int[] ring2, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                builder.AddQuad(ring1[i], ring1[next], ring2[next], ring2[i]);
            }
        }
    }
}
