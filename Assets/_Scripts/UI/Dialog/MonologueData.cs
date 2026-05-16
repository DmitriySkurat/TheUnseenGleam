using UnityEngine;

[CreateAssetMenu(fileName = "MonologueData", menuName = "Dialog/Monologue Data")]
public class MonologueData : ScriptableObject
{
    [TextArea(1, 4)]
    public string[] lines;
    [Tooltip("Seconds each line stays visible after typing completes.")]
    public float durationPerLine = 3f;
}
