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

        // Wide multi-storey scene backdrops (separate source folder the user connected).
        private const string BackgroundSourceDirectory = "D:/code/nlinh/background_new";
        private static readonly Dictionary<string, string> BackgroundImports = new()
        {
            ["bg1.png"] = "Art/Backgrounds/BG_Scene1.png",
            ["bg2.png"] = "Art/Backgrounds/BG_Scene2.png",
            ["bg3.png"] = "Art/Backgrounds/BG_Scene3.png",
            ["bg4.png"] = "Art/Backgrounds/BG_Scene4.png",
            // New single backdrop used for all zones.
            ["bg.jpg"] = "Art/Backgrounds/BG_Main.jpg"
        };

        // Extra art placed directly into the project (already copied by setup).
        private static readonly Dictionary<string, string> ExtraAssets = new()
        {
            // Enemy energy-ball projectile (white-matte source; knockOut handled in sprite factory).
            ["element/fire.png"] = "Art/Enemies/Enemy_Projectile_Fire.png"
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
            ImportFolder(BackgroundSourceDirectory, BackgroundImports);
            // Extra pre-placed assets (already in Assets folder; just ensure import settings are correct).
            foreach (var v in ExtraAssets.Values) ApplySettings("Assets/_Project/" + v);
            AssetDatabase.Refresh();
            foreach (var relative in Imports.Values) ApplySettings("Assets/_Project/" + relative);
            foreach (var relative in BackgroundImports.Values) ApplySettings("Assets/_Project/" + relative);
            AssetDatabase.SaveAssets();
        }

        private static void ImportFolder(string sourceDirectory, Dictionary<string, string> imports)
        {
            if (!Directory.Exists(sourceDirectory)) { Debug.LogWarning($"[BurnOut] Optional source folder not found (skipping): {sourceDirectory}"); return; }
            foreach (var pair in imports)
            {
                var source = Path.Combine(sourceDirectory, pair.Key);
                var destination = Path.Combine(Application.dataPath, "_Project", pair.Value);
                if (!File.Exists(source)) { Debug.LogWarning($"[BurnOut] Expected source art not found: {pair.Key}"); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                if (!File.Exists(destination)) File.Copy(source, destination, false);
            }
        }

        private static void ApplySettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;
            bool isBackground = assetPath.Contains("Backgrounds");
            bool isJpg = assetPath.EndsWith(".jpg") || assetPath.EndsWith(".jpeg");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = isBackground ? SpriteImportMode.Single : SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = !isBackground && !isJpg;
            importer.textureCompression = isBackground ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
