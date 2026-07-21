using UnityEngine.SceneManagement;

namespace BurnOut.Core
{
    public static class SceneLoader
    {
        public static void LoadLevel01() => SceneManager.LoadScene("SC_Level01");
        public static void LoadMainMenu() => SceneManager.LoadScene("SC_MainMenu");
    }
}
