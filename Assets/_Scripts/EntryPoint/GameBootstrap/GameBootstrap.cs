using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameServiceRegistry serviceRegistry;


        private IEnumerator Start()
        {
            GameSettingsLoader.LoadAndApplySavedSettings();
            yield return null; // ждём следующий кадр — Screen.SetResolution применится
            
            if (Camera.main != null)
            {
                Camera.main.rect = new Rect(0, 0, 1, 1);
                Camera.main.aspect = (float)Screen.width / Screen.height;
            }

            var task = serviceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;
            
            // Имитация загрузки
            var loadingDuration = 0.3f;
            var loading = loadingDuration;
            while(loading > 0f) {
                loading -= Time.deltaTime;
                Debug.Log("Loading... " + (loadingDuration - loading) * 100f + "%");
                yield return null;
            }
            
            Debug.Log("Loading complete! Starting game...");
            
            Utility.SceneLoader.Load(Utility.SceneNames.Menu);
        }
    }
}