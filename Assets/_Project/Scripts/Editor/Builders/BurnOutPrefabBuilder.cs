using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Enemies;
using BurnOut.Input;
using BurnOut.Items;
using BurnOut.Player;
using BurnOut.UI;
using BurnOut.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BurnOut.Editor
{
    public static class BurnOutPrefabBuilder
    {
        private const string Root = "Assets/_Project";
        private static Sprite whiteSprite;

        [MenuItem("BurnOut/05 Build Prefabs")]
        public static void BuildPrefabs()
        {
            CreateMovementConfig();
            BurnOutSpriteFactory.GetPlayerIdleSprite();
            BurnOutSpriteFactory.GetPlatformSprite();
            BurnOutSpriteFactory.GetEnemySprite();
            BurnOutSpriteFactory.GetCheckpointSprite();
            BurnOutSpriteFactory.GetDoorSprite();
            BurnOutSpriteFactory.GetOpenDoorSprite();
            BurnOutSpriteFactory.GetFragmentSprite();
            BurnOutSpriteFactory.GetHazardSprite();
            BurnOutSpriteFactory.GetStepIslandSprite();
            BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_Key.png", "Assets/_Project/Art/Items/ITEM_Key_Cropped.png", 56f);
            BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_SanityOrb.png", "Assets/_Project/Art/Items/ITEM_SanityOrb_Cropped.png", 56f);
            CreatePlayer();
            CreateEnemyProjectile();
            CreateEnemy("PF_Enemy_Shadow", false);
            CreateEnemy("PF_MiniBoss_Shadow", true);
            CreatePickup<SanityPickup>("PF_SanityOrb", "Items", Color.cyan, "SanityItem");
            CreatePickup<HealthPickup>("PF_HealthPickup", "Items", Color.red, "Untagged");
            CreatePickup<KeyPickup>("PF_Key", "Items", Color.yellow, "Key");
            CreatePickup<MentalFragmentPickup>("PF_MentalFragment", "Items", new Color(.85f, .35f, 1f), "MentalFragment");
            CreateCheckpoint(); CreateDoor(); CreateExit(); CreateHazard(); CreateManager<GameManager>("PF_GameManager"); CreateManager<AudioManager>("PF_AudioManager");
            CreateUiPrefab<PlayerHUD>("PF_PlayerHUD"); CreateUiPrefab<BossHUD>("PF_BossHUD");
            AssetDatabase.SaveAssets();
        }

        public static void RebuildPrototypePrefabs()
        {
            var generatedPrefabs = new[]
            {
                "Player/PF_Player", "Enemies/PF_Enemy_Shadow", "Enemies/PF_MiniBoss_Shadow", "Enemies/PF_EnemyProjectile",
                "Items/PF_SanityOrb", "Items/PF_HealthPickup", "Items/PF_Key", "Items/PF_MentalFragment",
                "Environment/PF_Checkpoint", "Environment/PF_LockedDoor", "Environment/PF_LevelExit", "Environment/PF_Hazard_Spikes",
                "Systems/PF_GameManager", "Systems/PF_AudioManager", "UI/PF_PlayerHUD", "UI/PF_BossHUD"
            };
            foreach (var prefab in generatedPrefabs) AssetDatabase.DeleteAsset($"{Root}/Prefabs/{prefab}.prefab");
            BuildPrefabs();
        }

        public static GameObject LoadPrefab(string name, string category) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Prefabs/{category}/{name}.prefab");

        public static void RepairPlayerPrefab()
        {
            const string path = Root + "/Prefabs/Player/PF_Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) UpgradeExistingPlayerPrefab(path);
        }

        private static void CreateMovementConfig()
        {
            var path = Root + "/ScriptableObjects/PlayerMovementConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(path) == null) AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PlayerMovementConfig>(), path);
        }

        private static void CreatePlayer()
        {
            const string path = Root + "/Prefabs/Player/PF_Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                UpgradeExistingPlayerPrefab(path);
                return;
            }
            var go = CreateVisual("PF_Player", new Color(.35f, .8f, 1f), "Player", "Player", new Vector2(.8f, 1.4f));
            var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 3f; body.freezeRotation = true;
            go.AddComponent<CapsuleCollider2D>().size = new Vector2(.72f, 1.35f);
            var playerInput = go.AddComponent<PlayerInput>();
            playerInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root + "/Input/BurnOutInputActions.inputactions");
            playerInput.defaultActionMap = "Gameplay";
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            go.AddComponent<PlayerInputReader>();
            var movement = go.AddComponent<PlayerMovement>();
            var sanity = go.AddComponent<PlayerSanity>();
            var health = go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerInventory>();
            var combat = go.AddComponent<PlayerCombat>();
            var visualAnimator = go.AddComponent<PlayerVisualAnimator>();
            go.AddComponent<PlayerFeedbackFX>();
            var ground = new GameObject("GroundCheck").transform; ground.SetParent(go.transform); ground.localPosition = new Vector3(0f, -.72f, 0f);
            var attack = CreateHitbox("AttackHitbox", go.transform, 1, .8f, 1); attack.transform.localPosition = new Vector3(.72f, .1f, 0f);
            var skill = CreateHitbox("SkillHitbox", go.transform, 2, 1.2f, 2); skill.transform.localPosition = new Vector3(.95f, .1f, 0f);
            var movementObject = new SerializedObject(movement);
            movementObject.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(Root + "/ScriptableObjects/PlayerMovementConfig.asset");
            movementObject.FindProperty("groundCheck").objectReferenceValue = ground;
            movementObject.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
            movementObject.FindProperty("visual").objectReferenceValue = go.transform;
            movementObject.ApplyModifiedPropertiesWithoutUndo();
            var combatObject = new SerializedObject(combat);
            combatObject.FindProperty("normalAttackHitbox").objectReferenceValue = attack;
            combatObject.FindProperty("skillHitbox").objectReferenceValue = skill;
            combatObject.ApplyModifiedPropertiesWithoutUndo();
            ConfigurePlayerAnimation(visualAnimator, go.GetComponent<SpriteRenderer>());
            SavePrefab(go, path);
        }

        private static void UpgradeExistingPlayerPrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var playerInput = root.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root + "/Input/BurnOutInputActions.inputactions");
                    playerInput.defaultActionMap = "Gameplay";
                    playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                }

                var movement = root.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    var data = new SerializedObject(movement);
                    data.FindProperty("visual").objectReferenceValue = root.transform;
                    data.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Hitbox2D CreateHitbox(string name, Transform parent, int damage, float knockback, float size)
        {
            var hit = new GameObject(name); hit.transform.SetParent(parent); hit.layer = LayerMask.NameToLayer("PlayerAttack");
            var box = hit.AddComponent<BoxCollider2D>(); box.size = new Vector2(size, .7f); box.isTrigger = true;
            var hitbox = hit.AddComponent<Hitbox2D>();
            var serialized = new SerializedObject(hitbox);
            serialized.FindProperty("damage").intValue = damage;
            serialized.FindProperty("knockback").floatValue = knockback;
            serialized.FindProperty("targetLayers").intValue = 1 << LayerMask.NameToLayer("Enemy");
            serialized.FindProperty("hitboxCollider").objectReferenceValue = box;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return hitbox;
        }

        private static void CreateEnemy(string name, bool boss)
        {
            var path = Root + "/Prefabs/Enemies/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual(name, boss ? new Color(.7f, .15f, .55f) : new Color(.7f, .25f, .3f), "Enemy", "Enemy", boss ? new Vector2(1.5f, 2.1f) : new Vector2(.9f, 1.3f));
            go.AddComponent<BoxCollider2D>();
            var health = go.AddComponent<EnemyHealth>();
            var brain = !boss ? go.AddComponent<EnemyBrain>() : null;
            if (boss) go.AddComponent<MiniBossController>();
            if (!boss) { var animator = go.AddComponent<EnemyVisualAnimator>(); ConfigureEnemyAnimation(animator, go.GetComponent<SpriteRenderer>()); }
            var healthData = new SerializedObject(health); healthData.FindProperty("maxHealth").intValue = boss ? 20 : 3; healthData.ApplyModifiedPropertiesWithoutUndo();
            if (brain != null) { var brainData = new SerializedObject(brain); brainData.FindProperty("energyProjectilePrefab").objectReferenceValue = LoadPrefab("PF_EnemyProjectile", "Enemies"); brainData.ApplyModifiedPropertiesWithoutUndo(); }
            SavePrefab(go, path);
        }

        private static void CreatePickup<T>(string name, string category, Color color, string tag) where T : Component
        {
            var path = Root + "/Prefabs/" + category + "/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual(name, color, "Item", tag, new Vector2(.45f, .45f));
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            go.AddComponent<T>();
            SavePrefab(go, path);
        }

        private static void CreateEnemyProjectile()
        {
            const string path = Root + "/Prefabs/Enemies/PF_EnemyProjectile.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual("PF_EnemyProjectile", Color.magenta, "EnemyAttack", "Untagged", new Vector2(.3f, .3f));
            var collider = go.AddComponent<CircleCollider2D>(); collider.isTrigger = true;
            var hitbox = go.AddComponent<Hitbox2D>(); go.AddComponent<Projectile>(); go.AddComponent<EnemyProjectile>();
            var hitboxData = new SerializedObject(hitbox); hitboxData.FindProperty("damage").intValue = 1; hitboxData.FindProperty("knockback").floatValue = 5f; hitboxData.FindProperty("targetLayers").intValue = 1 << LayerMask.NameToLayer("Player"); hitboxData.FindProperty("hitboxCollider").objectReferenceValue = collider; hitboxData.ApplyModifiedPropertiesWithoutUndo();
            var trail = go.AddComponent<TrailRenderer>(); trail.time = .28f; trail.startWidth = .18f; trail.endWidth = .02f; trail.startColor = new Color(.25f, .95f, 1f, .9f); trail.endColor = new Color(.5f, .1f, 1f, 0f);
            SavePrefab(go, path);
        }

        private static void CreateCheckpoint()
        {
            const string path = Root + "/Prefabs/Environment/PF_Checkpoint.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual("PF_Checkpoint", new Color(.4f, .95f, .8f), "Interactable", "Checkpoint", new Vector2(.65f, 1.4f));
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var component = go.AddComponent<Checkpoint>();
            var data = new SerializedObject(component);
            data.FindProperty("visual").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            data.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(go, path);
        }

        private static void CreateDoor()
        {
            const string path = Root + "/Prefabs/Environment/PF_LockedDoor.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual("PF_LockedDoor", new Color(.85f, .7f, .15f), "Interactable", "Untagged", new Vector2(.55f, 2.6f));
            var box = go.AddComponent<BoxCollider2D>(); box.isTrigger = true;
            var component = go.AddComponent<LockedDoor>();
            var data = new SerializedObject(component); data.FindProperty("blockingCollider").objectReferenceValue = box; data.FindProperty("visual").objectReferenceValue = go.GetComponent<SpriteRenderer>(); data.FindProperty("openSprite").objectReferenceValue = BurnOutSpriteFactory.GetOpenDoorSprite(); data.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(go, path);
        }

        private static void CreateExit()
        {
            const string path = Root + "/Prefabs/Environment/PF_LevelExit.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual("PF_LevelExit", Color.white, "Interactable", "LevelExit", new Vector2(1f, 2f)); go.AddComponent<BoxCollider2D>().isTrigger = true; go.AddComponent<LevelExit>(); SavePrefab(go, path);
        }

        private static void CreateHazard()
        {
            const string path = Root + "/Prefabs/Environment/PF_Hazard_Spikes.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = CreateVisual("PF_Hazard_Spikes", Color.gray, "Hazard", "Untagged", new Vector2(1f, .35f)); go.AddComponent<BoxCollider2D>().isTrigger = true; go.AddComponent<Hazard>(); SavePrefab(go, path);
        }

        private static void CreateManager<T>(string name) where T : Component
        {
            var path = Root + "/Prefabs/Systems/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = new GameObject(name); go.AddComponent<T>(); SavePrefab(go, path);
        }

        private static void CreateUiPrefab<T>(string name) where T : Component
        {
            var path = Root + "/Prefabs/UI/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            var go = new GameObject(name); go.AddComponent<T>(); SavePrefab(go, path);
        }

        private static GameObject CreateVisual(string name, Color color, string layer, string tag, Vector2 scale)
        {
            var go = new GameObject(name); go.layer = LayerMask.NameToLayer(layer); go.tag = tag;
            var renderer = go.AddComponent<SpriteRenderer>();
            var playerSprite = name == "PF_Player" ? BurnOutSpriteFactory.GetPlayerIdleSprite() : null;
            var itemSprite = name == "PF_Key"
                ? BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_Key.png", "Assets/_Project/Art/Items/ITEM_Key_Cropped.png", 56f)
                : name == "PF_SanityOrb"
                    ? BurnOutSpriteFactory.GetTrimmedSprite("Assets/_Project/Art/Items/ITEM_SanityOrb.png", "Assets/_Project/Art/Items/ITEM_SanityOrb_Cropped.png", 56f)
                    : null;
            var environmentSprite = name == "PF_Enemy_Shadow" || name == "PF_MiniBoss_Shadow" ? BurnOutSpriteFactory.GetEnemySprite()
                : name == "PF_Checkpoint" ? BurnOutSpriteFactory.GetCheckpointSprite()
                : name == "PF_LockedDoor" || name == "PF_LevelExit" ? BurnOutSpriteFactory.GetDoorSprite()
                : name == "PF_MentalFragment" ? BurnOutSpriteFactory.GetFragmentSprite()
                : name == "PF_Hazard_Spikes" ? BurnOutSpriteFactory.GetHazardSprite()
                : null;
            var artwork = playerSprite != null ? playerSprite : itemSprite != null ? itemSprite : environmentSprite;
            renderer.sprite = artwork != null ? artwork : GetWhiteSprite();
            renderer.color = artwork != null ? Color.white : color;
            if (artwork != null)
            {
                renderer.drawMode = SpriteDrawMode.Simple;
                var artworkScale = playerSprite != null ? .95f
                    : itemSprite != null ? .85f
                    : name == "PF_MiniBoss_Shadow" ? 1.25f
                    : name == "PF_Enemy_Shadow" ? .8f
                    : name == "PF_Checkpoint" ? .44f
                    : name == "PF_LockedDoor" || name == "PF_LevelExit" ? .72f
                    : name == "PF_MentalFragment" ? .68f
                    : name == "PF_Hazard_Spikes" ? .8f : 1f;
                go.transform.localScale = new Vector3(artworkScale, artworkScale, 1f);
            }
            else
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = scale;
            }
            return go;
        }

        private static Sprite GetWhiteSprite()
        {
            if (whiteSprite == null) whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            return whiteSprite;
        }

        private static void ConfigurePlayerAnimation(PlayerVisualAnimator animator, SpriteRenderer renderer)
        {
            var data = new SerializedObject(animator);
            data.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            SetSprites(data, "idleFrames", new[] { BurnOutSpriteFactory.GetPlayerIdleSprite() });
            SetSprites(data, "runFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Move.png", "Assets/_Project/Art/Characters/Player/Frames/Run", 48f));
            SetSprites(data, "jumpFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Jump.png", "Assets/_Project/Art/Characters/Player/Frames/Jump", 48f));
            SetSprites(data, "lowSanityFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_LowSanity.png", "Assets/_Project/Art/Characters/Player/Frames/LowSanity", 48f));
            SetSprites(data, "attackFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Skill02.png", "Assets/_Project/Art/Characters/Player/Frames/Attack", 48f));
            SetSprites(data, "skillFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Skill03.png", "Assets/_Project/Art/Characters/Player/Frames/Skill", 48f));
            SetSprites(data, "dashFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Skill03.png", "Assets/_Project/Art/Characters/Player/Frames/Dash", 48f));
            SetSprites(data, "deathFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Player/Player_Death.png", "Assets/_Project/Art/Characters/Player/Frames/Death", 48f));
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureEnemyAnimation(EnemyVisualAnimator animator, SpriteRenderer renderer)
        {
            var data = new SerializedObject(animator);
            var move = BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Enemies/Enemy_Move.png", "Assets/_Project/Art/Characters/Enemies/Frames/Move", 64f);
            data.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            SetSprites(data, "idleFrames", move.Length > 0 ? new[] { move[0] } : System.Array.Empty<Sprite>());
            SetSprites(data, "moveFrames", move);
            SetSprites(data, "attackFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Enemies/Enemy_Attack.png", "Assets/_Project/Art/Characters/Enemies/Frames/Attack", 64f));
            SetSprites(data, "deathFrames", BurnOutSpriteFactory.GetAnimationFrames("Assets/_Project/Art/Characters/Enemies/Enemy_Death.png", "Assets/_Project/Art/Characters/Enemies/Frames/Death", 64f));
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSprites(SerializedObject target, string propertyName, Sprite[] sprites)
        {
            var property = target.FindProperty(propertyName);
            property.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
