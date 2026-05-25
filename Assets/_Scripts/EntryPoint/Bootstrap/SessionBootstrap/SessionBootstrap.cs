using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace EntryPoint
{
    public class SessionBootstrap : LifecycleBootstrap<ISessionLifecycle>
    {
        [SerializeField] private AudioClip _gameplayMusic;

        private bool _disposed;

        protected override void Awake() { }

        public async Task StartSession()
        {
            DontDestroyOnLoad(gameObject);
            Services.Register(this);

            if (_registry != null)
                await _registry.InitializeAsync();

            FindObjects();
            InitializeObjects();

            if (_gameplayMusic != null && Services.IsRegistered<AudioManager>())
                Services.Get<AudioManager>().PlayMusic(_gameplayMusic);

            _logger?.Log("Session started", this);
        }

        public void EndSession()
        {
            if (_disposed) return;
            _disposed = true;

            DisposeObjects();
            DisposeServiceRegistry();
            Services.Unregister<SessionBootstrap>();
            Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            EndSession();
        }

        protected override IEnumerator Bootstrap() { yield break; }
    }
}
