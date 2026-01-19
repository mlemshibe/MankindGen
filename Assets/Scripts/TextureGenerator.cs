using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風テクスチャ生成（低解像度、シンプルなパターン）
    /// </summary>
    public static class TextureGenerator
    {
        // PS1風の低解像度
        private const int FACE_TEXTURE_SIZE = 64;
        private const int SKIN_TEXTURE_SIZE = 32;
        private const int CLOTHING_TEXTURE_SIZE = 64;

        /// <summary>
        /// 顔テクスチャを生成（目・鼻・口の位置は固定）
        /// </summary>
        public static Texture2D GenerateFaceTexture(HumanParameters param)
        {
            Texture2D tex = new Texture2D(FACE_TEXTURE_SIZE, FACE_TEXTURE_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // PS1風のピクセレート
            tex.wrapMode = TextureWrapMode.Clamp;

            Color skinColor = param.skinColor;
            Color darkSkin = skinColor * 0.85f;
            darkSkin.a = 1f;

            // 肌色で塗りつぶし
            Color[] pixels = new Color[FACE_TEXTURE_SIZE * FACE_TEXTURE_SIZE];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = skinColor;

            // 目の位置（テクスチャ上で固定）
            int eyeY = (int)(FACE_TEXTURE_SIZE * 0.6f);
            int leftEyeX = (int)(FACE_TEXTURE_SIZE * 0.35f);
            int rightEyeX = (int)(FACE_TEXTURE_SIZE * 0.65f);
            int eyeSize = 4;

            // 目を描画（白目）
            DrawEye(pixels, leftEyeX, eyeY, eyeSize, Color.white, Color.black);
            DrawEye(pixels, rightEyeX, eyeY, eyeSize, Color.white, Color.black);

            // 眉毛
            int browY = eyeY + 5;
            Color browColor = param.hairColor * 0.8f;
            browColor.a = 1f;
            DrawLine(pixels, leftEyeX - 2, browY, leftEyeX + 3, browY + 1, browColor);
            DrawLine(pixels, rightEyeX - 3, browY + 1, rightEyeX + 2, browY, browColor);

            // 鼻の陰（テクスチャ上のヒント、実際の鼻はジオメトリ）
            int noseY = (int)(FACE_TEXTURE_SIZE * 0.42f);
            int noseX = FACE_TEXTURE_SIZE / 2;
            DrawLine(pixels, noseX, noseY + 3, noseX, noseY - 2, darkSkin);

            // 口
            int mouthY = (int)(FACE_TEXTURE_SIZE * 0.25f);
            int mouthX = FACE_TEXTURE_SIZE / 2;
            Color lipColor = new Color(
                Mathf.Min(1f, skinColor.r * 1.1f),
                skinColor.g * 0.85f,
                skinColor.b * 0.85f
            );
            DrawLine(pixels, mouthX - 4, mouthY, mouthX + 4, mouthY, lipColor);
            // 口の上下のライン
            pixels[mouthY * FACE_TEXTURE_SIZE + mouthX - 3] = darkSkin;
            pixels[mouthY * FACE_TEXTURE_SIZE + mouthX + 3] = darkSkin;

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 肌テクスチャを生成（均質な単一色）
        /// </summary>
        public static Texture2D GenerateSkinTexture(HumanParameters param)
        {
            Texture2D tex = new Texture2D(SKIN_TEXTURE_SIZE, SKIN_TEXTURE_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[SKIN_TEXTURE_SIZE * SKIN_TEXTURE_SIZE];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = param.skinColor;

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 髪テクスチャを生成（均質な単一色）
        /// </summary>
        public static Texture2D GenerateHairTexture(HumanParameters param)
        {
            Texture2D tex = new Texture2D(SKIN_TEXTURE_SIZE, SKIN_TEXTURE_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[SKIN_TEXTURE_SIZE * SKIN_TEXTURE_SIZE];
            Color hairColor = param.hairColor;

            for (int y = 0; y < SKIN_TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < SKIN_TEXTURE_SIZE; x++)
                {
                    // 髪に微妙なストライプ模様を追加（PS1風）
                    float stripe = ((x + y) % 4 == 0) ? 0.95f : 1f;
                    Color c = hairColor * stripe;
                    c.a = 1f;
                    pixels[y * SKIN_TEXTURE_SIZE + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 上半身衣服テクスチャを生成
        /// </summary>
        public static Texture2D GenerateUpperClothingTexture(HumanParameters param)
        {
            Texture2D tex = new Texture2D(CLOTHING_TEXTURE_SIZE, CLOTHING_TEXTURE_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[CLOTHING_TEXTURE_SIZE * CLOTHING_TEXTURE_SIZE];
            Color baseColor = param.upperClothingColor;
            Color darkColor = baseColor * 0.85f;
            darkColor.a = 1f;

            for (int y = 0; y < CLOTHING_TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < CLOTHING_TEXTURE_SIZE; x++)
                {
                    Color c = baseColor;

                    // 衣服のパターン生成（スタイルによって変更）
                    switch (param.upperClothing)
                    {
                        case ClothingStyle.TShirt:
                            // シンプルな単色
                            break;

                        case ClothingStyle.LongSleeve:
                            // 微妙なストライプ
                            if (x % 8 < 2)
                                c = darkColor;
                            break;

                        case ClothingStyle.Jacket:
                            // ボタン風のアクセント
                            int centerX = CLOTHING_TEXTURE_SIZE / 2;
                            if (Mathf.Abs(x - centerX) < 3 && y % 12 < 4 && y > 10 && y < 50)
                                c = darkColor;
                            // 襟
                            if (y > CLOTHING_TEXTURE_SIZE - 10 && Mathf.Abs(x - centerX) < 15)
                                c = darkColor;
                            break;
                    }

                    pixels[y * CLOTHING_TEXTURE_SIZE + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 下半身衣服テクスチャを生成
        /// </summary>
        public static Texture2D GenerateLowerClothingTexture(HumanParameters param)
        {
            Texture2D tex = new Texture2D(CLOTHING_TEXTURE_SIZE, CLOTHING_TEXTURE_SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[CLOTHING_TEXTURE_SIZE * CLOTHING_TEXTURE_SIZE];
            Color baseColor = param.lowerClothingColor;
            Color darkColor = baseColor * 0.9f;
            darkColor.a = 1f;

            for (int y = 0; y < CLOTHING_TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < CLOTHING_TEXTURE_SIZE; x++)
                {
                    Color c = baseColor;

                    switch (param.lowerClothing)
                    {
                        case ClothingStyle.Pants:
                            // デニム風の微妙なテクスチャ
                            if ((x + y) % 3 == 0)
                                c = Color.Lerp(baseColor, darkColor, 0.3f);
                            break;

                        case ClothingStyle.Shorts:
                            // シンプル
                            break;
                    }

                    pixels[y * CLOTHING_TEXTURE_SIZE + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 靴テクスチャを生成
        /// </summary>
        public static Texture2D GenerateShoeTexture(Color shoeColor)
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[32 * 32];
            Color darkColor = shoeColor * 0.8f;
            darkColor.a = 1f;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    // 靴底部分は暗く
                    Color c = (y < 8) ? darkColor : shoeColor;
                    pixels[y * 32 + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ヘルパー関数：目を描画
        private static void DrawEye(Color[] pixels, int centerX, int centerY, int size, Color whiteColor, Color pupilColor)
        {
            int halfSize = size / 2;

            // 白目
            for (int dy = -halfSize; dy <= halfSize; dy++)
            {
                for (int dx = -halfSize; dx <= halfSize; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    if (x >= 0 && x < FACE_TEXTURE_SIZE && y >= 0 && y < FACE_TEXTURE_SIZE)
                    {
                        pixels[y * FACE_TEXTURE_SIZE + x] = whiteColor;
                    }
                }
            }

            // 瞳
            pixels[centerY * FACE_TEXTURE_SIZE + centerX] = pupilColor;
            if (centerX + 1 < FACE_TEXTURE_SIZE)
                pixels[centerY * FACE_TEXTURE_SIZE + centerX + 1] = pupilColor;
            if (centerY - 1 >= 0)
                pixels[(centerY - 1) * FACE_TEXTURE_SIZE + centerX] = pupilColor;
            if (centerX + 1 < FACE_TEXTURE_SIZE && centerY - 1 >= 0)
                pixels[(centerY - 1) * FACE_TEXTURE_SIZE + centerX + 1] = pupilColor;
        }

        // ヘルパー関数：線を描画
        private static void DrawLine(Color[] pixels, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < FACE_TEXTURE_SIZE && y0 >= 0 && y0 < FACE_TEXTURE_SIZE)
                    pixels[y0 * FACE_TEXTURE_SIZE + x0] = color;

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}
