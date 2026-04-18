using UnityEngine;

/// <summary>
/// Триггер-зона перехода между сценами.
/// Разместить на GameObject с Collider2D (Is Trigger = true).
/// При входе игрока запускает аддитивный переход через SceneTransitionManager.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string _targetSceneName;
    [SerializeField] private string _playerTag = "Player";

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag(_playerTag)) return;

        _triggered = true;
        Services.Get<SceneTransitionManager>().TransitionTo(_targetSceneName);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        var col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            $"→ {_targetSceneName}");
    }
#endif
}
