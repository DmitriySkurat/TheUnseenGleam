using UnityEngine;

public class PlayerSpriteFlip : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;

    [SerializeField] private SpriteRenderer _sprite;

    private PlayerContext _ctx;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();
    }

    public void Dispose() { }

    void LateUpdate()
    {
        if (_ctx == null) return;

        if (_ctx.isLedgeGrabbing)
        {
            _sprite.flipX = !_ctx.ledgeFacingRight;
            return;
        }

        float dirX = _ctx.velocity.x;
        if (Mathf.Abs(dirX) > 0.01f)
            _sprite.flipX = dirX < 0f;
    }
}
