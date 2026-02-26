using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM {
    public class AudioLoopActivity : Activity {
        readonly AudioSource source;

        public AudioLoopActivity(AudioSource source) {
            this.source = source;
        }

        public override Task ActivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Inactive || source == null) return Task.CompletedTask;
            Mode = ActivityMode.Activating;
            source.Play();
            Mode = ActivityMode.Active;
            return Task.CompletedTask;
        }

        public override Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active || source == null) return Task.CompletedTask;
            Mode = ActivityMode.Deactivating;
            source.Stop();
            Mode = ActivityMode.Inactive;
            return Task.CompletedTask;
        }
    }
}
