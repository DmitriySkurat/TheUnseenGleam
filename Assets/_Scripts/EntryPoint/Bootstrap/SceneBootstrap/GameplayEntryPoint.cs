using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class GameplayEntryPoint : SceneBootstrap
    {
        [SerializeField] private SessionBootstrap _sessionBootstrap;

        protected override IEnumerator Bootstrap()
        {
            // Сркываем курсор - перенести в отдельный сервис, который будет управлять состоянием курсора в зависимости от сцены и контекста
            // Cursor.visible = false;
            
            if (!Services.IsRegistered<SessionBootstrap>())
            {
                var sessionInstance = Instantiate(_sessionBootstrap);
                var sessionTask = sessionInstance.StartSession();
                while (!sessionTask.IsCompleted)
                    yield return null;
            }

            var task = _registry.InitializeAsync();
            while (!task.IsCompleted)
                yield return null;

            FindObjects();
            InitializeObjects();

            Services.Register<GameplayEntryPoint>(this);
            _logger?.Log("Gameplay scene initialization complete", this);
        }

        protected override void OnDestroy()
        {
            if (Services.IsRegistered<GameplayEntryPoint>())
                Services.Unregister<GameplayEntryPoint>();
            base.OnDestroy();
        }
    }
}
