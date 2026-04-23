using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM {
    public class AnimatorPlayActivity : Activity {
        readonly Animator animator;
        readonly int stateHash;
        readonly int layer;

        public AnimatorPlayActivity(Animator animator, int stateHash, int layer = 0) {
            this.animator = animator;
            this.stateHash = stateHash;
            this.layer = layer;
        }

        public override Task ActivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Inactive) return Task.CompletedTask;
            Mode = ActivityMode.Activating;
            animator.Play(stateHash, layer, 0f);
            Mode = ActivityMode.Active;
            return Task.CompletedTask;
        }

        public override Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active) return Task.CompletedTask;
            Mode = ActivityMode.Deactivating;
            Mode = ActivityMode.Inactive;
            return Task.CompletedTask;
        }
    }
}
