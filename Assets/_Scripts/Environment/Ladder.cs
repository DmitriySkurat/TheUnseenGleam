using UnityEngine;

// добавить снап к центру лестницы
[RequireComponent(typeof(Collider2D))]
public class Ladder : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}