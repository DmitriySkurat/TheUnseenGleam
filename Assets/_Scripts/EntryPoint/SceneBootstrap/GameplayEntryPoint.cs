using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.InputSystem.UI;

using Object = UnityEngine.Object;

namespace EntryPoint
{
    public class GameplayEntryPoint : SceneBootstrap
    {
        [SerializeField] private GameplaySceneServiceRegistry _serviceRegistry;
        
        
        // Просто для запуска сцен, после завершения разработки удалить
        [SerializeField] private GameServiceRegistry serviceRegistry;
        protected override void Awake()
        {
            StartCoroutine(InitializeAndBootstrap());
            
            // StartCoroutine(Bootstrap());
        }

        private IEnumerator InitializeAndBootstrap()
        {
            // Пропускаем инициализацию глобальных сервисов, если они уже зарегистрированы
            // (например, при аддитивной загрузке сцены через SceneTransitionManager)
            if (!Services.IsRegistered<InputManager>())
            {
                var task = serviceRegistry.InitializeAsync();
                while (!task.IsCompleted)
                    yield return null;
            }

            StartCoroutine(Bootstrap());
        }
        // КОНЕЦ ---------

        protected override void OnDestroy()
        {
            _serviceRegistry?.Dispose();
            base.OnDestroy();
        }

        protected override IEnumerator Bootstrap()
        {
            var task = _serviceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
            {
                Debug.LogError($"[GameplayEntryPoint] Service registry initialization failed: {task.Exception?.Flatten().InnerException?.Message}\n{task.Exception?.Flatten().InnerException?.StackTrace}", this);
                yield break;
            }

            _logger?.Log("Gameplay scene services initialized", this);

            FindInializableObjects();
            InitializeSceneObjects();

            _logger?.Log("Gameplay scene initialization complete", this);
        }
        
        // private void EnsureEventSystem()
        // {
        //     if (Object.FindAnyObjectByType<EventSystem>() != null)
        //         return;

        //     var eventSystemObject = new GameObject("EventSystem");
        //     eventSystemObject.AddComponent<EventSystem>();
        //     eventSystemObject.AddComponent<InputSystemUIInputModule>();

        //     _logger?.Log("Created missing EventSystem for UI mouse input", eventSystemObject);
        // }

        
    }
}
