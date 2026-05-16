using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class RockActivatable : RockActivatableObject
{
    [Header("Sprites")]
    [SerializeField] private Sprite deactivatedSprite;
    [SerializeField] private Sprite activatedSprite;

    private SpriteRenderer _sr;
    private BoxCollider2D _col;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<BoxCollider2D>();

        onActivated.AddListener(OnActivatedVisuals);
        onDeactivated.AddListener(OnDeactivatedVisuals);

        ApplySprite(deactivatedSprite);
    }

    private void OnActivatedVisuals() => ApplySprite(activatedSprite);
    private void OnDeactivatedVisuals() => ApplySprite(deactivatedSprite);

    private void ApplySprite(Sprite sprite)
    {
        if (sprite == null) return;

        _sr.sprite = sprite;
        _col.size = sprite.bounds.size;
        _col.offset = sprite.bounds.center;
    }
}
