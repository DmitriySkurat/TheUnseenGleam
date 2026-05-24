using UnityEngine;

/// <summary>
/// Завершает катсцену и переходит в указанную сцену.
/// Можно вызвать через UnityEvent, Animation Event или OnTriggerEnter2D.
/// </summary>
public class CutsceneCompleteTrigger : MonoBehaviour
{
    [SerializeField] private string _nextScene = SceneNames.PrisonBeginning;
    [SerializeField] private string _playerTag = "Player";

    private bool _triggered;

    public void Complete()
    {
        if (_triggered) return;
        _triggered = true;
        Services.Get<SceneTransitionManager>().TransitionTo(_nextScene);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;
        Complete();
    }
}
