using UnityEngine;

public class DroppedItemPickup : Interactable
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private string interactableLayerName = "Interactable";
    [SerializeField] private float destroyAfter = 60f;
    [SerializeField] private float minNoiseRadius = 0.5f;
    [SerializeField] private float maxNoiseRadius = 3f;
    [SerializeField] private float minFallHeight = 0.5f;
    [SerializeField] private float maxFallHeight = 5f;
    [SerializeField, Range(0f, 0.5f)] private float noiseVariance = 0.1f;

    private ItemData _item;
    private int _count;
    private NoiseSystem _noiseSystem;
    private bool _landed;
    private float _peakY;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null)
            _sr = GetComponentInChildren<SpriteRenderer>();
        _defaultColor = _sr != null ? _sr.color : Color.white;
        _rb = GetComponent<Rigidbody2D>();
        gameObject.layer = LayerMask.NameToLayer(interactableLayerName);
        _peakY = transform.position.y;
    }

    void Update()
    {
        if (!_landed)
            _peakY = Mathf.Max(_peakY, transform.position.y);
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
        {
            _sr.sprite = item.sprite;
            FitColliderToSprite(item.sprite);
        }
    }

    private void FitColliderToSprite(Sprite sprite)
    {
        var poly = GetComponent<PolygonCollider2D>();
        if (poly != null && sprite.GetPhysicsShapeCount() > 0)
        {
            var points = new System.Collections.Generic.List<Vector2>();
            poly.pathCount = sprite.GetPhysicsShapeCount();
            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
            {
                sprite.GetPhysicsShape(i, points);
                poly.SetPath(i, points);
            }
            return;
        }

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = sprite.bounds.size;
            box.offset = sprite.bounds.center;
        }
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

        float fallHeight = Mathf.Max(0f, _peakY - pos.y);
        float radius = EvaluateNoiseRadius(fallHeight);
        _noiseSystem?.EmitNoise(pos, radius, gameObject, NoiseType.ObjectImpact, noiseVariance);
    }

    private float EvaluateNoiseRadius(float fallHeight)
    {
        if (fallHeight < minFallHeight) return minNoiseRadius;
        float maxH = Mathf.Max(minFallHeight + 0.01f, maxFallHeight);
        float t = Mathf.InverseLerp(minFallHeight, maxH, fallHeight);
        return Mathf.Lerp(minNoiseRadius, maxNoiseRadius, t);
    }

    private void SelfDestroy() => Destroy(gameObject);
}
