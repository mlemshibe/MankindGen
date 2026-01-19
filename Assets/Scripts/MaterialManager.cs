using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風マテリアル管理
    /// </summary>
    public class MaterialManager
    {
        public Material FaceMaterial { get; private set; }
        public Material SkinMaterial { get; private set; }
        public Material HairMaterial { get; private set; }
        public Material UpperClothingMaterial { get; private set; }
        public Material LowerClothingMaterial { get; private set; }
        public Material ShoeMaterial { get; private set; }

        private Texture2D faceTexture;
        private Texture2D skinTexture;
        private Texture2D hairTexture;
        private Texture2D upperClothingTexture;
        private Texture2D lowerClothingTexture;
        private Texture2D shoeTexture;

        /// <summary>
        /// パラメータからマテリアルを生成
        /// </summary>
        public void GenerateMaterials(HumanParameters param, Shader shader = null)
        {
            // デフォルトはUnlit/Texture（PS1風のフラットシェーディング）
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");

            // テクスチャ生成
            faceTexture = TextureGenerator.GenerateFaceTexture(param);
            skinTexture = TextureGenerator.GenerateSkinTexture(param);
            hairTexture = TextureGenerator.GenerateHairTexture(param);
            upperClothingTexture = TextureGenerator.GenerateUpperClothingTexture(param);
            lowerClothingTexture = TextureGenerator.GenerateLowerClothingTexture(param);
            shoeTexture = TextureGenerator.GenerateShoeTexture(new Color(0.2f, 0.15f, 0.1f));

            // マテリアル作成
            FaceMaterial = CreateMaterial("FaceMaterial", faceTexture, shader);
            SkinMaterial = CreateMaterial("SkinMaterial", skinTexture, shader);
            HairMaterial = CreateMaterial("HairMaterial", hairTexture, shader);
            UpperClothingMaterial = CreateMaterial("UpperClothingMaterial", upperClothingTexture, shader);
            LowerClothingMaterial = CreateMaterial("LowerClothingMaterial", lowerClothingTexture, shader);
            ShoeMaterial = CreateMaterial("ShoeMaterial", shoeTexture, shader);
        }

        /// <summary>
        /// ランタイムで色のみ更新
        /// </summary>
        public void UpdateColors(HumanParameters param)
        {
            // テクスチャを再生成
            if (faceTexture != null)
            {
                Object.Destroy(faceTexture);
                faceTexture = TextureGenerator.GenerateFaceTexture(param);
                FaceMaterial.mainTexture = faceTexture;
            }

            if (skinTexture != null)
            {
                Object.Destroy(skinTexture);
                skinTexture = TextureGenerator.GenerateSkinTexture(param);
                SkinMaterial.mainTexture = skinTexture;
            }

            if (hairTexture != null)
            {
                Object.Destroy(hairTexture);
                hairTexture = TextureGenerator.GenerateHairTexture(param);
                HairMaterial.mainTexture = hairTexture;
            }

            if (upperClothingTexture != null)
            {
                Object.Destroy(upperClothingTexture);
                upperClothingTexture = TextureGenerator.GenerateUpperClothingTexture(param);
                UpperClothingMaterial.mainTexture = upperClothingTexture;
            }

            if (lowerClothingTexture != null)
            {
                Object.Destroy(lowerClothingTexture);
                lowerClothingTexture = TextureGenerator.GenerateLowerClothingTexture(param);
                LowerClothingMaterial.mainTexture = lowerClothingTexture;
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Cleanup()
        {
            DestroyIfNotNull(faceTexture);
            DestroyIfNotNull(skinTexture);
            DestroyIfNotNull(hairTexture);
            DestroyIfNotNull(upperClothingTexture);
            DestroyIfNotNull(lowerClothingTexture);
            DestroyIfNotNull(shoeTexture);

            DestroyIfNotNull(FaceMaterial);
            DestroyIfNotNull(SkinMaterial);
            DestroyIfNotNull(HairMaterial);
            DestroyIfNotNull(UpperClothingMaterial);
            DestroyIfNotNull(LowerClothingMaterial);
            DestroyIfNotNull(ShoeMaterial);
        }

        private Material CreateMaterial(string name, Texture2D texture, Shader shader)
        {
            Material mat = new Material(shader);
            mat.name = name;
            mat.mainTexture = texture;
            return mat;
        }

        private void DestroyIfNotNull(Object obj)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj);
            }
        }
    }
}
