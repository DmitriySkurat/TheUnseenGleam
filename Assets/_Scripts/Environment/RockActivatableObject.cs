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

    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private bool _activated;
    private bool _isOn;

    public virtual void OnHitByRock(Vector2 hitPoint, GameObject rockSource)
    {
        if (activateOnlyOnce && _activated) return;

        _activated = true;

        if (toggleable)
        {
            _isOn = !_isOn;
            if (_isOn)
                onActivated?.Invoke();
            else
                onDeactivated?.Invoke();
        }
        else
        {
            onActivated?.Invoke();
        }

        Vector2 raw = (Vector2)transform.position - hitPoint;
        Vector2 axis = Mathf.Abs(raw.x) >= Mathf.Abs(raw.y)
            ? new Vector2(Mathf.Sign(raw.x), 0f)
            : new Vector2(0f, Mathf.Sign(raw.y));
        StartCoroutine(NudgeRoutine(axis));
    }

    private IEnumerator NudgeRoutine(Vector2 dir)
    {
        Vector3 origin = transform.localPosition;
        Vector3 target = origin + (Vector3)(dir * nudgeDistance);
        float t = 0f;

        while (t < nudgeDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.LerpUnclamped(origin, target, Mathf.SmoothStep(0f, 1f, t / nudgeDuration));
            yield return null;
        }

        transform.localPosition = target;
    }
}
