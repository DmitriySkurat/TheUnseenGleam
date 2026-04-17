using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace EntryPoint
{
    public class SessionBootstrap : BootstrapBase
    {
        [Header("Debug")]
        [SerializeField] protected Utility.Logger _logger;

        private bool _disposed;

        protected override void Awake() { }

        public async Task StartSession()
        {
            DontDestroyOnLoad(gameObject);
            Services.Register(this);

            if (_registry != null)
                await _registry.InitializeAsync();

            _logger?.Log("Session started", this);
        }

        public void EndSession()
        {
            if (_disposed) return;
            _disposed = true;

            _registry?.Dispose();
            Services.Unregister<SessionBootstrap>();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            EndSession();
        }

        protected override IEnumerator Bootstrap() { yield break; }
    }
}
