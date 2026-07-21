# Extending Burn Out

## Enemy

Create a prefab based on `PF_Enemy_Shadow`, add `EnemyHealth` and a focused behaviour (for example a dash or projectile component), set health/damage fields, layer it as `Enemy`, and place it through `BurnOutSceneBuilder`. Use `IDamageable`; do not change `PlayerCombat`.

## Pickup

Subclass `PickupBase`, implement `Apply(PlayerHealth)`, create an `Item`-layer prefab with trigger collider, and add it to the builder. Existing pickups are examples; core pickup flow stays unchanged.

## Ability or hazard

Add a small component that owns only the new rule. For abilities, use a `Hitbox2D` or `Projectile` and bind the action in `BurnOutInputActions`. For hazards, use a trigger collider and `DamageInfo`. Do not modify UI from these components.

## UI indicator or scene

Subscribe a UI component to the appropriate player/enemy event, then create it in the scene builder. New scenes should be created by a builder method and added to `EditorBuildSettings`, not edited as raw YAML.
