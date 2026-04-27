using System;
using System.Collections;
using UnityEngine;

namespace EntryPoint
{
    public class CutsceneEntryPoint : SceneBootstrap
    {
        public static event Action OnBootstrapComplete;

        [SerializeField] private SessionBootstrap _sessionBootstrap;
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.1f;
        [SerializeField, Range(0f, 2f)] private float _globalLightIntensity = 1f;

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

            ApplyCutsceneSettings();

            var input = Services.Get<InputManager>();
            input.EnableGameplay();
            input.SetLookAroundEnabled(false);

            _logger?.Log("Cutscene scene initialization complete", this);

            OnBootstrapComplete?.Invoke();
        }

        private void ApplyCutsceneSettings()
        {
            var vignette = FindAnyObjectByType<VignetteController>();
            vignette?.ForceIntensity(_vignetteIntensity);

            if (Services.IsRegistered<LightSystem>())
                Services.Get<LightSystem>().UpdateGlobalLight(_globalLightIntensity, Color.white);

            var hotbar = FindAnyObjectByType<HotbarUI>();
            if (hotbar != null)
                hotbar.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
