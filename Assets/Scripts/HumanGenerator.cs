using UnityEngine;

namespace MankindGen
{
    /// <summary>
    /// PS1風ローポリ人間モデル生成メインクラス
    /// </summary>
    public class HumanGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        public HumanParameters parameters = new HumanParameters();
        public bool generateOnStart = false;
        public bool useRandomSeed = true;

        [Header("Generated Model")]
        [SerializeField] private GameObject generatedModel;

        private MaterialManager materialManager;

        // 基本サイズ定数（メートル単位）
        private const float BASE_TORSO_HEIGHT = 0.55f;
        private const float BASE_HEAD_HEIGHT = 0.22f;
        private const float BASE_ARM_LENGTH = 0.55f;
        private const float BASE_LEG_LENGTH = 0.85f;

        private void Start()
        {
            if (generateOnStart)
            {
                if (useRandomSeed)
                    parameters = HumanParameters.CreateRandom();
                Generate();
            }
        }

        /// <summary>
        /// 現在のパラメータでモデルを生成
        /// </summary>
        [ContextMenu("Generate Human")]
        public void Generate()
        {
            ClearGenerated();

            materialManager = new MaterialManager();
            materialManager.GenerateMaterials(parameters);

            generatedModel = new GameObject("GeneratedHuman_" + parameters.seed);
            generatedModel.transform.SetParent(transform);
            generatedModel.transform.localPosition = Vector3.zero;
            generatedModel.transform.localRotation = Quaternion.identity;

            // サイズ計算
            float heightScale = parameters.height;
            float torsoHeight = BASE_TORSO_HEIGHT * parameters.torsoLength * heightScale;
            float upperTorsoHeight = torsoHeight * 0.55f;
            float lowerTorsoHeight = torsoHeight * 0.35f;
            float headHeight = BASE_HEAD_HEIGHT * parameters.headHeight;
            float neckHeight = 0.06f * heightScale;

            float upperArmLength = BASE_ARM_LENGTH * 0.45f * parameters.armLength * heightScale;
            float forearmLength = BASE_ARM_LENGTH * 0.45f * parameters.armLength * heightScale;
            float handLength = 0.08f * heightScale;

            float thighLength = BASE_LEG_LENGTH * 0.45f * parameters.legLength * heightScale;
            float calfLength = BASE_LEG_LENGTH * 0.45f * parameters.legLength * heightScale;
            float footHeight = 0.04f * heightScale;

            // 下から上へ積み上げて位置を計算
            float groundY = 0f;

            // 足 → ふくらはぎ → 太もも → 下半身 → 上半身 → 首 → 頭
            float footY = groundY + footHeight * 0.3f;
            float calfY = groundY + footHeight + calfLength * 0.5f;
            float thighY = groundY + footHeight + calfLength + thighLength * 0.5f;
            float lowerBodyY = groundY + footHeight + calfLength + thighLength + lowerTorsoHeight * 0.5f;
            float upperBodyY = lowerBodyY + lowerTorsoHeight * 0.5f + upperTorsoHeight * 0.5f;
            float neckY = upperBodyY + upperTorsoHeight * 0.5f + neckHeight * 0.5f;
            float headY = neckY + neckHeight * 0.5f + headHeight * 0.5f;

            // 上半身
            CreateBodyPart("UpperBody", BodyMeshGenerator.GenerateUpperBody(parameters),
                materialManager.UpperClothingMaterial,
                new Vector3(0, upperBodyY, 0));

            // 下半身
            CreateBodyPart("LowerBody", BodyMeshGenerator.GenerateLowerBody(parameters),
                materialManager.LowerClothingMaterial,
                new Vector3(0, lowerBodyY, 0));

            // 首
            CreateBodyPart("Neck", BodyMeshGenerator.GenerateNeck(parameters),
                materialManager.SkinMaterial,
                new Vector3(0, neckY, 0));

            // 頭部
            CreateBodyPart("Head", HeadMeshGenerator.Generate(parameters),
                materialManager.FaceMaterial,
                new Vector3(0, headY, 0));

            // 髪
            Mesh hairMesh = HairMeshGenerator.Generate(parameters);
            if (hairMesh != null)
            {
                CreateBodyPart("Hair", hairMesh, materialManager.HairMaterial,
                    new Vector3(0, headY, 0));
            }

            // 腕（左右）
            GenerateArms(upperBodyY, upperTorsoHeight);

            // 脚（左右）
            GenerateLegs(footY, calfY, thighY, footHeight, calfLength, thighLength);
        }

        /// <summary>
        /// ランダムパラメータで生成
        /// </summary>
        [ContextMenu("Generate Random Human")]
        public void GenerateRandom()
        {
            parameters = HumanParameters.CreateRandom();
            Generate();
        }

        /// <summary>
        /// 指定シードで生成
        /// </summary>
        public void GenerateWithSeed(int seed)
        {
            parameters = HumanParameters.CreateRandom(seed);
            Generate();
        }

