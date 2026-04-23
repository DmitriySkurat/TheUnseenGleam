using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class SortingOrderSetter : MonoBehaviour
{
    [SerializeField] SortingOrder _order;

    SortingGroup _group;
    Renderer     _renderer;
    int          _defaultOrder;

    void Awake()
    {
        _group        = GetComponentInParent<SortingGroup>();
        _renderer     = GetComponentInParent<Renderer>();
        _defaultOrder = (int)_order;
        Apply(_defaultOrder);
    }

    public void SetOrder(int order) => Apply(order);
    public void ResetOrder()        => Apply(_defaultOrder);

    void Apply(int order)
    {
        if (_group    != null) { _group.sortingOrder    = order; return; }
        if (_renderer != null)   _renderer.sortingOrder = order;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var group    = GetComponentInParent<SortingGroup>();
        var renderer = GetComponentInParent<Renderer>();
        int order    = (int)_order;
        if (group    != null) { group.sortingOrder    = order; return; }
        if (renderer != null)   renderer.sortingOrder = order;
    }
#endif
}
