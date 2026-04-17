using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace EntryPoint
{
    public abstract class SceneBootstrap : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] protected Utility.Logger _logger;

        protected List<IInitializable> _initializables;
        protected List<IDisposable> _disposables;

        protected virtual void Awake()
        {
            StartCoroutine(Bootstrap());
        }

        protected abstract IEnumerator Bootstrap();


        protected void FindInializableObjects()
        {
            //var currentScene = gameObject.scene;

            _initializables = Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                //.Where(mb => mb.gameObject.scene == currentScene)
                .OfType<IInitializable>()
                .OrderBy(i => i.Order)
                .ToList();

            _logger?.Log($"Found {_initializables.Count} initializables", this);
        }

        protected void InitializeSceneObjects()
        {
            foreach (var obj in _initializables)
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

            // Сохраняем всех IDisposable
            _disposables = _initializables
                .OfType<IDisposable>()
                .ToList();
        }

        protected virtual void OnDestroy()
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