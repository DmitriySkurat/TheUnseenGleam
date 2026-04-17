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
                var sessionTask = _sessionBootstrap.StartSession();
                while (!sessionTask.IsCompleted)
                    yield return null;
            }

            var task = _registry.InitializeAsync();
            while (!task.IsCompleted)
                yield return null;

            FindObjects();
            InitializeObjects();

            _logger?.Log("Gameplay scene initialization complete", this);
        }
    }
}
