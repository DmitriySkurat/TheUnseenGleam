using UnityEngine;

/// <summary>
/// Place in a scene to make the player walk in automatically on load.
/// Input is fully blocked until the walk completes.
/// </summary>
public class SceneEntryWalker : MonoBehaviour, ISceneLifecycle
{
    public enum WalkDirection { Left = -1, Right = 1 }

    [SerializeField] private WalkDirection _direction = WalkDirection.Right;
    [SerializeField] private float _walkDistance = 3f;

    public InitializationOrder Order => (InitializationOrder)210;

    private PlayerContext _ctx;
    private float _startX;
    private bool _active;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _ctx = Services.Get<PlayerContext>();
        _startX = _ctx.transform.position.x;
        _ctx.sceneEntryMoveX = (float)_direction;
        _ctx.isSceneEntry = true;
        _active = true;
    }

    public void Dispose()
    {
        StopEntry();
    }

    void Update()
    {
        if (!_active || _ctx == null) return;

        float traveled = Mathf.Abs(_ctx.transform.position.x - _startX);
        if (traveled >= _walkDistance)
            StopEntry();
    }

    private void StopEntry()
    {
        _active = false;
        if (_ctx == null) return;
        _ctx.isSceneEntry = false;
        _ctx.sceneEntryMoveX = 0f;
    }
}