        /// <summary>
        /// 腕を生成
        /// </summary>
        private void GenerateArms(float upperBodyY, float upperTorsoHeight)
        {
            float heightScale = parameters.height;
            float upperArmLength = BASE_ARM_LENGTH * 0.45f * parameters.armLength * heightScale;
            float forearmLength = BASE_ARM_LENGTH * 0.45f * parameters.armLength * heightScale;
            float handLength = 0.08f * heightScale;

            // 肩の位置（上半身上部）
            float shoulderY = upperBodyY + upperTorsoHeight * 0.35f;
            float shoulderX = 0.20f * parameters.shoulderWidth;

            // 腕は肩から下へ
            float upperArmY = shoulderY - upperArmLength * 0.5f;
            float elbowY = shoulderY - upperArmLength;
            float forearmY = elbowY - forearmLength * 0.5f;
            float wristY = elbowY - forearmLength;
            float handY = wristY - handLength * 0.4f;

            // 左腕
            CreateBodyPart("LeftUpperArm", LimbMeshGenerator.GenerateUpperArm(parameters, true),
                GetArmMaterial(true),
                new Vector3(-shoulderX, upperArmY, 0));

            CreateBodyPart("LeftForearm", LimbMeshGenerator.GenerateForearm(parameters, true),
                GetArmMaterial(false),
                new Vector3(-shoulderX, forearmY, 0));

            CreateBodyPart("LeftHand", LimbMeshGenerator.GenerateHand(parameters, true),
                materialManager.SkinMaterial,
                new Vector3(-shoulderX, handY, 0));

            // 右腕
            CreateBodyPart("RightUpperArm", LimbMeshGenerator.GenerateUpperArm(parameters, false),
                GetArmMaterial(true),
                new Vector3(shoulderX, upperArmY, 0));

            CreateBodyPart("RightForearm", LimbMeshGenerator.GenerateForearm(parameters, false),
                GetArmMaterial(false),
                new Vector3(shoulderX, forearmY, 0));

            CreateBodyPart("RightHand", LimbMeshGenerator.GenerateHand(parameters, false),
                materialManager.SkinMaterial,
                new Vector3(shoulderX, handY, 0));
        }

        /// <summary>
        /// 脚を生成
        /// </summary>
        private void GenerateLegs(float footY, float calfY, float thighY,
            float footHeight, float calfLength, float thighLength)
        {
            float hipX = 0.08f * parameters.hipWidth;

            // 左脚
            CreateBodyPart("LeftThigh", LimbMeshGenerator.GenerateThigh(parameters, true),
                materialManager.LowerClothingMaterial,
                new Vector3(-hipX, thighY, 0));

            CreateBodyPart("LeftCalf", LimbMeshGenerator.GenerateCalf(parameters, true),
                GetLegMaterial(),
                new Vector3(-hipX, calfY, 0));

            CreateBodyPart("LeftFoot", LimbMeshGenerator.GenerateFoot(parameters, true),
                materialManager.ShoeMaterial,
                new Vector3(-hipX, footY, 0));

            // 右脚
            CreateBodyPart("RightThigh", LimbMeshGenerator.GenerateThigh(parameters, false),
                materialManager.LowerClothingMaterial,
                new Vector3(hipX, thighY, 0));

            CreateBodyPart("RightCalf", LimbMeshGenerator.GenerateCalf(parameters, false),
                GetLegMaterial(),
                new Vector3(hipX, calfY, 0));

            CreateBodyPart("RightFoot", LimbMeshGenerator.GenerateFoot(parameters, false),
                materialManager.ShoeMaterial,
                new Vector3(hipX, footY, 0));
        }

        /// <summary>
        /// 腕のマテリアルを取得（衣服スタイルによる）
        /// </summary>
        private Material GetArmMaterial(bool isUpperArm)
        {
            if (parameters.upperClothing == ClothingStyle.TShirt)
            {
                return isUpperArm ? materialManager.UpperClothingMaterial : materialManager.SkinMaterial;
            }
            return materialManager.UpperClothingMaterial;
        }

        /// <summary>
        /// 脚のマテリアルを取得（衣服スタイルによる）
        /// </summary>
        private Material GetLegMaterial()
        {
            if (parameters.lowerClothing == ClothingStyle.Shorts)
            {
                return materialManager.SkinMaterial;
            }
            return materialManager.LowerClothingMaterial;
        }

        /// <summary>
        /// ボディパーツを作成
        /// </summary>
        private GameObject CreateBodyPart(string name, Mesh mesh, Material material, Vector3 position)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(generatedModel.transform);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.identity;

            MeshFilter filter = part.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.material = material;

            return part;
        }

        /// <summary>
        /// 生成済みモデルをクリア
        /// </summary>
        [ContextMenu("Clear Generated")]
        public void ClearGenerated()
        {
            if (generatedModel != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedModel);
                else
                    DestroyImmediate(generatedModel);
                generatedModel = null;
            }

            materialManager?.Cleanup();
            materialManager = null;
        }

        private void OnDestroy()
        {
            ClearGenerated();
        }

        /// <summary>
        /// 生成されたモデルを取得
        /// </summary>
        public GameObject GetGeneratedModel()
        {
            return generatedModel;
        }

        /// <summary>
        /// 生成されたモデルをプレハブとして保存用にエクスポート
        /// </summary>
        public GameObject ExportAsPrefab()
        {
            if (generatedModel == null)
                return null;

            // 親から切り離してルートに配置
            GameObject exported = Instantiate(generatedModel);
            exported.name = "ExportedHuman_" + parameters.seed;
            exported.transform.SetParent(null);
            return exported;
        }
    }
}
