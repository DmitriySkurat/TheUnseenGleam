using UnityEngine;
using System.Collections;

namespace EntryPoint
{
    public abstract class SceneBootstrap : LifecycleBootstrap<ISceneLifecycle>
    {
        // Просто для запуска сцен, после завершения разработки удалить
        [SerializeField] private GameServiceRegistry gameServiceRegistry;

        /// <summary>
        /// Ограничиваем поиск объектами из той же сцены, что и сам Bootstrap.
        /// Без этого при аддитивной загрузке FindObjects подхватывал бы объекты
        /// из уже выгружаемой старой сцены.
        /// </summary>
        protected override bool ShouldInclude(MonoBehaviour mb)
            => mb.gameObject.scene == gameObject.scene;

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
