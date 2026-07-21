# Asset Slice Guide

Manual slicing is required before using the following art for animation: `Player_Move`, `Player_Jump`, `Player_Death`, `Player_LowSanity`, `Player_Skill01`–`03`, `Enemy_Move`, `Enemy_Attack`, `Enemy_Death`, `ENV_Platform_Tiles`, and `ENV_Interactables`.

The supplied PNGs have mixed dimensions and no reliable frame metadata. In Unity's Sprite Editor, inspect each visual grid, slice only after confirming the frame size, set player/enemy pivots to Bottom Center, and create clips/controllers. Do not use automatic slicing without that inspection.
