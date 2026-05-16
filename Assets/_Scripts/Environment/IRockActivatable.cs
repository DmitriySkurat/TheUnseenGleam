using UnityEngine;

public interface IRockActivatable
{
    void OnHitByRock(Vector2 hitPoint, GameObject rockSource);
}
