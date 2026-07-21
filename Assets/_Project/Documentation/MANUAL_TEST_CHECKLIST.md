# Manual Test Checklist

1. Open the project with Unity `6000.5.4f1`; wait for imports to finish and resolve any Console compile errors before proceeding.
2. Run `BurnOut > Run Full Setup`, then open `Assets/_Project/Scenes/SC_MainMenu` and press Play.
3. Start the level. Confirm A/D or arrows move, Space jumps, double jump works in air, and Left Shift dashes.
4. Confirm the camera follows, the player collides with ground, J/left mouse and K/right mouse damage enemies, and enemies damage the player.
5. Reach the sanity orb, key platform, locked door, checkpoint, boss, mental fragment, and level-complete panel.
6. Let HP or sanity reach zero and confirm respawn at the latest checkpoint with health and sanity restored.
7. Press Escape to pause/resume; confirm Main Menu, Restart, HUD, key state, boss health, and low-sanity overlay.
8. In `BurnOut > Validate Project`, confirm all listed checks pass. Confirm no Missing Script, NullReferenceException, or duplicate manager appears in the Console.
