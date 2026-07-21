using BurnOut.Audio;
using BurnOut.Core;
using BurnOut.Enemies;
using BurnOut.Player;
using BurnOut.UI;
using BurnOut.World;
using Unity.Cinemachine;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BurnOut.Editor
{
    public static class BurnOutSceneBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private static Sprite whiteSprite;

        [MenuItem("BurnOut/06 Build Scenes")]
        public static void BuildScenes()
        {
            BuildLevel();
            BuildMainMenu();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneFolder + "/SC_MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneFolder + "/SC_Level01.unity", true)
            };
            AssetDatabase.SaveAssets();
        }

        [MenuItem("BurnOut/08 Repair UI Input In Open Scene")]
        public static void RepairUiInputInOpenScene()
        {
            EnsureInputSystemEventSystem();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[BurnOut] Replaced the legacy UI input module with Input System UI input.");
        }

        private static void BuildLevel()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var systems = Parent("Systems");
            var manager = InstantiatePrefab("PF_GameManager", "Systems", systems.transform).GetComponent<GameManager>();
            InstantiatePrefab("PF_AudioManager", "Systems", systems.transform);
            new GameObject("CheckpointManager").transform.SetParent(systems.transform);
            systems.transform.GetChild(systems.transform.childCount - 1).gameObject.AddComponent<CheckpointManager>();

            var backgrounds = Parent("Backgrounds");
            // These paintings are full frames rather than tileable strips.  Keep them camera centred
            // so travelling through the level cannot reveal an empty side of the image.
            CreateBackground("BG_Far", "Assets/_Project/Art/Backgrounds/BG_Far_BurnoutRealm.png", Vector3.zero, -100, backgrounds.transform, 1.32f, Color.white);
            CreateBackground("BG_Mid", "Assets/_Project/Art/Backgrounds/BG_Mid_Ruins.png", new Vector3(.35f, -.1f, 0f), -90, backgrounds.transform, 1.36f, new Color(1f, 1f, 1f, .35f));
            var environment = Parent("Environment"); var groundParent = Parent("Ground", environment.transform); var platforms = Parent("Platforms", environment.transform);
            // A deliberate five-beat route: learn movement -> earn key -> breach gate -> survive encounters -> boss/exit.
            CreateGround("ArrivalFloor", new Vector2(1f, -1f), new Vector2(10f, 1f), groundParent.transform);
            CreateStepIsland("LessonStep01", new Vector2(9f, -.3f), platforms.transform);
            CreateStepIsland("LessonStep02", new Vector2(15f, 1.25f), platforms.transform);
            CreateGround("ReflectionFloor", new Vector2(23f, -1f), new Vector2(10f, 1f), groundParent.transform);
            CreateStepIsland("KeyLedge", new Vector2(29f, .65f), platforms.transform);
            CreateStepIsland("KeyLedgeHigh", new Vector2(35f, 1.8f), platforms.transform);
            CreateGround("GateHallFloor", new Vector2(43f, -1f), new Vector2(11f, 1f), groundParent.transform);
            CreateStepIsland("GateHallCover", new Vector2(47f, .15f), platforms.transform);
            CreateGround("PressureFloor", new Vector2(60f, -1f), new Vector2(16f, 1f), groundParent.transform);
            CreateStepIsland("PressureStep01", new Vector2(55f, .25f), platforms.transform);
            CreateStepIsland("PressureStep02", new Vector2(65f, 1.25f), platforms.transform);
            CreateStepIsland("PressureBridge", new Vector2(70f, .1f), platforms.transform);
            CreateGround("RooftopFloor", new Vector2(87f, -1f), new Vector2(26f, 1f), groundParent.transform);
            CreateStepIsland("RooftopStep01", new Vector2(76f, .35f), platforms.transform);
            CreateStepIsland("RooftopStep02", new Vector2(87f, 1.55f), platforms.transform);
            CreateGround("ReliefExitFloor", new Vector2(108f, -1f), new Vector2(16f, 1f), groundParent.transform);
            CreateStepIsland("ReliefExitStep", new Vector2(101f, .45f), platforms.transform);
            CreateGround("RecoveryFloor", new Vector2(127f, -1f), new Vector2(18f, 1f), groundParent.transform);
            CreateStepIsland("RecoveryBridge", new Vector2(117f, .1f), platforms.transform);
            CreateStepIsland("RecoveryStep01", new Vector2(121f, .2f), platforms.transform);
            CreateStepIsland("RecoveryStep02", new Vector2(132f, 1.3f), platforms.transform);
            CreateGround("GardenApproach", new Vector2(149f, -1f), new Vector2(20f, 1f), groundParent.transform);
            CreateStepIsland("GardenBridge", new Vector2(137.5f, .1f), platforms.transform);
            CreateStepIsland("GardenStep", new Vector2(143f, .35f), platforms.transform);
            var hazard = InstantiatePrefab("PF_Hazard_Spikes", "Environment", environment.transform); hazard.transform.position = new Vector3(18.5f, -.28f, 0f);

            var interactables = Parent("Interactables");
            var spawnCheckpoint = InstantiatePrefab("PF_Checkpoint", "Environment", interactables.transform); spawnCheckpoint.transform.position = new Vector3(1f, .35f, 0f);
            var key = InstantiatePrefab("PF_Key", "Items", interactables.transform); key.transform.position = new Vector3(35f, 3.6f, 0f);
            var door = InstantiatePrefab("PF_LockedDoor", "Environment", interactables.transform); door.transform.position = new Vector3(153f, .85f, 0f);
            var bossCheckpoint = InstantiatePrefab("PF_Checkpoint", "Environment", interactables.transform); bossCheckpoint.transform.position = new Vector3(50f, .35f, 0f);
            var sanity = InstantiatePrefab("PF_SanityOrb", "Items", interactables.transform); sanity.transform.position = new Vector3(15f, 3.25f, 0f);
            var health = InstantiatePrefab("PF_HealthPickup", "Items", interactables.transform); health.transform.position = new Vector3(24f, .35f, 0f);
            var exit = InstantiatePrefab("PF_LevelExit", "Environment", interactables.transform); exit.transform.position = new Vector3(158f, .85f, 0f);

            var enemies = Parent("Enemies");
            CreateEncounter("Encounter_Reflection", new Vector3(23f, 1f, 0f), new Vector2(5f, 5f), new[] { new Vector3(-1.5f, -.8f), new Vector3(2.2f, -.8f) }, 1, enemies.transform);
            CreateEncounter("Encounter_GateHall", new Vector3(44f, 1f, 0f), new Vector2(6f, 5f), new[] { new Vector3(-2.5f, -.8f), new Vector3(2.5f, -.8f) }, 2, enemies.transform);
            CreateEncounter("Encounter_Pressure", new Vector3(61f, 1f, 0f), new Vector2(8f, 5f), new[] { new Vector3(-3f, -.8f), new Vector3(3f, -.8f) }, 1, enemies.transform);
            CreateEncounter("Encounter_Rooftop", new Vector3(77f, 1f, 0f), new Vector2(8f, 5f), new[] { new Vector3(-3f, -.8f), new Vector3(2f, -.8f) }, 1, enemies.transform);
            var boss = InstantiatePrefab("PF_MiniBoss_Shadow", "Enemies", enemies.transform); boss.name = "MiniBoss"; boss.transform.position = new Vector3(96f, .4f, 0f); boss.SetActive(false);
            var player = InstantiatePrefab("PF_Player", "Player", null); player.name = "Player"; player.transform.position = new Vector3(1f, .5f, 0f);

            var cameras = Parent("Cameras");
            var mainCamera = new GameObject("Main Camera"); mainCamera.tag = "MainCamera"; mainCamera.transform.SetParent(cameras.transform); mainCamera.transform.position = new Vector3(1f, 2f, -10f);
            var camera = mainCamera.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5.5f; camera.backgroundColor = new Color(.05f, .03f, .1f);
            var cinemachineBrain = mainCamera.AddComponent<CinemachineBrain>(); cinemachineBrain.enabled = false;
            var cinemachine = new GameObject("CM_PlayerFollow").AddComponent<CinemachineCamera>(); cinemachine.transform.SetParent(cameras.transform); cinemachine.Follow = player.transform; cinemachine.enabled = false;
            var cameraFollow = mainCamera.AddComponent<CameraFollow2D>();
            var cameraData = new SerializedObject(cameraFollow); cameraData.FindProperty("target").objectReferenceValue = player.transform; cameraData.ApplyModifiedPropertiesWithoutUndo();

            var ui = CreateLevelUi(manager, player.GetComponent<PlayerHealth>());
            var arena = new GameObject("BossArenaTrigger"); arena.transform.SetParent(interactables.transform); arena.transform.position = new Vector3(89f, 1f, 0f); var trigger = arena.AddComponent<BoxCollider2D>(); trigger.size = new Vector2(2f, 5f); trigger.isTrigger = true;
            var arenaComponent = arena.AddComponent<BossArenaTrigger>();
            var arenaData = new SerializedObject(arenaComponent); arenaData.FindProperty("boss").objectReferenceValue = boss.GetComponent<MiniBossController>(); arenaData.FindProperty("bossHud").objectReferenceValue = ui.GetComponentInChildren<BossHUD>(true); arenaData.ApplyModifiedPropertiesWithoutUndo();
            var miniBoss = boss.GetComponent<MiniBossController>(); var bossData = new SerializedObject(miniBoss); bossData.FindProperty("mentalFragmentPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_MentalFragment", "Items"); bossData.ApplyModifiedPropertiesWithoutUndo();
            CreateWorldText("Move: A/D or Arrow Keys     Jump: Space     Dash: Left Shift", new Vector3(4f, 2f, 0f));
            CreateWorldText("Find the key. Defeat the shadow to recover your fragment.", new Vector3(31f, 4f, 0f));
            CreateWorldText("Survive the pressure hall. The rooftop is close.", new Vector3(68f, 4f, 0f));
            CreateWorldText("Defeat the source of pressure, then carry the fragment to the Garden.", new Vector3(123f, 4f, 0f));
            EditorSceneManager.SaveScene(scene, SceneFolder + "/SC_Level01.unity");
        }

        private static void BuildMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera"); camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10); camera.AddComponent<Camera>().orthographic = true;
            new GameObject("AudioManager").AddComponent<AudioManager>();
            var canvas = CreateCanvas("MainMenuUI");
            CreateMenuBackground(canvas.transform);
            var menu = canvas.gameObject.AddComponent<MainMenuController>();
            var start = CreateMenuButtonHitTarget("Start", canvas.transform, new Vector2(-660, -80)); UnityEventTools.AddPersistentListener(start.onClick, menu.StartGame);
            var settings = CreateMenuButtonHitTarget("Settings", canvas.transform, new Vector2(-660, -175)); UnityEventTools.AddPersistentListener(settings.onClick, menu.OpenSettings);
            var quit = CreateMenuButtonHitTarget("Exit", canvas.transform, new Vector2(-660, -265)); UnityEventTools.AddPersistentListener(quit.onClick, menu.ExitGame);
            var settingsPanel = CreatePanel("SettingsPanel", canvas.transform, new Vector2(520, 360)); settingsPanel.SetActive(false);
            CreateLabel("SettingsTitle", settingsPanel.transform, "SETTINGS", 30, new Vector2(0, 120), new Vector2(440, 60));
            var settingsUi = settingsPanel.AddComponent<SettingsUI>();
            var master = CreateSlider("MasterVolume", settingsPanel.transform, new Vector2(40, 55)); master.value = 1f; CreateLabel("MasterLabel", settingsPanel.transform, "MASTER", 16, new Vector2(-150, 55), new Vector2(120, 30)); UnityEventTools.AddPersistentListener(master.onValueChanged, settingsUi.SetMasterVolume);
            var music = CreateSlider("MusicVolume", settingsPanel.transform, new Vector2(40, 5)); music.value = .7f; CreateLabel("MusicLabel", settingsPanel.transform, "MUSIC", 16, new Vector2(-150, 5), new Vector2(120, 30)); UnityEventTools.AddPersistentListener(music.onValueChanged, settingsUi.SetMusicVolume);
            var sfx = CreateSlider("SfxVolume", settingsPanel.transform, new Vector2(40, -45)); sfx.value = .8f; CreateLabel("SfxLabel", settingsPanel.transform, "SFX", 16, new Vector2(-150, -45), new Vector2(120, 30)); UnityEventTools.AddPersistentListener(sfx.onValueChanged, settingsUi.SetSfxVolume);
            var back = CreateButton("Back", settingsPanel.transform, "BACK", new Vector2(0, -100)); UnityEventTools.AddPersistentListener(back.onClick, menu.CloseSettings);
            var menuData = new SerializedObject(menu); menuData.FindProperty("settingsPanel").objectReferenceValue = settingsPanel; menuData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, SceneFolder + "/SC_MainMenu.unity");
        }

        private static GameObject CreateLevelUi(GameManager manager, PlayerHealth player)
        {
            var canvas = CreateCanvas("UI");
            var hud = new GameObject("PlayerHUD").AddComponent<PlayerHUD>(); hud.transform.SetParent(canvas.transform, false);
            var hudArtwork = BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/UI/UI_PlayerHUD.png", "Assets/_Project/Art/UI/UI_PlayerHUD_Cropped.png", 100f);
            CreateHudArtwork("HudArtwork", canvas.transform, hudArtwork, new Vector2(42, -42), new Vector2(820, 78));
            var keySprite = BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_Key.png", "Assets/_Project/Art/Items/ITEM_Key_Cropped.png", 56f);
            var key = CreateHudArtwork("KeyIcon", canvas.transform, keySprite, new Vector2(-64, -52), new Vector2(58, 58), true).gameObject; key.SetActive(false);
            var overlay = CreatePanel("LowSanityOverlay", canvas.transform, new Vector2(1800, 1000)); overlay.GetComponent<Image>().color = new Color(.22f, 0f, .35f, .18f); overlay.SetActive(false);
            var hudData = new SerializedObject(hud); hudData.FindProperty("playerHealth").objectReferenceValue = player; hudData.FindProperty("playerSanity").objectReferenceValue = player.GetComponent<PlayerSanity>(); hudData.FindProperty("inventory").objectReferenceValue = player.GetComponent<PlayerInventory>(); hudData.FindProperty("keyIcon").objectReferenceValue = key; hudData.FindProperty("lowSanityOverlay").objectReferenceValue = overlay; hudData.ApplyModifiedPropertiesWithoutUndo();
            var bossPanel = CreatePanel("BossHUD", canvas.transform, new Vector2(450, 70)); bossPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 245); var bossSlider = CreateSlider("BossHealth", bossPanel.transform, Vector2.zero); var bossHud = bossPanel.AddComponent<BossHUD>(); var bossData = new SerializedObject(bossHud); bossData.FindProperty("panel").objectReferenceValue = bossPanel; bossData.FindProperty("healthBar").objectReferenceValue = bossSlider; bossData.ApplyModifiedPropertiesWithoutUndo(); bossPanel.SetActive(false);
            var pause = CreatePanel("PausePanel", canvas.transform, new Vector2(420, 300)); CreateLabel("PauseTitle", pause.transform, "PAUSED", 38, new Vector2(0, 75), new Vector2(300, 60)); var pauseMenu = pause.AddComponent<PauseMenu>(); var resume = CreateButton("Resume", pause.transform, "RESUME", new Vector2(0, 0)); UnityEventTools.AddPersistentListener(resume.onClick, pauseMenu.Resume); var restart = CreateButton("Restart", pause.transform, "RESTART", new Vector2(0, -70)); UnityEventTools.AddPersistentListener(restart.onClick, pauseMenu.Restart); pause.SetActive(false);
            var complete = CreatePanel("LevelCompletePanel", canvas.transform, new Vector2(550, 300)); CreateLabel("Complete", complete.transform, "MENTAL FRAGMENT RECOVERED\nLEVEL COMPLETE", 31, new Vector2(0, 45), new Vector2(500, 120)); complete.SetActive(false);
            var over = CreatePanel("GameOverPanel", canvas.transform, new Vector2(450, 250)); CreateLabel("Over", over.transform, "LOST IN THE VOID", 32, new Vector2(0, 40), new Vector2(400, 100)); over.SetActive(false);
            var managerData = new SerializedObject(manager); managerData.FindProperty("pausePanel").objectReferenceValue = pause; managerData.FindProperty("gameOverPanel").objectReferenceValue = over; managerData.FindProperty("levelCompletePanel").objectReferenceValue = complete; managerData.ApplyModifiedPropertiesWithoutUndo();
            return canvas.gameObject;
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            EnsureInputSystemEventSystem();
            return canvas;
        }

        private static void EnsureInputSystemEventSystem()
        {
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule), typeof(InputSystemUiSetup));
                return;
            }

            foreach (var legacyModule in eventSystem.GetComponents<StandaloneInputModule>()) Object.DestroyImmediate(legacyModule);
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            if (eventSystem.GetComponent<InputSystemUiSetup>() == null) eventSystem.gameObject.AddComponent<InputSystemUiSetup>();
        }

        private static void CreateMenuBackground(Transform parent)
        {
            var go = new GameObject("MenuArtwork", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<RawImage>();
            image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/UI/UI_MainMenu.png");
            image.raycastTarget = false;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = size; go.GetComponent<Image>().color = new Color(.04f, .03f, .1f, .88f); return go;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = dimensions; var label = go.GetComponent<TextMeshProUGUI>(); label.text = text; label.fontSize = size; label.alignment = TextAlignmentOptions.Center; label.color = Color.white; return label;
        }

        private static Button CreateButton(string name, Transform parent, string text, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(220, 52); go.GetComponent<Image>().color = new Color(.22f, .13f, .35f, 1f); CreateLabel("Text", go.transform, text, 22, Vector2.zero, new Vector2(220, 52)); return go.GetComponent<Button>();
        }

        private static Button CreateMenuButtonHitTarget(string name, Transform parent, Vector2 position)
        {
            var go = new GameObject(name + "HitTarget", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420, 100);
            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            return go.GetComponent<Button>();
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(200, 20); return go.GetComponent<Slider>();
        }

        private static GameObject Parent(string name, Transform parent = null) { var go = new GameObject(name); if (parent != null) go.transform.SetParent(parent); return go; }
        private static GameObject InstantiatePrefab(string name, string category, Transform parent) { return (GameObject)PrefabUtility.InstantiatePrefab(BurnOutPrefabBuilder.LoadPrefab(name, category), parent); }
        private static void SpawnPrefab(string name, string category, Transform parent, Vector3 position) { var go = InstantiatePrefab(name, category, parent); go.transform.position = position; }
        private static void CreateGround(string name, Vector2 position, Vector2 size, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.layer = LayerMask.NameToLayer("Ground"); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = BurnOutSpriteFactory.GetPlatformSprite(); renderer.color = Color.white; renderer.drawMode = SpriteDrawMode.Sliced; renderer.size = size; var collider = go.AddComponent<BoxCollider2D>(); collider.size = size; }
        private static void CreateStepIsland(string name, Vector2 position, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.layer = LayerMask.NameToLayer("Ground"); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = BurnOutSpriteFactory.GetStepIslandSprite(); renderer.sortingOrder = 3; var collider = go.AddComponent<BoxCollider2D>(); collider.size = new Vector2(4.1f, .36f); collider.offset = new Vector2(0f, 1.4f); }
        private static void CreateEncounter(string name, Vector3 position, Vector2 size, Vector3[] spawnOffsets, int waves, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; var trigger = go.AddComponent<BoxCollider2D>(); trigger.size = size; trigger.isTrigger = true; var encounter = go.AddComponent<EncounterSpawner>(); var data = new SerializedObject(encounter); data.FindProperty("enemyPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_Enemy_Shadow", "Enemies"); data.FindProperty("rewardPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_SanityOrb", "Items"); data.FindProperty("waveCount").intValue = waves; var offsets = data.FindProperty("spawnOffsets"); offsets.arraySize = spawnOffsets.Length; for (var i = 0; i < spawnOffsets.Length; i++) offsets.GetArrayElementAtIndex(i).vector3Value = spawnOffsets[i]; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void CreateBackground(string name, string path, Vector3 offset, int sortingOrder, Transform parent, float scale, Color tint) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = new Vector3(offset.x, offset.y, 0f); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); renderer.sortingOrder = sortingOrder; renderer.color = tint; go.transform.localScale = Vector3.one * scale; var backdrop = go.AddComponent<CameraBackdrop2D>(); var data = new SerializedObject(backdrop); data.FindProperty("offset").vector2Value = offset; data.FindProperty("cameraInfluence").floatValue = 1f; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static Image CreateHudArtwork(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, bool rightAnchored = false) { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f); rect.pivot = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f); rect.anchoredPosition = position; rect.sizeDelta = size; var image = go.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static void CreateWorldText(string text, Vector3 position) { var go = new GameObject("TutorialText", typeof(TextMeshPro)); go.transform.position = position; var label = go.GetComponent<TextMeshPro>(); label.text = text; label.fontSize = 2.2f; label.alignment = TextAlignmentOptions.Center; label.color = new Color(.9f, .8f, 1f); }
        private static Sprite GetWhiteSprite() { if (whiteSprite == null) whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); return whiteSprite; }
    }
}
