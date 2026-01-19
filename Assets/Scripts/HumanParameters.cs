using UnityEngine;
using System;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ人間モデルのパラメータを定義
    /// </summary>
    [Serializable]
    public class HumanParameters
    {
        [Header("Body Proportions")]
        [Range(0.8f, 1.2f)] public float height = 1.0f;
        [Range(0.7f, 1.3f)] public float shoulderWidth = 1.0f;
        [Range(0.7f, 1.3f)] public float hipWidth = 1.0f;
        [Range(0.8f, 1.2f)] public float armLength = 1.0f;
        [Range(0.8f, 1.2f)] public float legLength = 1.0f;
        [Range(0.7f, 1.3f)] public float torsoLength = 1.0f;

        [Header("Head Parameters")]
        [Range(0.8f, 1.2f)] public float headWidth = 1.0f;
        [Range(0.8f, 1.2f)] public float headHeight = 1.0f;
        [Range(0.8f, 1.2f)] public float headDepth = 1.0f;
        [Range(0.0f, 1.0f)] public float jawWidth = 0.5f;
        [Range(0.0f, 1.0f)] public float chinPointiness = 0.5f;

        [Header("Face Parameters")]
        [Range(0.5f, 1.5f)] public float noseLength = 1.0f;
        [Range(0.5f, 1.5f)] public float noseWidth = 1.0f;
        [Range(0.0f, 1.0f)] public float noseHeight = 0.5f;
        [Range(-0.3f, 0.3f)] public float faceProfileAngle = 0.0f;

        [Header("Hair Parameters")]
        public HairStyle hairStyle = HairStyle.Short;
        [Range(0.8f, 1.5f)] public float hairVolume = 1.0f;
        [Range(0.0f, 1.0f)] public float hairLength = 0.5f;
        public Color hairColor = Color.black;

        [Header("Skin")]
        public Color skinColor = new Color(0.87f, 0.72f, 0.60f);

        [Header("Clothing")]
        public ClothingStyle upperClothing = ClothingStyle.TShirt;
        public ClothingStyle lowerClothing = ClothingStyle.Pants;
        public Color upperClothingColor = Color.white;
        public Color lowerClothingColor = new Color(0.2f, 0.2f, 0.4f);

        [Header("Random Seed")]
        public int seed = 0;

        public static HumanParameters CreateRandom(int seed = -1)
        {
            if (seed < 0)
                seed = UnityEngine.Random.Range(0, int.MaxValue);

            UnityEngine.Random.InitState(seed);
            var param = new HumanParameters();
            param.seed = seed;

            // Body
            param.height = UnityEngine.Random.Range(0.85f, 1.15f);
            param.shoulderWidth = UnityEngine.Random.Range(0.8f, 1.2f);
            param.hipWidth = UnityEngine.Random.Range(0.8f, 1.2f);
            param.armLength = UnityEngine.Random.Range(0.9f, 1.1f);
            param.legLength = UnityEngine.Random.Range(0.9f, 1.1f);
            param.torsoLength = UnityEngine.Random.Range(0.9f, 1.1f);

            // Head
            param.headWidth = UnityEngine.Random.Range(0.85f, 1.15f);
            param.headHeight = UnityEngine.Random.Range(0.85f, 1.15f);
            param.headDepth = UnityEngine.Random.Range(0.85f, 1.15f);
            param.jawWidth = UnityEngine.Random.Range(0.3f, 0.7f);
            param.chinPointiness = UnityEngine.Random.Range(0.2f, 0.8f);

            // Face
            param.noseLength = UnityEngine.Random.Range(0.7f, 1.3f);
            param.noseWidth = UnityEngine.Random.Range(0.7f, 1.3f);
            param.noseHeight = UnityEngine.Random.Range(0.3f, 0.7f);
            param.faceProfileAngle = UnityEngine.Random.Range(-0.15f, 0.15f);

            // Hair
            param.hairStyle = (HairStyle)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HairStyle)).Length);
            param.hairVolume = UnityEngine.Random.Range(0.9f, 1.3f);
            param.hairLength = UnityEngine.Random.Range(0.2f, 0.9f);
            param.hairColor = GetRandomHairColor();

            // Skin
            param.skinColor = GetRandomSkinColor();

            // Clothing
            param.upperClothing = (ClothingStyle)UnityEngine.Random.Range(0, 3);
            param.lowerClothing = (ClothingStyle)UnityEngine.Random.Range(3, 5);
            param.upperClothingColor = GetRandomClothingColor();
            param.lowerClothingColor = GetRandomClothingColor();

            return param;
        }

        private static Color GetRandomHairColor()
        {
            float r = UnityEngine.Random.value;
            if (r < 0.3f) return new Color(0.1f, 0.08f, 0.06f); // Black
            if (r < 0.5f) return new Color(0.35f, 0.22f, 0.12f); // Brown
            if (r < 0.65f) return new Color(0.55f, 0.35f, 0.2f); // Light Brown
            if (r < 0.75f) return new Color(0.85f, 0.7f, 0.4f); // Blonde
            if (r < 0.85f) return new Color(0.6f, 0.3f, 0.15f); // Auburn
            return new Color(0.5f, 0.5f, 0.5f); // Gray
        }

        private static Color GetRandomSkinColor()
        {
            float r = UnityEngine.Random.value;
            if (r < 0.25f) return new Color(0.96f, 0.87f, 0.78f); // Light
            if (r < 0.5f) return new Color(0.87f, 0.72f, 0.60f); // Medium
            if (r < 0.75f) return new Color(0.72f, 0.55f, 0.42f); // Tan
            return new Color(0.45f, 0.32f, 0.22f); // Dark
        }

        private static Color GetRandomClothingColor()
        {
            Color[] colors = new Color[]
            {
                Color.white,
                Color.black,
                new Color(0.2f, 0.2f, 0.4f), // Navy
                new Color(0.6f, 0.1f, 0.1f), // Dark Red
                new Color(0.1f, 0.4f, 0.2f), // Forest Green
                new Color(0.3f, 0.3f, 0.3f), // Gray
                new Color(0.9f, 0.85f, 0.7f), // Beige
                new Color(0.4f, 0.25f, 0.15f), // Brown
            };
            return colors[UnityEngine.Random.Range(0, colors.Length)];
        }
    }

    public enum HairStyle
    {
        Bald,
        Short,
        Medium,
        Long,
        Spiky,
        Slicked
    }

    public enum ClothingStyle
    {
        TShirt,
        LongSleeve,
        Jacket,
        Pants,
        Shorts
    }
}
