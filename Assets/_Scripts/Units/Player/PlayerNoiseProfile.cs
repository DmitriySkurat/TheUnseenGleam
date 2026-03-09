using UnityEngine;

[CreateAssetMenu(menuName = "Player/Noise Profile")]
public class PlayerNoiseProfile : ScriptableObject
{
    [Min(0f)] public float walkRadius = 2f;
    [Min(0f)] public float runRadius = 4f;
    [Min(0f)] public float crouchRadius = 1f;
    [Min(0f)] public float jumpRadius = 3f;
}
