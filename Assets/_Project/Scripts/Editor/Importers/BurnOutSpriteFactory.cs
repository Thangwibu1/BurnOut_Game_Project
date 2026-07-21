using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BurnOut.Editor
{
    public static class BurnOutSpriteFactory
    {
        private const string SourcePath = "Assets/_Project/Art/Characters/Player/Player_Move.png";
        private const string OutputPath = "Assets/_Project/Art/Characters/Player/Player_Idle_Cropped.png";
        private const string PlatformSourcePath = "Assets/_Project/Art/Environment/Platforms/ENV_Platform_Tiles.png";
        private const string PlatformOutputPath = "Assets/_Project/Art/Environment/Platforms/ENV_Platform_Main_Cropped.png";
        private const string EnemySourcePath = "Assets/_Project/Art/Characters/Enemies/Enemy_Move.png";
        private const string InteractableSourcePath = "Assets/_Project/Art/Environment/Interactables/ENV_Interactables.png";

        public static Sprite GetPlayerIdleSprite()
        {
            return GetCroppedSprite(SourcePath, OutputPath, 518, 634, 48, 84, 48f);
        }

        public static Sprite GetPlatformSprite()
        {
            // Main wide platform: x 390–1090, y 78–220 in the original 1920x1080 sheet.
            return GetCroppedSprite(PlatformSourcePath, PlatformOutputPath, 390, 860, 700, 150, 100f);
        }

        // The source art is a presentation sheet, not a Unity sprite sheet.  These
        // explicit crops preserve the artist's pixels and prevent Unity from using
        // a stretched placeholder shape for world objects.
        public static Sprite GetEnemySprite()
        {
            return GetCroppedSprite(EnemySourcePath, "Assets/_Project/Art/Characters/Enemies/Enemy_Shadow_Cropped.png", 248, 430, 145, 165, 64f);
        }

        public static Sprite GetCheckpointSprite()
        {
            return GetCroppedSprite(InteractableSourcePath, "Assets/_Project/Art/Environment/Interactables/ENV_Checkpoint_Cropped.png", 748, 325, 265, 315, 100f);
        }

        public static Sprite GetDoorSprite()
        {
            return GetCroppedSprite(InteractableSourcePath, "Assets/_Project/Art/Environment/Interactables/ENV_Door_Cropped.png", 475, 650, 265, 380, 100f);
        }

        public static Sprite GetOpenDoorSprite()
        {
            return GetCroppedSprite(InteractableSourcePath, "Assets/_Project/Art/Environment/Interactables/ENV_Door_Open_Cropped.png", 750, 650, 270, 380, 100f);
        }

        public static Sprite GetFragmentSprite()
        {
            return GetCroppedSprite(InteractableSourcePath, "Assets/_Project/Art/Environment/Interactables/ENV_Fragment_Cropped.png", 1125, 335, 205, 305, 100f);
        }

        public static Sprite GetHazardSprite()
        {
            return GetCroppedSprite(PlatformSourcePath, "Assets/_Project/Art/Environment/Platforms/ENV_Hazard_Cropped.png", 1110, 70, 310, 120, 100f);
        }

        public static Sprite GetStepIslandSprite()
        {
            return GetCroppedSprite("Assets/_Project/Art/Environment/Platforms/ENV_Steps.png", "Assets/_Project/Art/Environment/Platforms/ENV_StepIsland_Cropped.png", 370, 600, 460, 330, 100f);
        }

        public static Sprite GetTrimmedSprite(string sourcePath, string outputPath, float pixelsPerUnit)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
            if (existing != null) return existing;

            var importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            if (importer == null) return null;
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null) return null;

            var pixels = source.GetPixels32();
            var minX = source.width; var minY = source.height; var maxX = -1; var maxY = -1;
            for (var y = 0; y < source.height; y++)
            for (var x = 0; x < source.width; x++)
            {
                if (pixels[y * source.width + x].a < 12) continue;
                minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
            }
            if (maxX < minX || maxY < minY) return null;
            const int padding = 4;
            minX = Mathf.Max(0, minX - padding); minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(source.width - 1, maxX + padding); maxY = Mathf.Min(source.height - 1, maxY + padding);
            return GetCroppedSprite(sourcePath, outputPath, minX, minY, maxX - minX + 1, maxY - minY + 1, pixelsPerUnit);
        }

        /// <summary>Extracts the separated frames in an artist supplied transparent animation strip.</summary>
        public static Sprite[] GetAnimationFrames(string sourcePath, string outputFolder, float pixelsPerUnit)
        {
            var importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            if (importer == null) return System.Array.Empty<Sprite>();
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null) return System.Array.Empty<Sprite>();

            var pixels = source.GetPixels32();
            var occupiedColumns = new bool[source.width];
            for (var x = 0; x < source.width; x++)
                for (var y = 0; y < source.height; y++)
                    if (pixels[y * source.width + x].a >= 12) { occupiedColumns[x] = true; break; }

            const int splitGap = 12;
            var segments = new List<(int start, int end)>();
            var start = -1; var end = -1; var gap = 0;
            for (var x = 0; x < occupiedColumns.Length; x++)
            {
                if (occupiedColumns[x])
                {
                    if (start < 0) start = x;
                    end = x; gap = 0;
                }
                else if (start >= 0 && ++gap >= splitGap)
                {
                    segments.Add((start, end)); start = -1; end = -1; gap = 0;
                }
            }
            if (start >= 0) segments.Add((start, end));
            if (segments.Count == 0) return System.Array.Empty<Sprite>();

            Directory.CreateDirectory(Path.Combine(Application.dataPath, outputFolder.Substring("Assets/".Length)));
            var frames = new List<Sprite>();
            for (var index = 0; index < segments.Count; index++)
            {
                var segment = segments[index];
                var minY = source.height; var maxY = -1;
                for (var x = segment.start; x <= segment.end; x++)
                for (var y = 0; y < source.height; y++)
                    if (pixels[y * source.width + x].a >= 12) { minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                if (maxY < minY) continue;
                const int padding = 4;
                var left = Mathf.Max(0, segment.start - padding);
                var bottom = Mathf.Max(0, minY - padding);
                var right = Mathf.Min(source.width - 1, segment.end + padding);
                var top = Mathf.Min(source.height - 1, maxY + padding);
                var outputPath = $"{outputFolder}/Frame_{index:00}.png";
                var frame = GetCroppedSprite(sourcePath, outputPath, left, bottom, right - left + 1, top - bottom + 1, pixelsPerUnit);
                if (frame != null) frames.Add(frame);
            }
            return frames.ToArray();
        }

        private static Sprite GetCroppedSprite(string sourcePath, string outputPath, int x, int y, int width, int height, float pixelsPerUnit)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
            if (existing != null) return existing;

            var importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            if (importer == null) return null;
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();

            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null) return null;
            var cropped = new Texture2D(width, height, TextureFormat.RGBA32, false);
            cropped.SetPixels(source.GetPixels(x, y, width, height));
            cropped.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, outputPath.Substring("Assets/".Length)), cropped.EncodeToPNG());
            Object.DestroyImmediate(cropped);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);

            var outputImporter = (TextureImporter)AssetImporter.GetAtPath(outputPath);
            outputImporter.textureType = TextureImporterType.Sprite;
            outputImporter.spriteImportMode = SpriteImportMode.Single;
            outputImporter.spritePixelsPerUnit = pixelsPerUnit;
            outputImporter.alphaIsTransparency = true;
            outputImporter.filterMode = FilterMode.Point;
            outputImporter.textureCompression = TextureImporterCompression.Uncompressed;
            outputImporter.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
        }
    }
}
