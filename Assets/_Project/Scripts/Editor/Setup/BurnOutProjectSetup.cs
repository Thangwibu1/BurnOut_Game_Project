using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BurnOut.Editor
{
    public static class BurnOutProjectSetup
    {
        private const string Root = "Assets/_Project";
        private static readonly string[] Folders =
        {
            "Art/Backgrounds", "Art/Characters/Player", "Art/Characters/Enemies", "Art/Environment/Platforms", "Art/Environment/Props", "Art/Environment/Interactables", "Art/Effects", "Art/Items", "Art/UI",
            "Animations/Player", "Animations/Enemies", "Animations/Environment", "Animations/UI", "Audio/Music", "Audio/SFX", "Input", "Materials", "Prefabs/Player", "Prefabs/Enemies", "Prefabs/Environment", "Prefabs/Items", "Prefabs/Systems", "Prefabs/UI", "Scenes", "Scripts/Runtime", "Scripts/Editor", "Settings", "ScriptableObjects", "Tests/EditMode", "Tests/PlayMode", "Documentation"
        };

        [MenuItem("BurnOut/01 Setup Folders")]
        public static void SetupFolders()
        {
            foreach (var folder in Folders) Directory.CreateDirectory(Path.Combine(Application.dataPath, "_Project", folder));
            AssetDatabase.Refresh();
            Debug.Log("[BurnOut] Required project folders are ready.");
        }

        [MenuItem("BurnOut/03 Configure Tags And Layers")]
        public static void ConfigureTagsAndLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tagManager.FindProperty("tags");
            foreach (var tag in new[] { "Player", "Enemy", "Checkpoint", "LevelExit", "SanityItem", "Key", "MentalFragment" }) AddUnique(tags, tag);
            var layers = tagManager.FindProperty("layers");
            var layerNames = new[] { "Player", "Enemy", "Ground", "PlayerAttack", "EnemyAttack", "Item", "Interactable", "Hazard" };
            for (var i = 0; i < layerNames.Length; i++)
            {
                var layer = layers.GetArrayElementAtIndex(8 + i);
                if (string.IsNullOrWhiteSpace(layer.stringValue) || layer.stringValue == layerNames[i]) layer.stringValue = layerNames[i];
                else Debug.LogWarning($"[BurnOut] Layer slot {8 + i} is already '{layer.stringValue}'; '{layerNames[i]}' must be configured manually.");
            }
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("BurnOut/04 Configure Input")]
        public static void ConfigureInput()
        {
            var inputAsset = AssetDatabase.LoadAssetAtPath<Object>(Root + "/Input/BurnOutInputActions.inputactions");
            if (inputAsset == null) { Debug.LogError("[BurnOut] BurnOutInputActions.inputactions is missing."); return; }
            var inputSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            inputSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorBuildSettings.AddConfigObject("com.unity.input.settings.actions", inputAsset, true);
            Debug.Log("[BurnOut] Input Actions registered. Set Active Input Handling to 'Input System Package (New)' if Unity requests a restart.");
        }

        [MenuItem("BurnOut/Run Full Setup")]
        public static void RunFullSetup()
        {
            SetupFolders();
            BurnOutAssetImporter.ImportAndOrganizeAssets();
            ConfigureTagsAndLayers();
            ConfigureInput();
            BurnOutPrefabBuilder.RebuildPrototypePrefabs();
            BurnOutSceneBuilder.BuildScenes();
            BurnOutValidationWindow.ValidateProject();
            AssetDatabase.SaveAssets();
            Debug.Log("[BurnOut] Full setup completed. Review the validation summary above.");
        }

        private static void AddUnique(SerializedProperty values, string value)
        {
            for (var i = 0; i < values.arraySize; i++) if (values.GetArrayElementAtIndex(i).stringValue == value) return;
            values.InsertArrayElementAtIndex(values.arraySize);
            values.GetArrayElementAtIndex(values.arraySize - 1).stringValue = value;
        }
    }
}
