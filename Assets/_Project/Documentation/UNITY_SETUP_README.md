# Unity Setup Readme

Open `D:\code\nlinh\BurnOut\BurnOut` in Unity Hub using Unity `6000.5.4f1`. Let Package Manager resolve the installed Input System, Cinemachine 3, URP, and TextMeshPro packages.

Run the single menu command: `BurnOut > Run Full Setup`.

It creates folders (if absent), copies named source art without overwriting existing copies, registers tags/layers and Input Actions, creates prefabs, builds `SC_MainMenu` and `SC_Level01`, adds them to Build Settings in order, and prints a validation summary. If Unity requests a restart after Input System configuration, restart then run the command once more.

If the Console identifies a source-sheet slicing issue, follow `ASSET_SLICE_GUIDE.md`; the prototype otherwise uses safe placeholder sprites.
