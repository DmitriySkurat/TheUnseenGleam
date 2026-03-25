using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lian : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}
