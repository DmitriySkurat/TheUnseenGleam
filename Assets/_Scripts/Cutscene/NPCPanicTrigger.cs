using UnityEngine;

/// <summary>
/// Place on an invisible trigger collider in the scene.
/// When any ScriptedAgent enters it, all WanderingNPCs start fleeing.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NPCPanicTrigger : MonoBehaviour
{
    [SerializeField] private bool _singleUse = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ScriptedAgent>() == null) return;

        var npcs = Object.FindObjectsByType<WanderingNPC>(FindObjectsSortMode.None);
        Vector2 threatPos = other.transform.position;
        foreach (var npc in npcs)
            npc.StartFleeing(threatPos);

        if (_singleUse)
            gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.35f);
        var col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
#endif
}
