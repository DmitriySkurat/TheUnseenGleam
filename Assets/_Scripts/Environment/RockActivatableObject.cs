using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RockActivatableObject : MonoBehaviour, IRockActivatable
{
    [SerializeField] private bool toggleable = false;
    [SerializeField] private bool activateOnlyOnce = true;

    [Header("Hit Nudge")]
    [SerializeField] private float nudgeDistance = 0.12f;
    [SerializeField] private float nudgeDuration  = 0.18f;

    [Header("Auto Reset")]
    [SerializeField] private float resetDelay = 3f;

    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private bool _activated;
    private bool _isOn;
    private Vector3 _originLocalPosition;
    private bool _originCaptured;
    private Coroutine _resetCoroutine;

    public virtual void OnHitByRock(Vector2 hitPoint, GameObject rockSource)
    {
        if (!_originCaptured)
        {
            _originLocalPosition = transform.localPosition;
            _originCaptured = true;
        }

        if (activateOnlyOnce && _activated) return;

        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }

        _activated = true;

        if (toggleable)
        {
            _isOn = !_isOn;
            if (_isOn) onActivated?.Invoke();
            else       onDeactivated?.Invoke();
        }
        else
        {
            onActivated?.Invoke();
        }

        Vector2 raw  = (Vector2)transform.position - hitPoint;
        Vector2 axis = Mathf.Abs(raw.x) >= Mathf.Abs(raw.y)
            ? new Vector2(Mathf.Sign(raw.x), 0f)
            : new Vector2(0f, Mathf.Sign(raw.y));

        _resetCoroutine = StartCoroutine(NudgeThenResetRoutine(axis));
    }

    private IEnumerator NudgeThenResetRoutine(Vector2 dir)
    {
        Vector3 nudgeTarget = _originLocalPosition + (Vector3)(dir * nudgeDistance);

        // Nudge forward
        yield return SmoothMove(transform.localPosition, nudgeTarget, nudgeDuration);

        // Hold
        yield return new WaitForSeconds(resetDelay);

        // Return to origin
        yield return SmoothMove(transform.localPosition, _originLocalPosition, nudgeDuration);

        // Reverse activation state
        if (toggleable)
        {
            if (_isOn)
            {
                _isOn = false;
                onDeactivated?.Invoke();
            }
        }
        else
        {
            onDeactivated?.Invoke();
        }

        _activated = false;
        _resetCoroutine = null;
    }

    private IEnumerator SmoothMove(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.LerpUnclamped(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.localPosition = to;
    }
}
