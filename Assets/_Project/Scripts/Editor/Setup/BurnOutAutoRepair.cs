using UnityEditor;

namespace BurnOut.Editor
{
    [InitializeOnLoad]
    public static class BurnOutAutoRepair
    {
        static BurnOutAutoRepair()
        {
            EditorApplication.delayCall += RepairGeneratedPlayerPrefab;
        }

        private static void RepairGeneratedPlayerPrefab()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += RepairGeneratedPlayerPrefab;
                return;
            }

            BurnOutProjectSetup.RunFullSetup();
        }
    }
}
