using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
