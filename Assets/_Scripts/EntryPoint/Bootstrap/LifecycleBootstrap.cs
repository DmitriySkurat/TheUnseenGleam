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

        protected void FindObjects()
        {
            _lifecycleObjects = Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                )
                .OfType<T>()
                .OrderBy(i => i.Order)
                .ToList();

            _logger?.Log($"Found {_lifecycleObjects.Count} {typeof(T).Name} objects", this);
        }

        protected void InitializeObjects()
        {
            foreach (var obj in _lifecycleObjects)
            {
                _logger?.Log($"Initializing {obj.GetType().Name} | Order: {obj.Order}", obj as Object);
                obj.Initialize();
            }
        }

        protected virtual void DisposeObjects()
        {
            if (_lifecycleObjects == null || _lifecycleObjects.Count == 0)
                return;

            for (int i = _lifecycleObjects.Count - 1; i >= 0; i--)
            {
                var disposable = _lifecycleObjects[i];
                _logger?.Log($"Disposing {disposable.GetType().Name}", disposable as Object);
                disposable.Dispose();
            }
        }

        protected virtual void DisposeServiceRegistry()
        {
            _registry?.Dispose();
        }

        protected virtual void OnDestroy()
        {
            DisposeObjects();
            DisposeServiceRegistry();
        }
    }
}
