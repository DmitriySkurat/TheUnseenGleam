using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonologueZoneTrigger : MonoBehaviour
{
    [SerializeField] private SpeechBubble _target;
    [SerializeField] private MonologueData _data;
    [SerializeField] private bool _triggerOnce = true;

    private bool _triggered;

    private void Awake() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _triggered) return;
        if (!other.CompareTag("Player")) return;
        if (_data == null || _target == null) return;

        _triggered = true;
        _target.SaySequence(_data);
    }
}
