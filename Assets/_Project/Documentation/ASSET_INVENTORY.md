# Asset Inventory

Twenty-one identified source files have been copied (without overwriting anything) from `D:/code/nlinh/ano` to named copies under `Assets/_Project/Art`. `BurnOutAssetImporter` applies the Unity import settings when `BurnOut > Run Full Setup` is run: backgrounds are single sprites; candidate sheets are multiple sprites with full-rect meshes and no compression.

Imported mappings include the two backgrounds, platform/interactable sheets, player move/jump/death/low-sanity/skills, enemy move/attack/death, key, sanity item, lore paper, rock, steps, HUD, and main-menu artwork. The four unclassified `ChatGPT Image ...` files are deliberately left unimported.

The prototype builder uses an editor-provided white fallback sprite for gameplay objects until art sheets are deliberately sliced and assigned.
