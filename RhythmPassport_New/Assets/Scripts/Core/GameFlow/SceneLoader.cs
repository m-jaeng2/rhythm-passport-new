using UnityEngine.SceneManagement;

namespace RhythmPassport.Core.GameFlow
{
    public static class SceneLoader
    {
        public static void LoadBoot()
        {
            SceneManager.LoadScene(SceneNames.Boot);
        }

        public static void LoadTitle()
        {
            SceneManager.LoadScene(SceneNames.Title);
        }

        public static void LoadCalibration()
        {
            SceneManager.LoadScene(SceneNames.Calibration);
        }

        public static void LoadGameplay()
        {
            SceneManager.LoadScene(SceneNames.Gameplay);
        }

        public static void LoadResult()
        {
            SceneManager.LoadScene(SceneNames.Result);
        }
    }
}
