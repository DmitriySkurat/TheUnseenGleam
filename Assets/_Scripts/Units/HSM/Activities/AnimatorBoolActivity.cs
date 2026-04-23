using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM {
    public class AnimatorBoolActivity : Activity {
        readonly Animator animator;
        readonly int paramHash;
        readonly bool enterValue;
        readonly bool exitValue;
        readonly bool hasParam;

        public AnimatorBoolActivity(Animator animator, int paramHash, bool enterValue = true, bool exitValue = false) {
            this.animator = animator;
            this.enterValue = enterValue;
            this.exitValue = exitValue;
            if (animator != null) {
                this.paramHash = paramHash;
                hasParam = HasParameter(animator, paramHash);
            }
        }

        public override Task ActivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Inactive || !hasParam) return Task.CompletedTask;
            Mode = ActivityMode.Activating;
            animator.SetBool(paramHash, enterValue);
            Mode = ActivityMode.Active;
            return Task.CompletedTask;
        }

        public override Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active || !hasParam) return Task.CompletedTask;
            Mode = ActivityMode.Deactivating;
            animator.SetBool(paramHash, exitValue);
            Mode = ActivityMode.Inactive;
            return Task.CompletedTask;
        }

        static bool HasParameter(Animator animator, int hash) {
            var parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++) {
                if (parameters[i].nameHash == hash) return true;
            }
            return false;
        }
    }
}
