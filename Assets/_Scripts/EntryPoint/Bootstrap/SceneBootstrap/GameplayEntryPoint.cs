using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class GameplayEntryPoint : SceneBootstrap
    {
        [SerializeField] private SessionBootstrap _sessionBootstrap;

        protected override IEnumerator Bootstrap()
        {
            if (!Services.IsRegistered<SessionBootstrap>())
            {
                var sessionTask = _sessionBootstrap.StartSession();
                while (!sessionTask.IsCompleted)
                    yield return null;
            }

            var task = _registry.InitializeAsync();
            while (!task.IsCompleted)
                yield return null;

            FindSceneObjects();
            InitializeSceneObjects();

            _logger?.Log("Gameplay scene initialization complete", this);
        }
    }
}
