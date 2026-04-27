using System;
using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class CutsceneEntryPoint : SceneBootstrap
    {
        public static event Action OnBootstrapComplete;

        [SerializeField] private SessionBootstrap _sessionBootstrap;
        
        protected override IEnumerator Bootstrap()
        {
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

            var input = Services.Get<InputManager>();
            input.EnableGameplay();
            input.SetLookAroundEnabled(false);

            _logger?.Log("Cutscene scene initialization complete", this);

            OnBootstrapComplete?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
