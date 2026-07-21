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
            // Four painted interiors placed edge to edge as fixed world backdrops — travelling right walks
            // Lily through four rooms of a derelict multi-storey building. Platforms below echo each room's floors.
            var bgTint = new Color(.88f, .9f, 1f);
            CreateZoneBackground("BG_Zone1_ArrivalHall", "Assets/_Project/Art/Backgrounds/BG_Scene1.png", 13.5f, 4f, 53f, backgrounds.transform, bgTint);
            CreateZoneBackground("BG_Zone2_Atrium", "Assets/_Project/Art/Backgrounds/BG_Scene2.png", 60f, 4f, 40f, backgrounds.transform, bgTint);
            CreateZoneBackground("BG_Zone3_Corridor", "Assets/_Project/Art/Backgrounds/BG_Scene3.png", 100f, 4f, 40f, backgrounds.transform, bgTint);
            CreateZoneBackground("BG_Zone4_Catacomb", "Assets/_Project/Art/Backgrounds/BG_Scene4.png", 145f, 4f, 50f, backgrounds.transform, bgTint);
            var environment = Parent("Environment"); var groundParent = Parent("Ground", environment.transform); var platforms = Parent("Platforms", environment.transform);
            // Solid landing floors are the safe beats; the connectors between them are staircases with real
            // gaps. A missed jump drops Lily off-screen and respawns her at the last checkpoint — a setback, not a soft-lock.
            CreateGround("ArrivalFloor", new Vector2(1f, -1f), new Vector2(10f, 1f), groundParent.transform);
            CreateGround("ReflectionFloor", new Vector2(23f, -1f), new Vector2(10f, 1f), groundParent.transform);
            CreateGround("GateHallFloor", new Vector2(43f, -1f), new Vector2(11f, 1f), groundParent.transform);
            CreateGround("PressureFloor", new Vector2(60f, -1f), new Vector2(16f, 1f), groundParent.transform);
            CreateGround("RooftopFloor", new Vector2(89f, -1f), new Vector2(26f, 1f), groundParent.transform);
            CreateGround("ReliefExitFloor", new Vector2(112f, -1f), new Vector2(14f, 1f), groundParent.transform);
            CreateGround("RecoveryFloor", new Vector2(132f, -1f), new Vector2(14f, 1f), groundParent.transform);
            CreateGround("GardenApproach", new Vector2(152f, -1f), new Vector2(16f, 1f), groundParent.transform);

            // Arrival -> Reflection: climb a staircase over an open drop, then descend the far side.
            CreateStaircase("ClimbA", new Vector2(8.5f, -.1f), 3, 3.2f, 1.15f, platforms.transform);
            CreateStepIsland("ClimbA_Peak", new Vector2(18f, 2.1f), platforms.transform, 2.2f);
            CreateStepIsland("ClimbA_Drop", new Vector2(20.5f, .7f), platforms.transform, 2.2f);
            // Reflection -> GateHall: rising staircase to a high ledge, then step down.
            CreateStaircase("ClimbB", new Vector2(30f, .1f), 3, 3.4f, 1.2f, platforms.transform);
            CreateStepIsland("ClimbB_Down", new Vector2(40f, 1.1f), platforms.transform, 2.2f);
            // GateHall -> Pressure: two quick treads across the gap.
            CreateStaircase("BridgeC", new Vector2(50.5f, .1f), 2, 2.6f, .0f, platforms.transform, 2f);
            // Pressure -> Rooftop: descend-then-ascend staircase over a spike gap.
            CreateStaircase("StairD", new Vector2(70.5f, .4f), 3, 3.2f, .55f, platforms.transform);
            // Rooftop -> ReliefExit: short hop across.
            CreateStepIsland("BridgeE", new Vector2(103.5f, .3f), platforms.transform, 2.2f);
            // ReliefExit -> Recovery: descending staircase.
            CreateStaircase("StairF", new Vector2(120.5f, .8f), 3, 3.0f, -.35f, platforms.transform);
            // Recovery -> Garden: a final demanding climb-and-drop to the gate.
            CreateStaircase("ClimbG", new Vector2(140.5f, .3f), 2, 3.4f, 1.25f, platforms.transform, 2f);
            CreateStepIsland("ClimbG_Drop", new Vector2(148f, .8f), platforms.transform, 2.2f);
            // Optional high balconies — reward exploration with sanity orbs and a breather route.
            CreateStepIsland("Zone1Balcony", new Vector2(24f, 3.4f), platforms.transform);
            CreateStepIsland("Zone2Balcony", new Vector2(60f, 3.3f), platforms.transform);
            CreateStepIsland("Zone3Balcony", new Vector2(97f, 3.1f), platforms.transform);
            CreateStepIsland("Zone4Balcony", new Vector2(132f, 3.2f), platforms.transform);
            // Spikes on the solid floors force well-timed jumps; the staircase gaps do the rest.
            var hazard = InstantiatePrefab("PF_Hazard_Spikes", "Environment", environment.transform); hazard.transform.position = new Vector3(25f, -.28f, 0f);
            var hazard2 = InstantiatePrefab("PF_Hazard_Spikes", "Environment", environment.transform); hazard2.transform.position = new Vector3(60f, -.28f, 0f);
            var hazard3 = InstantiatePrefab("PF_Hazard_Spikes", "Environment", environment.transform); hazard3.transform.position = new Vector3(133f, -.28f, 0f);

            // Depth + mood: slow drifting motes across the whole level.
            var atmosphere = new GameObject("AtmosphereFX"); atmosphere.transform.SetParent(backgrounds.transform); atmosphere.AddComponent<BurnOut.World.AtmosphereFX>();

            // Dress the ground with scattered rubble and lore papers so the route reads as living ruins, not a bare slab.
            var props = Parent("Props", environment.transform);
            var rock = BurnOutSpriteFactory.GetRockSprite();
            var rockSpots = new[] { new Vector3(4f, -.42f, 0f), new Vector3(20f, -.4f, 0f), new Vector3(26f, -.45f, 0f), new Vector3(41f, -.4f, 0f), new Vector3(58f, -.42f, 0f), new Vector3(84f, -.4f, 0f), new Vector3(92f, -.45f, 0f), new Vector3(106f, -.4f, 0f), new Vector3(126f, -.42f, 0f), new Vector3(148f, -.4f, 0f) };
            for (var i = 0; i < rockSpots.Length; i++) CreateProp("Rubble", rock, rockSpots[i], (i % 3 == 0 ? 1.15f : i % 3 == 1 ? .85f : 1f), props.transform, -18, new Color(.78f, .8f, .92f));
            var note = BurnOutSpriteFactory.GetLoreNoteSprite();
            CreateProp("LoreNote", note, new Vector3(29f, 1.15f, 0f), .8f, props.transform, -5, Color.white);
            CreateProp("LoreNote", note, new Vector3(101f, .95f, 0f), .8f, props.transform, -5, Color.white);

            var interactables = Parent("Interactables");
            // Checkpoints: one mid-route and one just before the boss arena so death is a setback, not a restart.
            var checkpointMid = InstantiatePrefab("PF_Checkpoint", "Environment", interactables.transform); checkpointMid.transform.position = new Vector3(43f, 0f, 0f);
            var checkpointBoss = InstantiatePrefab("PF_Checkpoint", "Environment", interactables.transform); checkpointBoss.transform.position = new Vector3(84f, 0f, 0f);
            var sanity = InstantiatePrefab("PF_SanityOrb", "Items", interactables.transform); sanity.transform.position = new Vector3(15f, 3.25f, 0f);
            var health = InstantiatePrefab("PF_HealthPickup", "Items", interactables.transform); health.transform.position = new Vector3(24f, .35f, 0f);
            // Single gate: carrying the boss's mental fragment through it wins the level immediately.
            var exit = InstantiatePrefab("PF_LevelExit", "Environment", interactables.transform); exit.transform.position = new Vector3(156f, .85f, 0f);

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
            // The boss drops the key on death; carrying it to the single gate wins the level.
            var miniBoss = boss.GetComponent<MiniBossController>(); var bossData = new SerializedObject(miniBoss); bossData.FindProperty("mentalFragmentPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_Key", "Items"); bossData.ApplyModifiedPropertiesWithoutUndo();
            CreateWorldText("Move: A/D or Arrows\nJump / Double Jump: Space     Dash: Left Ctrl", new Vector3(4f, 2.6f, 0f), 14f);
            CreateWorldText("Attack: Left Shift / LMB\nSkills:  Z Shockwave    X Aura (heal+shield)    C Rush (lunge)", new Vector3(4f, 1.4f, 0f), 16f);
            CreateWorldText("Slaying shadows restores sanity. The lower your sanity, the harder you hit.", new Vector3(23f, 4.4f, 0f));
            CreateWorldText("Climb the ruins. Watch your footing over the spikes.", new Vector3(31f, 4f, 0f));
            CreateWorldText("Survive the pressure hall. The rooftop is close.", new Vector3(68f, 4f, 0f));
            CreateWorldText("Defeat the shadow, grab the key it drops, then reach the gate to escape.", new Vector3(123f, 4f, 0f));
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
            // The source HUD image already paints a decorative, non-functional health bar.
            // Use one clear live meter instead, so players never see two conflicting HP bars.
            var portrait = BurnOutSpriteFactory.GetPlayerIdleSprite();
            CreateHudArtwork("Portrait", canvas.transform, portrait, new Vector2(34, -28), new Vector2(54, 54));
            var health = CreateHudMeter("HealthMeter", canvas.transform, new Vector2(98, -34), new Vector2(350, 24), new Color(.9f, .18f, .23f, .96f));
            var sanityMeter = CreateHudMeter("SanityMeter", canvas.transform, new Vector2(98, -66), new Vector2(350, 12), new Color(.16f, .9f, .92f, .92f));
            var healthText = CreateHudCounter("HealthText", canvas.transform, new Vector2(108, -37), new Vector2(220, 22));
            var sanityText = CreateHudCounter("SanityText", canvas.transform, new Vector2(108, -68), new Vector2(220, 18));
            var keySprite = BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_Key.png", "Assets/_Project/Art/Items/ITEM_Key_Cropped.png", 56f);
            var key = CreateHudArtwork("KeyIcon", canvas.transform, keySprite, new Vector2(-64, -52), new Vector2(58, 58), true).gameObject; key.SetActive(false);
            var overlay = CreatePanel("LowSanityOverlay", canvas.transform, new Vector2(1800, 1000)); overlay.GetComponent<Image>().color = new Color(.22f, 0f, .35f, .18f); overlay.SetActive(false);
            var hudData = new SerializedObject(hud); hudData.FindProperty("playerHealth").objectReferenceValue = player; hudData.FindProperty("playerSanity").objectReferenceValue = player.GetComponent<PlayerSanity>(); hudData.FindProperty("inventory").objectReferenceValue = player.GetComponent<PlayerInventory>(); hudData.FindProperty("healthBar").objectReferenceValue = health; hudData.FindProperty("sanityBar").objectReferenceValue = sanityMeter; hudData.FindProperty("healthText").objectReferenceValue = healthText; hudData.FindProperty("sanityText").objectReferenceValue = sanityText; hudData.FindProperty("keyIcon").objectReferenceValue = key; hudData.FindProperty("lowSanityOverlay").objectReferenceValue = overlay; hudData.ApplyModifiedPropertiesWithoutUndo();
            var bossPanel = CreatePanel("BossHUD", canvas.transform, new Vector2(450, 70)); bossPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 245); var bossSlider = CreateSlider("BossHealth", bossPanel.transform, Vector2.zero); var bossHud = bossPanel.AddComponent<BossHUD>(); var bossData = new SerializedObject(bossHud); bossData.FindProperty("panel").objectReferenceValue = bossPanel; bossData.FindProperty("healthBar").objectReferenceValue = bossSlider; bossData.ApplyModifiedPropertiesWithoutUndo(); bossPanel.SetActive(false);
            // Skill cooldown bar at the bottom so the three abilities read clearly.
            var skillBar = new GameObject("SkillCooldownHUD", typeof(RectTransform)); skillBar.transform.SetParent(canvas.transform, false);
            var skillRect = skillBar.GetComponent<RectTransform>(); skillRect.anchorMin = skillRect.anchorMax = new Vector2(.5f, 0f); skillRect.anchoredPosition = Vector2.zero; skillRect.sizeDelta = new Vector2(400, 160);
            skillBar.AddComponent<SkillCooldownHUD>();
            // PauseMenu lives on the always-active canvas (not the hidden panel) so its Start() runs and ESC works.
            var pauseMenu = canvas.gameObject.AddComponent<PauseMenu>();
            var pause = CreatePanel("PausePanel", canvas.transform, new Vector2(420, 420)); CreateLabel("PauseTitle", pause.transform, "PAUSED", 38, new Vector2(0, 120), new Vector2(300, 60)); var resume = CreateButton("Resume", pause.transform, "RESUME", new Vector2(0, 0)); UnityEventTools.AddPersistentListener(resume.onClick, pauseMenu.Resume); var restart = CreateButton("Restart", pause.transform, "RESTART", new Vector2(0, -70)); UnityEventTools.AddPersistentListener(restart.onClick, pauseMenu.Restart); var pauseMenuBtn = CreateButton("PauseMainMenu", pause.transform, "MAIN MENU", new Vector2(0, -140)); UnityEventTools.AddPersistentListener(pauseMenuBtn.onClick, pauseMenu.MainMenu); pause.SetActive(false);
            var complete = CreatePanel("LevelCompletePanel", canvas.transform, new Vector2(560, 340)); CreateLabel("Complete", complete.transform, "MENTAL FRAGMENT RECOVERED\nLEVEL COMPLETE", 31, new Vector2(0, 90), new Vector2(500, 120));
            var completeRestart = CreateButton("CompleteRestart", complete.transform, "PLAY AGAIN", new Vector2(0, -20)); UnityEventTools.AddPersistentListener(completeRestart.onClick, manager.RestartLevel);
            var completeMenu = CreateButton("CompleteMenu", complete.transform, "MAIN MENU", new Vector2(0, -90)); UnityEventTools.AddPersistentListener(completeMenu.onClick, manager.GoToMainMenu);
            complete.SetActive(false);
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
        private static void CreateStepIsland(string name, Vector2 position, Transform parent) { CreateStepIsland(name, position, parent, 3.8f); }
        private static void CreateStepIsland(string name, Vector2 position, Transform parent, float width) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.layer = LayerMask.NameToLayer("Ground"); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = BurnOutSpriteFactory.GetPlatformSprite(); renderer.color = Color.white; renderer.drawMode = SpriteDrawMode.Sliced; renderer.size = new Vector2(width, .52f); renderer.sortingOrder = 3; var collider = go.AddComponent<BoxCollider2D>(); collider.size = renderer.size; }

        // A run of evenly-spaced steps climbing (or descending) like a staircase. Narrower treads = trickier footing.
        private static void CreateStaircase(string name, Vector2 start, int count, float dx, float dy, Transform parent, float width = 2.2f)
        {
            for (var i = 0; i < count; i++)
                CreateStepIsland($"{name}{i:00}", new Vector2(start.x + dx * i, start.y + dy * i), parent, width);
        }
        private static void CreateEncounter(string name, Vector3 position, Vector2 size, Vector3[] spawnOffsets, int waves, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; var trigger = go.AddComponent<BoxCollider2D>(); trigger.size = size; trigger.isTrigger = true; var encounter = go.AddComponent<EncounterSpawner>(); var data = new SerializedObject(encounter); data.FindProperty("enemyPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_Enemy_Shadow", "Enemies"); data.FindProperty("rewardPrefab").objectReferenceValue = BurnOutPrefabBuilder.LoadPrefab("PF_SanityOrb", "Items"); data.FindProperty("waveCount").intValue = waves; var offsets = data.FindProperty("spawnOffsets"); offsets.arraySize = spawnOffsets.Length; for (var i = 0; i < spawnOffsets.Length; i++) offsets.GetArrayElementAtIndex(i).vector3Value = spawnOffsets[i]; data.ApplyModifiedPropertiesWithoutUndo(); }
        // A fixed-in-world painted room backdrop scaled to a target world width, sat behind everything.
        private static void CreateZoneBackground(string name, string path, float centerX, float centerY, float width, Transform parent, Color tint)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(centerX, centerY, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -100;
            renderer.color = tint;
            if (sprite != null)
            {
                float nativeWidth = sprite.bounds.size.x;
                float scale = nativeWidth > 0f ? width / nativeWidth : 1f;
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static void CreateBackground(string name, string path, Vector3 offset, int sortingOrder, Transform parent, float scale, Color tint) { var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = new Vector3(offset.x, offset.y, 0f); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); renderer.sortingOrder = sortingOrder; renderer.color = tint; go.transform.localScale = Vector3.one * scale; var backdrop = go.AddComponent<CameraBackdrop2D>(); var data = new SerializedObject(backdrop); data.FindProperty("offset").vector2Value = offset; data.FindProperty("cameraInfluence").floatValue = 1f; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static Image CreateHudArtwork(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, bool rightAnchored = false) { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f); rect.pivot = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f); rect.anchoredPosition = position; rect.sizeDelta = size; var image = go.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static Slider CreateHudMeter(string name, Transform parent, Vector2 position, Vector2 size, Color fillColor) { var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Slider)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f); rect.anchoredPosition = position; rect.sizeDelta = size; var background = go.GetComponent<Image>(); background.color = new Color(.04f, .02f, .08f, .6f); background.raycastTarget = false; var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(go.transform, false); var fillRect = fill.GetComponent<RectTransform>(); fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(2f, 2f); fillRect.offsetMax = new Vector2(-2f, -2f); var fillImage = fill.GetComponent<Image>(); fillImage.color = fillColor; fillImage.raycastTarget = false; var slider = go.GetComponent<Slider>(); slider.fillRect = fillRect; slider.direction = Slider.Direction.LeftToRight; slider.value = 1f; return slider; }
        private static TextMeshProUGUI CreateHudCounter(string name, Transform parent, Vector2 position, Vector2 size) { var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f); rect.anchoredPosition = position; rect.sizeDelta = size; var text = go.GetComponent<TextMeshProUGUI>(); text.fontSize = 15; text.alignment = TextAlignmentOptions.Left; text.color = Color.white; text.raycastTarget = false; return text; }
        private static void CreateWorldText(string text, Vector3 position) { CreateWorldText(text, position, 12f); }
        // World-space tutorial text. A TextMeshPro with no sized RectTransform stretches every string onto a
        // single runaway line; we give it a bounded width and enable wrapping so long lines flow into a
        // readable multi-line block centred on the anchor.
        private static void CreateWorldText(string text, Vector3 position, float width)
        {
            var go = new GameObject("TutorialText", typeof(TextMeshPro));
            go.transform.position = position;
            var label = go.GetComponent<TextMeshPro>();
            var rect = label.rectTransform;
            rect.sizeDelta = new Vector2(width, 3f);
            rect.pivot = new Vector2(.5f, .5f);
            label.text = text;
            label.fontSize = 2.2f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.color = new Color(.9f, .8f, 1f);
        }
        private static void CreateProp(string name, Sprite sprite, Vector3 position, float scale, Transform parent, int sortingOrder, Color tint) { if (sprite == null) return; var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = Vector3.one * scale; var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = sortingOrder; renderer.color = tint; }
        private static Sprite GetWhiteSprite() { if (whiteSprite == null) whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); return whiteSprite; }
    }
}
