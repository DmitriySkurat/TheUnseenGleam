using System.Collections;
using TMPro;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EntryPoint
{
    public class GameBootstrap : MonoBehaviour
    {
        //public static GameBootstrap Instance { get; private set; }
        [SerializeField] private GameServiceRegistry serviceRegistry;


        private IEnumerator Start()
        {
            // Пусть GameBootstrap умирает, все равно ServiceRegistry будет жить

            // if (Instance != null)
            // {
            //     Destroy(gameObject);
            //     yield break;
            // }

            // Instance = this;
            // DontDestroyOnLoad(gameObject);

            var task = serviceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;
            
            // Имитация загрузки
            var loadingDuration = 1f;
            while(loadingDuration > 0f) {
                loadingDuration -= Time.deltaTime;
                Debug.Log("Loading... " + (1f - loadingDuration) * 100f + "%");
                yield return null;
            }
            Debug.Log("Loading complete! Starting game...");
            
            Debug.Log("Game loaded.");
            
            Utility.SceneLoader.Load(Utility.SceneNames.Menu);
        }
    }
}