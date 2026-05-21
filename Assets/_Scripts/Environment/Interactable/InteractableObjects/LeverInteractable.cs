using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : Interactable {
    [SerializeField] private bool isOn = false;
    [SerializeField] private float animationDuration = 0.3f;

    private Animator _animator;
    private static readonly int LeverTime = Animator.StringToHash("LeverTime");

    public UnityEvent<bool> onLeverToggle;

    public override void Initialize() {
        base.Initialize();
        _animator = GetComponent<Animator>();
        if (_animator != null)
            _animator.SetFloat(LeverTime, isOn ? 1f : 0f);
    }

    public override void OnInteract(Interactor interactor) {
        isOn = !isOn;
        StopAllCoroutines();
        StartCoroutine(AnimateLever(isOn));
        onLeverToggle?.Invoke(isOn);
    }

    private IEnumerator AnimateLever(bool turningOn) {
        float current = _animator.GetFloat(LeverTime);
        float target = turningOn ? 1f : 0f;
        float elapsed = 0f;
        float duration = Mathf.Abs(target - current) * animationDuration;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            _animator.SetFloat(LeverTime, Mathf.Lerp(current, target, elapsed / duration));
            yield return null;
        }
        _animator.SetFloat(LeverTime, target);
    }
}
