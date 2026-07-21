# Burn Out — Project Audit

Date: 2026-07-21

## Project identity

- Unity project root: `D:\code\nlinh\BurnOut\BurnOut`
- Unity Editor declared by project: `6000.5.4f1`
- Product name: `BurnOut` (the UI will present it as **Burn Out**)
- Template state: a fresh Universal Render Pipeline 2D project with one `SampleScene`.
- Input handling: Input System package is installed and the project setting is `activeInputHandler: 1`, the Unity serialization value for **Input System Package (New)**. No legacy gameplay input is required.

## Installed packages relevant to the prototype

| Capability | Package | Version | Decision |
| --- | --- | --- | --- |
| Input | `com.unity.inputsystem` | 1.19.0 | Use `PlayerInput` and the generated input asset. |
| Camera | `com.unity.cinemachine` | 3.1.7 | Use Cinemachine 3 `CinemachineCamera`; builder creates it when available. |
| Rendering | `com.unity.render-pipelines.universal` | 17.6.0 | Keep the existing URP 2D pipeline and use UI overlay effects rather than requiring a custom shader. |
| UI | `com.unity.ugui` | 2.5.0 | Use Canvas with TextMeshPro UI components. |
| Tests | `com.unity.test-framework` | 1.7.0 | Include small pure-C# Edit Mode tests. |

## Existing project content

- `Assets/Scenes/SampleScene.unity` is the sole build scene.
- `Assets/Settings` contains the URP 2D renderer, volume profile, and template Input Actions asset.
- No gameplay scripts, prefabs, imported art, or custom project structure currently exists.
- `EditorBuildSettings.asset` currently references only `Assets/Scenes/SampleScene.unity`.

## Source-art inventory

The source folder `D:\code\nlinh\ano` contains 24 PNG files. All remain untouched; the importer copies selected files into `Assets/_Project/Art` using ASCII English names.

| Source | Pixel size | Intended destination |
| --- | ---: | --- |
| `BG_Far_BurnoutRealm.png` | 1672 x 941 | `Art/Backgrounds/BG_Far_BurnoutRealm.png` |
| `BG_Mid_Ruins.png` | 1672 x 941 | `Art/Backgrounds/BG_Mid_Ruins.png` |
| `ENV_Platform_Tiles.png` | 1920 x 1080 | `Art/Environment/Platforms/ENV_Platform_Tiles.png` |
| `ENV_Interactables.png` | 1920 x 1080 | `Art/Environment/Interactables/ENV_Interactables.png` |
| `move.png`, `jump.png`, `die.png`, `low sanity.png` | mixed | Player sprite-sheet candidates |
| `skill 1.png`, `skill 2.png`, `skill 3.png` | 1536 x 1024 | Player skill-sheet candidates |
| `quai vat di chuyen.png`, `quai vật tấn công.png`, `quai vat chet.png` | 1902 x 1080 | Enemy sprite-sheet candidates |
| `chìa.png`, `item sanity.png`, `giấy.png`, `đá.png`, `bậc.png`, `thanh hp.png` | mixed | Items, props, UI, and environment candidates |
| four `ChatGPT Image ...` files | 1254 x 1254 | Unclassified art; retained as source and not imported automatically. |

## Findings and delivery plan

1. Create the required `_Project` layout, documentation, runtime modules, editor automation, input asset, and test assembly.
2. Use a Unity editor builder—not hand-authored scene YAML—to create `SC_MainMenu`, `SC_Level01`, prefabs, build settings, tags, layers, and a playable prototype route.
3. The builder uses simple geometric fallback sprites wherever a source image cannot be safely sliced. This keeps the prototype playable without making unsafe slicing assumptions.
4. Import the two background images and the named source art conservatively. A separate slice guide identifies source images requiring a deliberate manual slice decision.
5. Run Unity in batch mode to execute the setup and validation if a Unity executable can be discovered on this machine. At audit time, the expected Unity Hub installation path was not present, so no compile claim is made yet.

## Constraints honoured

- No files in `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `Build`, or `Builds` are modified.
- No source asset in `D:\code\nlinh\ano` is edited or deleted.
- The project is currently in the audit phase; no gameplay implementation existed before this report.
