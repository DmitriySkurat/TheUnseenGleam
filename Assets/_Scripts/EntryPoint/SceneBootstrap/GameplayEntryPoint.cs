using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EntryPoint
{
    public class GameplayEntryPoint : SceneBootstrap
    {
        [Header("Debug")]
        [SerializeField] private Utility.Logger _logger;
        
        private List<IInitializable> _initializables;
        private List<IDisposable> _disposables;
        
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

        protected override IEnumerator Bootstrap()
        {    
            // Скрываем курсор для погружения в игру
            //Cursor.visible = false;
            
            var task = _serviceRegistry.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;
            
            _logger?.Log("Scene services initialized", this);
            
            FindInializableObjects();
            InitializeSceneObjects();

            _logger?.Log("Scene initialization complete", this);
        }
        
        private void FindInializableObjects()
        {
            _initializables = Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include, 
                    FindObjectsSortMode.None
                )
                .OfType<IInitializable>()
                .OrderBy(i => i.Order)
                .ToList();

            _logger?.Log($"Found {_initializables.Count} initializables", this);
        }
        
        private void InitializeSceneObjects()
        {
            foreach (var obj in _initializables)
            {
                _logger?.Log($"Initializing {obj.GetType().Name} | Order: {obj.Order}", obj as Object);

                obj.Initialize();
            }

            // Сохраняем всех IDisposable
            _disposables = _initializables
                .OfType<IDisposable>()
                .ToList();
        }

        private void OnDestroy()
        {
            if (_disposables == null || _disposables.Count == 0)
                return;

            _logger?.Log("Disposing scene objects...", this);

            // Dispose в ОБРАТНОМ порядке
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                var disposable = _disposables[i];

                _logger?.Log(
                    $"Disposing {disposable.GetType().Name}",
                    disposable as Object
                );

                disposable.Dispose();
            }

            _logger?.Log("Scene disposed successfully", this);
        }
    }
}