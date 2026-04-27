using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StumbleTrigger : MonoBehaviour
{
    [Tooltip("Триггер срабатывает только один раз, после чего объект отключается")]
    [SerializeField] private bool _singleUse = true;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Services.IsRegistered<PlayerContext>()) return;

        var ctx = Services.Get<PlayerContext>();
        if (ctx == null || !ctx.isAlive || ctx.isGrabbed) return;

        ctx.stumblePending = true;

        if (_singleUse)
            gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
