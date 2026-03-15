using UnityEngine;

public class AgentVision : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Vision Settings")]
    [Range(0f, 360f)]
    [SerializeField] private float _viewAngle = 90f;

    [SerializeField] private float _viewDistance = 5f;

    [SerializeField] private Color _gizmoColor = Color.yellow;
    
    public void Initialize()
    {
        
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawVision();
    }

    void OnDrawGizmosSelected()
    {
        DrawVision();
    }

    void DrawVision()
    {
        Gizmos.color = _gizmoColor;

        Vector3 pos = transform.position;

        float halfAngle = _viewAngle * 0.5f;

        Vector3 forward = transform.localScale.x >= 0 ? Vector3.right : Vector3.left;

        Vector3 leftDir = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(halfAngle, Vector3.forward) * forward;

        Gizmos.DrawLine(pos, pos + leftDir * _viewDistance);
        Gizmos.DrawLine(pos, pos + rightDir * _viewDistance);

        int segments = 30;
        Vector3 prevPoint = pos + leftDir * _viewDistance;

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * forward;
            Vector3 point = pos + dir * _viewDistance;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
#endif
}