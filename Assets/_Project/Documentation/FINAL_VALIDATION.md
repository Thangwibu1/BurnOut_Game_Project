# Final Validation

## Completed static checks

- Required folder layout, scripts, input asset, editor automation, and documentation exist.
- Runtime source contains no UnityEditor references and no legacy `FindObjectOfType`, `GameObject.Find`, or tag-find calls.
- The code separates player, combat, enemy, item, world, audio, and UI responsibilities; gameplay does not reference TMP/UI controls.
- Source art was not modified.

## Not verified in this environment

Unity Editor was not installed at the expected locations and could not be invoked, therefore asset import, C# compilation, prefab/scene generation, and Play Mode flow are **not claimed as verified**. Open the project in Unity `6000.5.4f1`, run `BurnOut > Run Full Setup`, then review the Console and run the checklist.

## Known prototype limits

- Source sprite sheets are not auto-sliced; placeholder visuals are used for generated gameplay prefabs.
- The mini-boss uses a simple dash attack, which satisfies the requested projectile-or-dash variant but is intentionally not a complex boss AI.
- The low-sanity effect is a Canvas overlay; it remains functional if URP volume effects are disabled.
