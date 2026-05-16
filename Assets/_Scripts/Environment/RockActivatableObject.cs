using UnityEngine;
using UnityEngine.Events;

public class RockActivatableObject : MonoBehaviour, IRockActivatable
{
    [SerializeField] private bool toggleable = false;
    [SerializeField] private bool activateOnlyOnce = true;

    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private bool _activated;
    private bool _isOn;

    public void OnHitByRock(Vector2 hitPoint, GameObject rockSource)
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
    }
}
