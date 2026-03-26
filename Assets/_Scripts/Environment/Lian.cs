using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Vines : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}
