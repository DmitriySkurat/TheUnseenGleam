using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StumbleTrigger : MonoBehaviour
{
    [SerializeField] private bool _singleUse = true;
    [SerializeField] private float _delay = 2f;

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

        if (_singleUse) GetComponent<Collider2D>().enabled = false;

        StartCoroutine(TriggerAfterDelay(ctx));
    }

    private IEnumerator TriggerAfterDelay(PlayerContext ctx)
    {
        yield return new WaitForSeconds(_delay);

        while (!ctx.grounded)
            yield return null;

        while (Mathf.Abs(ctx.velocity.x) < 0.1f)
        {
            if (!ctx.isAlive || ctx.isGrabbed) yield break;
            yield return null;
        }

        if (ctx.isAlive && !ctx.isGrabbed)
            ctx.stumblePending = true;
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
