using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BurnOut.Editor
{
    public static class BurnOutAssetImporter
    {
        private const string SourceDirectory = "D:/code/nlinh/ano";
        private static readonly Dictionary<string, string> Imports = new()
        {
            ["BG_Far_BurnoutRealm.png"] = "Art/Backgrounds/BG_Far_BurnoutRealm.png",
            ["BG_Mid_Ruins.png"] = "Art/Backgrounds/BG_Mid_Ruins.png",
            ["ENV_Platform_Tiles.png"] = "Art/Environment/Platforms/ENV_Platform_Tiles.png",
            ["ENV_Interactables.png"] = "Art/Environment/Interactables/ENV_Interactables.png",
            ["move.png"] = "Art/Characters/Player/Player_Move.png",
            ["jump.png"] = "Art/Characters/Player/Player_Jump.png",
            ["die.png"] = "Art/Characters/Player/Player_Death.png",
            ["low sanity.png"] = "Art/Characters/Player/Player_LowSanity.png",
            ["skill 1.png"] = "Art/Characters/Player/Player_Skill01.png",
            ["skill 2.png"] = "Art/Characters/Player/Player_Skill02.png",
            ["skill 3.png"] = "Art/Characters/Player/Player_Skill03.png",
            ["quai vat di chuyen.png"] = "Art/Characters/Enemies/Enemy_Move.png",
            ["quai vật tấn công.png"] = "Art/Characters/Enemies/Enemy_Attack.png",
            ["quai vat chet.png"] = "Art/Characters/Enemies/Enemy_Death.png",
            ["chìa.png"] = "Art/Items/ITEM_Key.png",
            ["item sanity.png"] = "Art/Items/ITEM_SanityOrb.png",
            ["giấy.png"] = "Art/Items/ITEM_LoreNote.png",
            ["đá.png"] = "Art/Environment/Props/ENV_Rock.png",
            ["bậc.png"] = "Art/Environment/Platforms/ENV_Steps.png",
            ["thanh hp.png"] = "Art/UI/UI_PlayerHUD.png",
            ["main menu.png"] = "Art/UI/UI_MainMenu.png"
        };

        [MenuItem("BurnOut/02 Import And Organize Assets")]
        public static void ImportAndOrganizeAssets()
        {
            if (!Directory.Exists(SourceDirectory)) { Debug.LogError($"[BurnOut] Source art directory is missing: {SourceDirectory}"); return; }
            foreach (var pair in Imports)
            {
                var source = Path.Combine(SourceDirectory, pair.Key);
                var destination = Path.Combine(Application.dataPath, "_Project", pair.Value);
                if (!File.Exists(source)) { Debug.LogWarning($"[BurnOut] Expected source art not found: {pair.Key}"); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                if (!File.Exists(destination)) File.Copy(source, destination, false);
            }
            AssetDatabase.Refresh();
            foreach (var relative in Imports.Values) ApplySettings("Assets/_Project/" + relative);
            AssetDatabase.SaveAssets();
        }

        private static void ApplySettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = assetPath.Contains("Backgrounds") ? SpriteImportMode.Single : SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = !assetPath.Contains("Backgrounds");
            importer.textureCompression = assetPath.Contains("Backgrounds") ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
