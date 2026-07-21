using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BurnOut.Editor
{
    public sealed class BurnOutValidationWindow : EditorWindow
    {
        private Vector2 scroll;
        private static readonly List<string> Results = new();

        [MenuItem("BurnOut/07 Validate Project")]
        public static void OpenAndValidate()
        {
            ValidateProject();
            GetWindow<BurnOutValidationWindow>("BurnOut Validation");
        }

        public static void ValidateProject()
        {
            Results.Clear();
            Check(File.Exists(Application.dataPath + "/_Project/Input/BurnOutInputActions.inputactions"), "Input Actions asset exists.");
            Check(File.Exists(Application.dataPath + "/_Project/Scenes/SC_MainMenu.unity"), "Main Menu scene exists.");
            Check(File.Exists(Application.dataPath + "/_Project/Scenes/SC_Level01.unity"), "Level 01 scene exists.");
            Check(EditorBuildSettings.scenes.Length >= 2, "Build settings contain both game scenes.");
            foreach (var path in new[] { "Player/PF_Player", "Enemies/PF_Enemy_Shadow", "Enemies/PF_MiniBoss_Shadow", "Items/PF_SanityOrb", "Items/PF_Key", "Environment/PF_Checkpoint", "Environment/PF_LockedDoor" })
                Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/" + path + ".prefab") != null, "Prefab exists: " + path);
            Debug.Log("[BurnOut] Validation summary\n" + string.Join("\n", Results));
        }

        private static void Check(bool condition, string label) => Results.Add((condition ? "PASS: " : "CHECK: ") + label);
        private void OnGUI() { scroll = EditorGUILayout.BeginScrollView(scroll); foreach (var result in Results) EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel); EditorGUILayout.EndScrollView(); }
    }
}
