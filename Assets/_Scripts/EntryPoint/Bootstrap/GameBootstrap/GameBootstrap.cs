using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class GameBootstrap : BootstrapBase
    {
        //[Header("Loading Screen")]
        //[SerializeField] private LoadingScreen _loadingScreen;

        protected override IEnumerator Bootstrap()
        {
            GameSettingsLoader.LoadAndApplySavedSettings();
            yield return null; // ждём следующий кадр — Screen.SetResolution применится

            if (Camera.main != null)
            {
                Camera.main.rect = new Rect(0, 0, 1, 1);
                Camera.main.aspect = (float)Screen.width / Screen.height;
            }

            var task = _registry.InitializeAsync();
            while (!task.IsCompleted)
                yield return null;

            // if (_loadingScreen != null)
            //     yield return StartCoroutine(_loadingScreen.Play());

            Utility.SceneLoader.Load(SceneNames.Menu);
        }
    }
}