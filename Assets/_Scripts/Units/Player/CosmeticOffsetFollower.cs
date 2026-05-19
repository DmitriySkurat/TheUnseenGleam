using UnityEngine;

public class CosmeticOffsetFollower : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player;

    Vector3 _baseLocalPos;
    PlayerContext _ctx;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _baseLocalPos = transform.localPosition;
        _ctx = Services.Get<PlayerContext>();
    }

    public void Dispose() { }

    void LateUpdate()
    {
        if (_ctx == null) return;
        transform.localPosition = _baseLocalPos + _ctx.cosmeticOffset;
    }
}
