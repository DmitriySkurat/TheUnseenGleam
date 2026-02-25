using System.Linq;
using UnityEngine;
using TMPro;
using NUnit.Framework;
using HSM;

[RequireComponent(typeof(Collider2D))]
    public class PlayerInteractor : MonoBehaviour {
        [Header("Detection")]
        public float interactRadius = 1.2f;
        public LayerMask interactableLayer;

        [Header("References")]
        public Transform interactOrigin;
        public TextMeshProUGUI promptText; // optional UI element

        PlayerContext ctx;

        private IInteractable current;
        
        public PlayerInteractor(PlayerContext ctx)
        {
            this.ctx = ctx;
        }
        

        void Awake() {
            if (interactOrigin == null) interactOrigin = transform;
        }

        void Update() {
            ScanForInteractable();

            UpdatePromptUI();
            
            if (ctx.isInteracting) {
                Interact();
            }
        }

        void ScanForInteractable() {
            var hits = Physics2D.OverlapCircleAll(interactOrigin.position, interactRadius, interactableLayer);
            IInteractable nearest = null;
            float best = float.MaxValue;

            foreach (var c in hits) {
                // ищем компонент, реализующий IInteractable
                var interactable = c.GetComponentInParent<MonoBehaviour>() as IInteractable;
                if (interactable == null) {
                    // пробуем через GetComponents
                    var comps = c.GetComponentsInParent<MonoBehaviour>();
                    foreach (var comp in comps) {
                        if (comp is IInteractable ii) { interactable = ii; break;}
                    }
                }
                if (interactable == null) continue;

                float dist = Vector2.SqrMagnitude(((MonoBehaviour)interactable).transform.position - interactOrigin.position);
                if (dist < best) { best = dist; nearest = interactable; }
            }

            if (!ReferenceEquals(nearest, current)) {
                if (current != null) current.OnDefocus();
                current = nearest;
                if (current != null) current.OnFocus();
            }
        }

        void UpdatePromptUI() {
            if (promptText == null) return;
            if (current != null) promptText.text = current.InteractionPrompt;
            else promptText.text = "";
        }

        public void Interact() {
            if (current != null) {
                current.Interact(gameObject);
            }
        }

        // визуализация радиуса в редакторе
        void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            var origin = interactOrigin != null ? interactOrigin.position : transform.position;
            Gizmos.DrawWireSphere(origin, interactRadius);
        }
    }