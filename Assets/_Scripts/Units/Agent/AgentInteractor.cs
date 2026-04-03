using UnityEngine;

public class AgentInteractor : Interactor
{
    private AgentContext _ctx;
    private float _lastInteractTime = float.MinValue;

    // Door the agent opened and must close after passing through
    private IInteractable _pendingClose;
    private float _pendingCloseDoorX;
    private float _pendingCloseDirSign;

    public void Initialize(AgentContext ctx)
    {
        _ctx = ctx;
    }

    void FixedUpdate()
    {
        if (_ctx == null) return;

        TryClosePendingDoor();
        ScanForInteractable();

        if (currentInteractable != null && Time.time - _lastInteractTime >= _ctx.stats.InteractCooldown)
            TryOpenDoor();
    }

    private void TryOpenDoor()
    {
        if (currentInteractable is not DoorInteractable door) return;
        if (door.IsOpen) return;

        PerformInteraction();
        _lastInteractTime = Time.time;

        _pendingClose = currentInteractable;
        _pendingCloseDoorX = ((MonoBehaviour)currentInteractable).transform.position.x;
        _pendingCloseDirSign = FacingSign();
    }

    private void TryClosePendingDoor()
    {
        if (_pendingClose == null) return;

        // Кто-то уже закрыл дверь до нас — сбрасываем, чтобы не блокировать повторное обнаружение
        if (_pendingClose is DoorInteractable pendingDoor && !pendingDoor.IsOpen)
        {
            _pendingClose = null;
            return;
        }

        float agentX = _ctx.transform.position.x;
        bool hasPassed = _pendingCloseDirSign * (agentX - _pendingCloseDoorX) > 1.25f;
        if (!hasPassed) return;

        _pendingClose.TryInteract(this);
        _lastInteractTime = Time.time;
        _pendingClose = null;
    }

    // Направление взгляда агента: по скорости или, если стоит, по localScale (как AgentVision)
    private float FacingSign()
    {
        float velX = _ctx.rb.linearVelocity.x;
        return Mathf.Abs(velX) >= 0.01f
            ? Mathf.Sign(velX)
            : (_ctx.transform.localScale.x >= 0f ? 1f : -1f);
    }

    private void ScanForInteractable()
    {
        var dir = new Vector2(FacingSign(), 0f);
        var origin = (Vector2)_ctx.transform.position;
        var hit = Physics2D.Linecast(origin, origin + dir * _ctx.stats.InteractRange, _ctx.stats.InteractableLayer);

        if (hit.collider != null)
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            // Игнорируем дверь только пока она открыта (агент проходит сквозь неё)
            bool passingThrough = ReferenceEquals(interactable, _pendingClose)
                && _pendingClose is DoorInteractable d && d.IsOpen;
            currentInteractable = passingThrough ? null : interactable;
        }
        else
        {
            currentInteractable = null;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_ctx?.rb == null) return;

        var dir = new Vector2(FacingSign(), 0f);
        var origin = (Vector2)_ctx.transform.position;

        Gizmos.color = currentInteractable != null ? Color.green : Color.white;
        Gizmos.DrawLine(origin, origin + dir * _ctx.stats.InteractRange);

        if (_pendingClose != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(_pendingCloseDoorX, origin.y, 0f), 0.2f);
        }
    }
#endif
}
