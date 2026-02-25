using UnityEngine;

[RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour, IInteractable {
        [TextArea] public string interactionPrompt = "Press E to interact";
        [Header("Visuals")]
        public bool highlightOnFocus = true;
        public Color highlightColor = Color.yellow;


        public virtual string InteractionPrompt => interactionPrompt;

        public virtual void OnFocus() {
            if (!highlightOnFocus) return;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = highlightColor;
        }

        public virtual void OnDefocus() {
            if (!highlightOnFocus) return;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }

        public abstract void Interact(GameObject interactor);
    }