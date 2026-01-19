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

        // 各パーツの位置オフセット（メートル）
        private const float HEAD_Y_OFFSET = 1.55f;
        private const float NECK_Y_OFFSET = 1.45f;
        private const float UPPER_BODY_Y_OFFSET = 1.15f;
        private const float LOWER_BODY_Y_OFFSET = 0.85f;
        private const float SHOULDER_X_OFFSET = 0.18f;
        private const float HIP_X_OFFSET = 0.09f;
        private const float UPPER_ARM_Y_OFFSET = 1.35f;
        private const float ELBOW_Y_OFFSET = 1.10f;
        private const float WRIST_Y_OFFSET = 0.85f;
        private const float THIGH_Y_OFFSET = 0.65f;
        private const float KNEE_Y_OFFSET = 0.40f;
        private const float ANKLE_Y_OFFSET = 0.08f;

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

            float heightScale = parameters.height;

            // 頭部
            CreateBodyPart("Head", HeadMeshGenerator.Generate(parameters), materialManager.FaceMaterial,
                new Vector3(0, HEAD_Y_OFFSET * heightScale, 0));

            // 髪
            Mesh hairMesh = HairMeshGenerator.Generate(parameters);
            if (hairMesh != null)
            {
                CreateBodyPart("Hair", hairMesh, materialManager.HairMaterial,
                    new Vector3(0, HEAD_Y_OFFSET * heightScale, 0));
            }

            // 首
            CreateBodyPart("Neck", BodyMeshGenerator.GenerateNeck(parameters), materialManager.SkinMaterial,
                new Vector3(0, NECK_Y_OFFSET * heightScale, 0));

            // 上半身
            CreateBodyPart("UpperBody", BodyMeshGenerator.GenerateUpperBody(parameters), materialManager.UpperClothingMaterial,
                new Vector3(0, UPPER_BODY_Y_OFFSET * heightScale, 0));

            // 下半身
            CreateBodyPart("LowerBody", BodyMeshGenerator.GenerateLowerBody(parameters), materialManager.LowerClothingMaterial,
                new Vector3(0, LOWER_BODY_Y_OFFSET * heightScale, 0));

            // 腕（左右）
            GenerateArms(heightScale);

            // 脚（左右）
            GenerateLegs(heightScale);
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
        private void GenerateArms(float heightScale)
        {
            float shoulderX = SHOULDER_X_OFFSET * parameters.shoulderWidth;

            // 左腕
            CreateBodyPart("LeftUpperArm", LimbMeshGenerator.GenerateUpperArm(parameters, true),
                GetArmMaterial(true),
                new Vector3(-shoulderX, UPPER_ARM_Y_OFFSET * heightScale, 0));

            CreateBodyPart("LeftForearm", LimbMeshGenerator.GenerateForearm(parameters, true),
                GetArmMaterial(false),
                new Vector3(-shoulderX, ELBOW_Y_OFFSET * heightScale, 0));

            CreateBodyPart("LeftHand", LimbMeshGenerator.GenerateHand(parameters, true),
                materialManager.SkinMaterial,
                new Vector3(-shoulderX, WRIST_Y_OFFSET * heightScale, 0));

            // 右腕
            CreateBodyPart("RightUpperArm", LimbMeshGenerator.GenerateUpperArm(parameters, false),
                GetArmMaterial(true),
                new Vector3(shoulderX, UPPER_ARM_Y_OFFSET * heightScale, 0));

            CreateBodyPart("RightForearm", LimbMeshGenerator.GenerateForearm(parameters, false),
                GetArmMaterial(false),
                new Vector3(shoulderX, ELBOW_Y_OFFSET * heightScale, 0));

            CreateBodyPart("RightHand", LimbMeshGenerator.GenerateHand(parameters, false),
                materialManager.SkinMaterial,
                new Vector3(shoulderX, WRIST_Y_OFFSET * heightScale, 0));
        }

        /// <summary>
        /// 脚を生成
        /// </summary>
        private void GenerateLegs(float heightScale)
        {
            float hipX = HIP_X_OFFSET * parameters.hipWidth;

            // 左脚
            CreateBodyPart("LeftThigh", LimbMeshGenerator.GenerateThigh(parameters, true),
                materialManager.LowerClothingMaterial,
                new Vector3(-hipX, THIGH_Y_OFFSET * heightScale, 0));

            CreateBodyPart("LeftCalf", LimbMeshGenerator.GenerateCalf(parameters, true),
                GetLegMaterial(),
                new Vector3(-hipX, KNEE_Y_OFFSET * heightScale, 0));

            CreateBodyPart("LeftFoot", LimbMeshGenerator.GenerateFoot(parameters, true),
                materialManager.ShoeMaterial,
                new Vector3(-hipX, ANKLE_Y_OFFSET * heightScale, 0));

            // 右脚
            CreateBodyPart("RightThigh", LimbMeshGenerator.GenerateThigh(parameters, false),
                materialManager.LowerClothingMaterial,
                new Vector3(hipX, THIGH_Y_OFFSET * heightScale, 0));

            CreateBodyPart("RightCalf", LimbMeshGenerator.GenerateCalf(parameters, false),
                GetLegMaterial(),
                new Vector3(hipX, KNEE_Y_OFFSET * heightScale, 0));

            CreateBodyPart("RightFoot", LimbMeshGenerator.GenerateFoot(parameters, false),
                materialManager.ShoeMaterial,
                new Vector3(hipX, ANKLE_Y_OFFSET * heightScale, 0));
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
