using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SortingOrderSetter : MonoBehaviour
{
    [SerializeField] SortingOrder _order;

    void Awake() => GetComponent<Renderer>().sortingOrder = (int)_order;

#if UNITY_EDITOR
    void OnValidate()
    {
        var r = GetComponent<Renderer>();
        if (r != null) r.sortingOrder = (int)_order;
    }
#endif
}
