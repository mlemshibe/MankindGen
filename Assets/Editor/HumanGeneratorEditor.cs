using UnityEngine;
using UnityEditor;

namespace MankindGen
{
    /// <summary>
    /// HumanGenerator用カスタムエディタ
    /// </summary>
    [CustomEditor(typeof(HumanGenerator))]
    public class HumanGeneratorEditor : Editor
    {
        private HumanGenerator generator;
        private bool showBodyParams = true;
        private bool showHeadParams = true;
        private bool showFaceParams = true;
        private bool showHairParams = true;
        private bool showClothingParams = true;

        private void OnEnable()
        {
            generator = (HumanGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PS1 Low-Poly Human Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 生成設定
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generateOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRandomSeed"));

            EditorGUILayout.Space();

            // 生成ボタン
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                generator.Generate();
            }
            if (GUILayout.Button("Random Generate", GUILayout.Height(30)))
            {
                generator.GenerateRandom();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Generated"))
            {
                generator.ClearGenerated();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // シード
            SerializedProperty paramsProp = serializedObject.FindProperty("parameters");

            EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("seed"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Random Seed"))
            {
                generator.parameters.seed = Random.Range(0, int.MaxValue);
                EditorUtility.SetDirty(generator);
            }
            if (GUILayout.Button("Copy Seed"))
            {
                GUIUtility.systemCopyBuffer = generator.parameters.seed.ToString();
            }
            if (GUILayout.Button("Paste Seed"))
            {
                if (int.TryParse(GUIUtility.systemCopyBuffer, out int seed))
                {
                    generator.parameters.seed = seed;
                    EditorUtility.SetDirty(generator);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Body Parameters
            showBodyParams = EditorGUILayout.Foldout(showBodyParams, "Body Proportions", true);
            if (showBodyParams)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("height"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("shoulderWidth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("hipWidth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("armLength"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("legLength"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("torsoLength"));
                EditorGUI.indentLevel--;
            }

            // Head Parameters
            showHeadParams = EditorGUILayout.Foldout(showHeadParams, "Head Parameters", true);
            if (showHeadParams)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("headWidth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("headHeight"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("headDepth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("jawWidth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("chinPointiness"));
                EditorGUI.indentLevel--;
            }

            // Face Parameters
            showFaceParams = EditorGUILayout.Foldout(showFaceParams, "Face Parameters", true);
            if (showFaceParams)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("noseLength"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("noseWidth"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("noseHeight"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("faceProfileAngle"));
                EditorGUI.indentLevel--;
            }

            // Hair Parameters
            showHairParams = EditorGUILayout.Foldout(showHairParams, "Hair Parameters", true);
            if (showHairParams)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("hairStyle"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("hairVolume"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("hairLength"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("hairColor"));
                EditorGUI.indentLevel--;
            }

            // Skin
            EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("skinColor"));

            // Clothing Parameters
            showClothingParams = EditorGUILayout.Foldout(showClothingParams, "Clothing Parameters", true);
            if (showClothingParams)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("upperClothing"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("lowerClothing"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("upperClothingColor"));
                EditorGUILayout.PropertyField(paramsProp.FindPropertyRelative("lowerClothingColor"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // エクスポートボタン
            if (generator.GetGeneratedModel() != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

                if (GUILayout.Button("Export as Prefab"))
                {
                    ExportAsPrefab();
                }
            }
        }

        private void ExportAsPrefab()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Human Prefab",
                "Human_" + generator.parameters.seed,
                "prefab",
                "Choose location to save the human prefab"
            );

            if (!string.IsNullOrEmpty(path))
            {
                GameObject exported = generator.ExportAsPrefab();
                if (exported != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(exported, path);
                    DestroyImmediate(exported);
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Export Complete", "Human prefab saved to:\n" + path, "OK");
                }
            }
        }
    }

    /// <summary>
    /// メニューからのジェネレーター作成
    /// </summary>
    public static class HumanGeneratorMenu
    {
        [MenuItem("GameObject/MankindGen/Create Human Generator", false, 10)]
        public static void CreateGenerator()
        {
            GameObject go = new GameObject("HumanGenerator");
            go.AddComponent<HumanGenerator>();
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create Human Generator");
        }

        [MenuItem("Window/MankindGen/Batch Generate Humans")]
        public static void OpenBatchGenerator()
        {
            BatchGeneratorWindow.ShowWindow();
        }
    }

    /// <summary>
    /// バッチ生成ウィンドウ
    /// </summary>
    public class BatchGeneratorWindow : EditorWindow
    {
        private int count = 10;
        private int startSeed = 0;
        private bool randomSeeds = true;
        private float spacing = 2f;
        private int columns = 5;

        public static void ShowWindow()
        {
            GetWindow<BatchGeneratorWindow>("Batch Human Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Batch Human Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            count = EditorGUILayout.IntSlider("Count", count, 1, 100);
            randomSeeds = EditorGUILayout.Toggle("Random Seeds", randomSeeds);

            if (!randomSeeds)
            {
                startSeed = EditorGUILayout.IntField("Start Seed", startSeed);
            }

            spacing = EditorGUILayout.Slider("Spacing", spacing, 0.5f, 5f);
            columns = EditorGUILayout.IntSlider("Columns", columns, 1, 20);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Batch", GUILayout.Height(30)))
            {
                GenerateBatch();
            }
        }

        private void GenerateBatch()
        {
            GameObject parent = new GameObject("GeneratedHumans");

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                GameObject go = new GameObject("HumanGenerator_" + i);
                go.transform.SetParent(parent.transform);
                go.transform.position = new Vector3(col * spacing, 0, row * spacing);

                HumanGenerator gen = go.AddComponent<HumanGenerator>();

                int seed = randomSeeds ? Random.Range(0, int.MaxValue) : startSeed + i;
                gen.GenerateWithSeed(seed);
            }

            Selection.activeGameObject = parent;
            Undo.RegisterCreatedObjectUndo(parent, "Batch Generate Humans");
        }
    }
}
