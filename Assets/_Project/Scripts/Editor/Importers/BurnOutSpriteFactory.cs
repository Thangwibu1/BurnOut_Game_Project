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
        private const string CheckpointPillowPath = "Assets/_Project/Art/Environment/Interactables/Checkpoint_Pillow.png";

        // Ornate gold twin-slot HUD frame (ano/thanh_mau.png). Transparent bg; content bbox 542x134 (~4:1).
        // Top slot = health bar, bottom slot = sanity bar. Used as a decorative border behind the meters.
        public static Sprite GetHealthFrameSprite()
        {
            return GetCroppedSprite("Assets/_Project/Art/UI/UI_HealthFrame.png", "Assets/_Project/Art/UI/UI_HealthFrame_Cropped.png", 715, 482, 542, 134, 100f);
        }

        public static Sprite GetPlayerIdleSprite()
        {
            return GetCroppedSprite(SourcePath, OutputPath, 518, 634, 48, 84, 48f);
        }

        // One blue orb cropped from the 3x2 element sheet — the health drop enemies leave behind.
        public static Sprite GetHealthOrbSprite()
        {
            const string src = "Assets/_Project/Art/Items/Item_HealthOrb_Sheet.png";
            const string outPath = "Assets/_Project/Art/Items/Item_HealthOrb_Cropped.png";
            // Sheet is 1920x1080; blob at PIL (x1108,y_top164,244x316). GetPixels uses bottom-left origin.
            return GetCroppedSprite(src, outPath, 1096, 588, 268, 340, 420f);
        }

        // The enemy energy ball — blue flame cropped from element/fire.png (white matte knocked out).
        public static Sprite GetFireProjectileSprite()
        {
            const string src = "Assets/_Project/Art/Enemies/Enemy_Projectile_Fire.png";
            const string outPath = "Assets/_Project/Art/Enemies/Enemy_Projectile_Fire_Cropped.png";
            // Sheet 1920x1080; flame blob at PIL(x685,y301,549x499). GetPixels uses bottom-left origin.
            return GetCroppedSprite(src, outPath, 655, 250, 609, 559, 800f, knockOutWhite: true);
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
            return GetCroppedSprite(CheckpointPillowPath, "Assets/_Project/Art/Environment/Interactables/Checkpoint_Pillow_Cropped.png", 228, 194, 594, 674, 214f);
        }

        // The dim, unlit resting shrine on the left of the interactables sheet — the checkpoint before it is touched.
        public static Sprite GetCheckpointInactiveSprite()
        {
            return GetCheckpointSprite(); // same pillow; Checkpoint component tints/swaps on activation
        }

        // A standalone rubble rock used to dress the ground so the route is not a bare slab.
        public static Sprite GetRockSprite()
        {
            return GetTrimmedSprite("Assets/_Project/Art/Environment/Props/ENV_Rock.png", "Assets/_Project/Art/Environment/Props/ENV_Rock_Cropped.png", 100f);
        }

        // The artist's lore paper — previously imported but unused. Dresses ledges as scattered notes.
        public static Sprite GetLoreNoteSprite()
        {
            return GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_LoreNote.png", "Assets/_Project/Art/Items/ITEM_LoreNote_Cropped.png", 90f);
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

        // A procedural dust shockwave: a forward-leaning crescent of soft particles, so the
        // Shockwave skill reads as a travelling ground wave rather than a solid box.
        public static Sprite GetShockwaveSprite()
        {
            const string outputPath = "Assets/_Project/Art/Effects/FX_Shockwave.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
            if (existing != null) return existing;
            const int w = 192, h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // Crescent: bright vertical front on the right, trailing dust to the left.
                float nx = x / (float)(w - 1), ny = (y / (float)(h - 1)) * 2f - 1f;
                float front = Mathf.Exp(-Mathf.Pow((nx - .82f) * 5.5f, 2f));      // leading edge
                float arc = Mathf.Exp(-Mathf.Pow(ny / (.85f - nx * .5f), 2f));    // curved body, taller at front
                float trail = Mathf.Clamp01(nx) * (1f - Mathf.Abs(ny));           // dusty tail
                float a = Mathf.Clamp01(front * 1.1f + arc * .55f) * arc + trail * .25f * arc;
                var c = Color.Lerp(new Color(1f, .82f, .5f), new Color(1f, .55f, .25f), 1f - nx);
                tex.SetPixel(x, y, new Color(c.r, c.g, c.b, Mathf.Clamp01(a)));
            }
            tex.Apply();
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "_Project/Art/Effects"));
            File.WriteAllBytes(Path.Combine(Application.dataPath, outputPath.Substring("Assets/".Length)), tex.EncodeToPNG());
            AssetDatabase.ImportAsset(outputPath);
            var importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (importer != null) { importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 100f; importer.SaveAndReimport(); }
            return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
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

        /// <summary>Frames for a multi-row monster sheet, one array per animation. Empty arrays are safe.</summary>
        public readonly struct MonsterFrames
        {
            public MonsterFrames(Sprite[] idle, Sprite[] move, Sprite[] attack, Sprite[] death, Sprite[] explosion)
            { Idle = idle; Move = move; Attack = attack; Death = death; Explosion = explosion; }
            public readonly Sprite[] Idle;
            public readonly Sprite[] Move;
            public readonly Sprite[] Attack;
            public readonly Sprite[] Death;
            public readonly Sprite[] Explosion; // null/empty for sheets without an explosion row
        }

        // monster-map-1.png — 5 rows, top→bottom: idle, move, attack, hurt, death. No explosion.
        // Frame counts per row (adjust here if an animation looks sheared/misaligned after a rebuild).
        public static MonsterFrames GetMap1Frames()
        {
            var counts = new[] { 4, 6, 6, 2, 6 };
            var rows = GetAnimationRows("Assets/_Project/Art/Characters/Enemies/monster-map-1.png", "Assets/_Project/Art/Characters/Enemies/Frames/Map1", 96f, counts);
            return new MonsterFrames(Row(rows, 0), Row(rows, 1), Row(rows, 2), Row(rows, 4), System.Array.Empty<Sprite>());
        }

        // monster-map-2.png — 5 labelled rows, top→bottom: idle, move, attack, EXPLOSION, hurt/death.
        // The explosion is its own row (index 3); death is the final row (index 4).
        public static MonsterFrames GetMap2Frames()
        {
            var counts = new[] { 4, 4, 6, 5, 4 };
            var rows = GetAnimationRows("Assets/_Project/Art/Characters/Enemies/monster-map-2.png", "Assets/_Project/Art/Characters/Enemies/Frames/Map2", 96f, counts);
            return new MonsterFrames(Row(rows, 0), Row(rows, 1), Row(rows, 2), Row(rows, 4), Row(rows, 3));
        }

        private static Sprite[] Row(List<Sprite[]> rows, int index)
            => index >= 0 && index < rows.Count ? rows[index] : System.Array.Empty<Sprite>();

        // New small-monster art: LittleMonster_Move.png (walk) + LittleMonster_Attack.png (attack),
        // each a single row of 5 frames on a transparent background. Idle reuses the first move frame;
        // death reuses the move loop (no dedicated death sheet). Explosion frames are passed in for bombers.
        public static MonsterFrames GetLittleMonsterFrames(Sprite[] explosion)
        {
            var move = GetAnimationRows("Assets/_Project/Art/Characters/Enemies/LittleMonster_Move.png", "Assets/_Project/Art/Characters/Enemies/Frames/Little/Move", 96f, new[] { 5 });
            var attack = GetAnimationRows("Assets/_Project/Art/Characters/Enemies/LittleMonster_Attack.png", "Assets/_Project/Art/Characters/Enemies/Frames/Little/Attack", 96f, new[] { 5 });
            var moveFrames = Row(move, 0);
            var attackFrames = Row(attack, 0);
            var idle = moveFrames.Length > 0 ? new[] { moveFrames[0] } : System.Array.Empty<Sprite>();
            return new MonsterFrames(idle, moveFrames, attackFrames, moveFrames, explosion ?? System.Array.Empty<Sprite>());
        }

        /// <summary>
        /// Slices a multi-row sprite sheet into one frame-array per animation row. Detects the horizontal
        /// bands (skipping thin text labels by pixel mass), then divides each band into <paramref name="frameCounts"/>
        /// EQUAL-WIDTH cells — the frames in these sheets touch edge-to-edge, so gap-based splitting fails
        /// and would return the whole row as one image. Each cell is exported with a shared bottom-anchored
        /// height so the animation doesn't jump, and a white-matte knockout.
        /// Returned top-to-bottom in visual order (index 0 = topmost row).
        /// </summary>
        public static List<Sprite[]> GetAnimationRows(string sourcePath, string outputFolder, float pixelsPerUnit, int[] frameCounts)
        {
            var result = new List<Sprite[]>();
            var importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[SpriteFactory] No importer for {sourcePath}"); return result; }
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null) { Debug.LogWarning($"[SpriteFactory] Could not load {sourcePath}"); return result; }

            var pixels = source.GetPixels32();
            int w = source.width, h = source.height;

            // Per-row foreground-pixel count. A COUNT (not "any pixel") ignores stray wisps and the thin
            // text labels on the sheet, which carry little mass.
            var rowMass = new int[h];
            for (int y = 0; y < h; y++)
            {
                int m = 0, rowBase = y * w;
                for (int x = 0; x < w; x++) if (IsForeground(pixels[rowBase + x])) m++;
                rowMass[y] = m;
            }

            int rowMassFloor = Mathf.Max(6, w / 120);
            var occupiedRows = new bool[h];
            for (int y = 0; y < h; y++) occupiedRows[y] = rowMass[y] >= rowMassFloor;

            // Group scanlines into vertical bands (one per animation row).
            const int rowGap = 8;
            int minBandHeight = Mathf.Max(40, h / 20);
            var bands = new List<(int start, int end)>();
            int start = -1, end = -1, gap = 0;
            for (int y = 0; y < h; y++)
            {
                if (occupiedRows[y]) { if (start < 0) start = y; end = y; gap = 0; }
                else if (start >= 0 && ++gap >= rowGap) { bands.Add((start, end)); start = -1; end = -1; gap = 0; }
            }
            if (start >= 0) bands.Add((start, end));
            bands.RemoveAll(b => b.end - b.start + 1 < minBandHeight);
            bands.Reverse(); // texture y=0 is the bottom; we want visual top-to-bottom

            Directory.CreateDirectory(Path.Combine(Application.dataPath, outputFolder.Substring("Assets/".Length)));
            var log = new List<string>();
            for (int b = 0; b < bands.Count; b++)
            {
                int count = b < frameCounts.Length ? frameCounts[b] : 1;
                var frames = SliceBandEqual(sourcePath, source, pixels, w, bands[b].start, bands[b].end, count, $"{outputFolder}/Row{b:00}", pixelsPerUnit);
                log.Add($"row{b}: y={bands[b].start}-{bands[b].end} → {frames.Length}/{count} frames");
                result.Add(frames);
            }
            Debug.Log($"[SpriteFactory] {Path.GetFileName(sourcePath)}: {bands.Count} rows → {string.Join(", ", log)}");
            return result;
        }

        // Background = near-transparent OR near-white. Handles sheets on a white matte as well as ones with
        // a real alpha channel — an alpha-only test collapses white-backed sheets into one giant frame.
        private static bool IsForeground(Color32 p)
        {
            if (p.a < 24) return false;
            return !(p.r >= 244 && p.g >= 244 && p.b >= 244);
        }

        // Divides one band into `count` equal-width cells. First finds the band's true horizontal extent
        // (trimming empty left/right margin), then cuts evenly. Every frame keeps the SAME crop height
        // (the band's full vertical extent) so the sprite's feet stay put and the animation doesn't bob.
        private static Sprite[] SliceBandEqual(string sourcePath, Texture2D source, Color32[] pixels, int w, int bandBottom, int bandTop, int count, string outputFolder, float pixelsPerUnit)
        {
            if (count < 1) count = 1;

            // Trim empty columns at the band's left/right so equal division lands on the actual frames.
            int contentLeft = w, contentRight = -1;
            for (int x = 0; x < w; x++)
                for (int y = bandBottom; y <= bandTop; y++)
                    if (IsForeground(pixels[y * w + x])) { contentLeft = Mathf.Min(contentLeft, x); contentRight = Mathf.Max(contentRight, x); break; }
            if (contentRight < contentLeft) return System.Array.Empty<Sprite>();

            // Shared vertical crop for the whole row → consistent frame height, feet aligned.
            int top = Mathf.Min(source.height - 1, bandTop + 2);
            int bottom = Mathf.Max(0, bandBottom - 2);
            int cropHeight = top - bottom + 1;

            float cellWidth = (contentRight - contentLeft + 1) / (float)count;
            Directory.CreateDirectory(Path.Combine(Application.dataPath, outputFolder.Substring("Assets/".Length)));

            var frames = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                int cellLeft = contentLeft + Mathf.RoundToInt(i * cellWidth);
                int cellRight = contentLeft + Mathf.RoundToInt((i + 1) * cellWidth) - 1;
                cellRight = Mathf.Min(cellRight, w - 1);
                int cellW = cellRight - cellLeft + 1;
                if (cellW < 2) continue;
                var outputPath = $"{outputFolder}/Frame_{i:00}.png";
                var frame = GetCroppedSprite(sourcePath, outputPath, cellLeft, bottom, cellW, cropHeight, pixelsPerUnit, true, new Vector2(.5f, 0f));
                if (frame != null) frames.Add(frame);
            }
            return frames.ToArray();
        }

        private static Sprite GetCroppedSprite(string sourcePath, string outputPath, int x, int y, int width, int height, float pixelsPerUnit, bool knockOutWhite = false, Vector2? pivot = null)
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
            var block = source.GetPixels(x, y, width, height);
            // Sheets exported on a white matte have no alpha, so make near-white pixels transparent —
            // otherwise every frame carries an opaque white box behind the sprite.
            if (knockOutWhite)
                for (int i = 0; i < block.Length; i++)
                {
                    var c = block[i];
                    if (c.r >= .957f && c.g >= .957f && c.b >= .957f) block[i] = new Color(c.r, c.g, c.b, 0f);
                }
            cropped.SetPixels(block);
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
            // Custom pivot (e.g. bottom-centre) so animation frames of varying content keep their feet planted.
            if (pivot.HasValue)
            {
                var settings = new TextureImporterSettings();
                outputImporter.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot.Value;
                outputImporter.SetTextureSettings(settings);
            }
            outputImporter.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
        }
    }
}
