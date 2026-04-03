using System.Collections.Generic;
using UnityEngine;
using PlatNav;

public class AgentDoorInteractor : Interactor, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy + 10;

    [Header("Detection")]
    [SerializeField] private LayerMask doorLayer;
    [SerializeField, Min(0f)] private float interactDistance = 0.6f;
    [SerializeField, Min(0f)] private float interactCooldown = 0.4f;

    [Header("Auto Close")]
    [SerializeField, Min(0f)] private float closeDelay = 0.6f;
    [SerializeField, Min(0f)] private float closeDistance = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 1f, 0.9f);

    private Collider2D _col;
    private PlatNavHandler _nav;

    private float _nextInteractTime;

    private struct OpenDoorEntry
    {
        public DoorInteractable door;
        public Collider2D doorCollider;
        public float openedAt;
        public float sideSign;
    }

    private readonly List<OpenDoorEntry> _openedDoors = new();

    public void Initialize()
    {
        _col = GetComponent<Collider2D>();
        _nav = GetComponent<PlatNavHandler>();
        enableHighlight = false;
    }

    private void FixedUpdate()
    {
        if (_col == null) return;

        UpdateAutoClose();

        if (_nav == null || _nav.State != PlatNavState.WalkingSegment)
            return;

        if (Time.time < _nextInteractTime)
            return;

        if (!TryFindDoorAhead(out DoorInteractable door, out Collider2D doorCollider))
        {
            SetCurrentInteractable(null);
            return;
        }

        if (door == null || door.IsOpen)
            return;

        bool wasOpen = door.IsOpen;
        SetCurrentInteractable(door);
        PerformInteraction();
        if (!wasOpen && door.IsOpen)
            RegisterOpenedDoor(door, doorCollider);
        _nextInteractTime = Time.time + interactCooldown;
    }

    private bool TryFindDoorAhead(out DoorInteractable door, out Collider2D doorCollider)
    {
        door = null;
        doorCollider = null;

        if (doorLayer.value == 0)
            return false;

        Vector2 origin = _col.bounds.center;
        float dir = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 direction = new Vector2(dir, 0f);

        Vector2 end = origin + direction * interactDistance;
        var hit = Physics2D.Linecast(origin, end, doorLayer);
        if (!hit.collider)
            return false;

        door = hit.collider.GetComponentInParent<DoorInteractable>();
        if (door == null)
            return false;

        doorCollider = door.GetComponent<Collider2D>();
        if (doorCollider == null)
            doorCollider = hit.collider;

        return true;
    }

    private void RegisterOpenedDoor(DoorInteractable door, Collider2D doorCollider)
    {
        float sign = Mathf.Sign(transform.position.x - door.transform.position.x);
        if (Mathf.Abs(sign) < 0.001f)
            sign = 1f;

        _openedDoors.Add(new OpenDoorEntry
        {
            door = door,
            doorCollider = doorCollider,
            openedAt = Time.time,
            sideSign = sign
        });
    }

    private void UpdateAutoClose()
    {
        if (_openedDoors.Count == 0)
            return;

        var agentBounds = _col.bounds;
        float agentX = transform.position.x;

        for (int i = _openedDoors.Count - 1; i >= 0; i--)
        {
            var entry = _openedDoors[i];
            if (entry.door == null)
            {
                _openedDoors.RemoveAt(i);
                continue;
            }

            if (!entry.door.IsOpen)
            {
                _openedDoors.RemoveAt(i);
                continue;
            }

            if (Time.time < entry.openedAt + closeDelay)
                continue;

            float doorX = entry.door.transform.position.x;
            float currentSign = Mathf.Sign(agentX - doorX);
            bool sideChanged = currentSign != 0f && currentSign != entry.sideSign;

            bool farEnough = Mathf.Abs(agentX - doorX) >= closeDistance;
            bool intersects = entry.doorCollider != null && agentBounds.Intersects(entry.doorCollider.bounds);

            if (sideChanged && farEnough && !intersects)
            {
                entry.door.SetOpen(false);
                _openedDoors.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        var col = _col != null ? _col : GetComponent<Collider2D>();
        Vector2 origin = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        float dir = transform.localScale.x >= 0f ? 1f : -1f;
        Gizmos.color = gizmoColor;
        Vector2 end = origin + new Vector2(dir * interactDistance, 0f);
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
