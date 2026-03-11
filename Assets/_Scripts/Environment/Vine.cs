using UnityEngine;

// Класс для лиан - реализация как в лестницах, только без снапа к центру
// + добавить немного гравитации, чтобы игрок постепенно падал с лиан 

[RequireComponent(typeof(Collider2D))]
public class Vine : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}