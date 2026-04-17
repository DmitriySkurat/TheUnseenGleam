using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public class MenuEntryPoint : SceneBootstrap
    {
        [SerializeField] private MenuSceneServiceRegistry _menuServiceRegistry;
        
        
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
        
        
        protected override IEnumerator Bootstrap()
        {
            var task = _menuServiceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
            {
                Debug.LogError($"[MenuEntryPoint] Service registry initialization failed: {task.Exception?.Flatten().InnerException?.Message}\n{task.Exception?.Flatten().InnerException?.StackTrace}", this);
                yield break;
            }

            _logger?.Log("Menu scene services initialized", this);

            FindInializableObjects();
            InitializeSceneObjects();

            _logger?.Log("Menu scene initialization complete", this);
        }
    }
}