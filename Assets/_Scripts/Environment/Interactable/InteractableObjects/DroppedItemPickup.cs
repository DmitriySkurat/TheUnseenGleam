using UnityEngine;

public class DroppedItemPickup : Interactable
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private string interactableLayerName = "Interactable";
    [SerializeField] private float destroyAfter = 60f;
    [SerializeField] private float noiseRadius = 1.5f;
    [SerializeField, Range(0f, 0.5f)] private float noiseVariance = 0.1f;

    private ItemData _item;
    private int _count;
    private NoiseSystem _noiseSystem;
    private bool _landed;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null)
            _sr = GetComponentInChildren<SpriteRenderer>();
        _defaultColor = _sr != null ? _sr.color : Color.white;
        _rb = GetComponent<Rigidbody2D>();
        gameObject.layer = LayerMask.NameToLayer(interactableLayerName);
    }

    void Start()
    {
        _noiseSystem = Services.Get<NoiseSystem>();
        if (destroyAfter > 0f)
            Invoke(nameof(SelfDestroy), destroyAfter);
    }

    // ISceneLifecycle — not used for dynamically spawned objects (Awake/Start handle init)
    public override void Initialize() { }
    public override void Dispose() { }

    public void Setup(ItemData item, int count)
    {
        _item = item;
        _count = count;
        if (_sr != null && item?.sprite != null)
            _sr.sprite = item.sprite;
    }

    public override void OnInteract(Interactor interactor)
    {
        if (!(interactor is PlayerInteractor player)) return;
        var inventory = player.Context?.inventory;
        if (inventory == null || _item == null) return;
        inventory.Add(_item, _count);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_landed) return;
        if ((groundMask.value & (1 << collision.gameObject.layer)) == 0) return;
        _landed = true;
        Vector3 pos = collision.contactCount > 0
            ? (Vector3)collision.GetContact(0).point
            : transform.position;
        _noiseSystem?.EmitNoise(pos, noiseRadius, gameObject, NoiseType.ObjectImpact, noiseVariance);
    }

    private void SelfDestroy() => Destroy(gameObject);
}
