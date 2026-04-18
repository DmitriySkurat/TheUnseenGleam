using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public abstract class SceneBootstrap : LifecycleBootstrap<ISceneLifecycle>
    {
        // Просто для запуска сцен, после завершения разработки удалить
        [SerializeField] private GameServiceRegistry gameServiceRegistry;

        protected override void Awake()
        {
            StartCoroutine(InitializeAndBootstrap());
        }

        private IEnumerator InitializeAndBootstrap()
        {
            if (!Services.IsRegistered<InputManager>())
            {
                var task = gameServiceRegistry.InitializeAsync();
                while (!task.IsCompleted)
                    yield return null;
            }

            StartCoroutine(Bootstrap());
        }
    }
}
