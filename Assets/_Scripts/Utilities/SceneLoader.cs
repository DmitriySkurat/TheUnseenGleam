using UnityEngine.SceneManagement;

namespace Utility
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            if (Services.IsRegistered<ScreenFader>())
                Services.Get<ScreenFader>().FadeOutAndLoad(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
    }
}
