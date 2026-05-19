using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollapsingFloor : MonoBehaviour
{
    [SerializeField] private float _collapseDelay = 0.6f;
    [SerializeField] private float _shakeAmplitude = 0.06f;
    [SerializeField] private float _respawnDelay = 0f;  // 0 = не восстанавливается

    private Collider2D _collider;
    private Vector3 _originPos;
    private bool _triggered;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _originPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_triggered) return;
        if (!col.gameObject.CompareTag("Player")) return;
        if (!IsLandedOnTop(col)) return;

        _triggered = true;
        StartCoroutine(CollapseRoutine());
    }

    bool IsLandedOnTop(Collision2D col)
    {
        float topY = _collider.bounds.max.y;
        foreach (var contact in col.contacts)
        {
            if (contact.point.y >= topY - 0.15f)
                return true;
        }
        return false;
    }

    private IEnumerator CollapseRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _collapseDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _collapseDelay;
            float shake = Mathf.Sin(elapsed * 45f) * _shakeAmplitude * (1f - t);
            transform.position = _originPos + Vector3.right * shake;
            yield return null;
        }

        transform.position = _originPos;
        _collider.enabled = false;

        if (_respawnDelay > 0f)
        {
            yield return new WaitForSeconds(_respawnDelay);
            transform.position = _originPos;
            _collider.enabled = true;
            _triggered = false;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.8f, 0.35f, 0f, 0.35f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
