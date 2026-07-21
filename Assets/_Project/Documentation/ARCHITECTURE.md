# Burn Out Architecture

Runtime code is grouped by feature under `Scripts/Runtime`. `Core` owns scene, checkpoint, and game lifecycle; `Input` emits player input events; `Player`, `Enemies`, `Items`, and `World` own gameplay; `UI` and `Audio` subscribe to state rather than being referenced by gameplay.

Data flow: Input System → `PlayerInputReader` → player components → C# events/interfaces → UI/audio. Damage flows through `IDamageable` and `DamageInfo`; pickups extend `PickupBase`; health, sanity, and inventory expose events for UI.

Scene lifecycle: `SC_MainMenu` calls `SceneLoader.LoadLevel01`; `SC_Level01` has one `GameManager`, `AudioManager`, `CheckpointManager`, Player, camera, UI, and gameplay route. The editor builder owns scene creation; do not hand-edit generated scene YAML.

To extend a feature, add a focused component or pickup subclass, configure it in a prefab, and connect it in `BurnOutSceneBuilder`. Core classes need no modification for a normal new pickup, enemy, or hazard.
