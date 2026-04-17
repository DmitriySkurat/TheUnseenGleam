using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace EntryPoint
{
    public abstract class SceneBootstrap : BootstrapBase
    {
        [Header("Debug")]
        [SerializeField] protected Utility.Logger _logger;
        
        protected List<ISceneLifecycle> _sceneObjects;
        
        
        // Просто для запуска сцен, после завершения разработки удалить
        [SerializeField] private GameServiceRegistry gameServiceRegistry;
        protected override void Awake()
        {
            StartCoroutine(InitializeAndBootstrap());
        }
        private IEnumerator InitializeAndBootstrap()
        {
            // Пропускаем инициализацию глобальных сервисов, если они уже зарегистрированы
            // (например, при аддитивной загрузке сцены через SceneTransitionManager)
            if (!Services.IsRegistered<InputManager>())
            {
                var task = gameServiceRegistry.InitializeAsync();
                while (!task.IsCompleted)
                    yield return null;
            }

            StartCoroutine(Bootstrap());
        }
        // КОНЕЦ ---------

        protected void FindSceneObjects()
        {
            //var currentScene = gameObject.scene;

            _sceneObjects = Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                //.Where(mb => mb.gameObject.scene == currentScene)
                .OfType<ISceneLifecycle>()
                .OrderBy(i => i.Order)
                .ToList();

            _logger?.Log($"Found {_sceneObjects.Count} scene objects", this);
        }

        protected void InitializeSceneObjects()
        {
            foreach (var obj in _sceneObjects)
            {
                _logger?.Log($"Initializing {obj.GetType().Name} | Order: {obj.Order}", obj as Object);

                
                obj.Initialize();
                
                // try
                // {
                //     obj.Initialize();
                // }
                // catch (System.Exception e)
                // {
                //     Debug.LogError($"[SceneBootstrap] Failed to initialize {obj.GetType().Name}: {e.Message}\n{e.StackTrace}", obj as Object);
                // }
            }
        }
        
        protected virtual void DisposeSceneObjects()
        {
            if (_sceneObjects == null || _sceneObjects.Count == 0)
                return;

            _logger?.Log("Disposing scene objects...", this);

            // Dispose в ОБРАТНОМ порядке
            for (int i = _sceneObjects.Count - 1; i >= 0; i--)
            {
                var disposable = _sceneObjects[i];

                _logger?.Log(
                    $"Disposing {disposable.GetType().Name}",
                    disposable as Object
                );

                disposable.Dispose();
            }

            _logger?.Log("Scene disposed successfully", this);
        }
        
        protected virtual void DisposeServiceRegistry()
        {
            if (_registry == null) 
                return;
                
            _registry?.Dispose();
        }

        protected virtual void OnDestroy()
        {
            DisposeSceneObjects();
            DisposeServiceRegistry();
        }
    }
}