using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EntryPoint
{
    public abstract class LifecycleBootstrap<T> : BootstrapBase where T : ILifecycle
    {
        [Header("Debug")]
        [SerializeField] protected Utility.Logger _logger;

        protected List<T> _lifecycleObjects;
        protected bool _isDisposed;

        protected void FindObjects()
        {
            _lifecycleObjects = Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .Where(ShouldInclude)
                .OfType<T>()
                .OrderBy(i => i.Order)
                .ToList();

            _logger?.Log($"Found {_lifecycleObjects.Count} {typeof(T).Name} objects", this);
        }

        /// <summary>
        /// Фильтр для FindObjects. По умолчанию — все объекты.
        /// SceneBootstrap переопределяет, чтобы брать только объекты из своей сцены.
        /// </summary>
        protected virtual bool ShouldInclude(MonoBehaviour mb) => true;

        protected void InitializeObjects()
        {
            foreach (var obj in _lifecycleObjects)
            {
                _logger?.Log($"Initializing {obj.GetType().Name} | Order: {obj.Order}", obj as Object);
                obj.Initialize();
            }
        }

        public virtual void DisposeObjects()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_lifecycleObjects == null || _lifecycleObjects.Count == 0)
                return;

            for (int i = _lifecycleObjects.Count - 1; i >= 0; i--)
            {
                var disposable = _lifecycleObjects[i];
                _logger?.Log($"Disposing {disposable.GetType().Name}", disposable as Object);
                disposable.Dispose();
            }

            _lifecycleObjects.Clear();
        }

        protected virtual void DisposeServiceRegistry()
        {
            _registry?.Dispose();
        }

        /// <summary>
        /// Страховочный вызов: если Dispose уже был вызван явно (из TransitionManager или EntryPoint),
        /// оба метода будут no-op благодаря флагу и пустым спискам реестра.
        /// </summary>
        protected virtual void OnDestroy()
        {
            DisposeObjects();
            DisposeServiceRegistry();
        }
    }
}
