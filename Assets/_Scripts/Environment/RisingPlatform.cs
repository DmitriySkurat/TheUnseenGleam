using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class RisingPlatform : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float riseDistance = 3f;
    [SerializeField] private float lowerDistance = 3f;
    [SerializeField] private float riseDuration = 1.5f;
    [SerializeField] private float lowerDuration = 1.5f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Auto-return")]
    [SerializeField] private bool returnAfterRise = false;
    [SerializeField] private float holdDuration = 2f;

    [Header("Chain")]
    [SerializeField] private SpriteRenderer chainRendererLeft;
    [SerializeField] private SpriteRenderer chainRendererRight;

    private float _platformHalfHeight;

    [Header("Events")]
    public UnityEvent onRiseComplete;
    public UnityEvent onLowerComplete;

    private Rigidbody2D _rb;
    private Vector2 _startPosition;
    private Vector2 _raisedPosition;
    private Vector2 _loweredPosition;
    private bool _isRaised;
    private Coroutine _moveCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        _platformHalfHeight = col != null ? col.bounds.extents.y : 0f;

        _startPosition = _rb.position;
        _raisedPosition = _startPosition + Vector2.up * riseDistance;
        _loweredPosition = _startPosition - Vector2.up * lowerDistance;

        UpdateChain(_startPosition);
    }

    public void Rise()
    {
        RestartMove(MoveRoutine(_raisedPosition, riseDuration, rising: true));
    }

    public void Lower()
    {
        RestartMove(MoveRoutine(_loweredPosition, lowerDuration, rising: false));
    }

    public void Toggle()
    {
        if (_isRaised) Lower();
        else Rise();
    }

    public void SetRaised(bool raised)
    {
        if (raised) Rise();
        else ReturnToStart();
    }

    public void SetLowered(bool lowered)
    {
        if (lowered) Lower();
        else ReturnToStart();
    }

    public void ReturnToStart()
    {
        RestartMove(MoveRoutine(_startPosition, lowerDuration, rising: false));
    }

    private void RestartMove(IEnumerator routine)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(routine);
    }

    private IEnumerator MoveRoutine(Vector2 target, float duration, bool rising)
    {
        Vector2 from = _rb.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            Vector2 pos = Vector2.LerpUnclamped(from, target, t);
            _rb.MovePosition(pos);
            UpdateChain(pos);
            yield return new WaitForFixedUpdate();
        }

        _rb.MovePosition(target);
        UpdateChain(target);
        _isRaised = rising;

        if (rising)
        {
            onRiseComplete?.Invoke();
            if (returnAfterRise)
            {
                if (holdDuration > 0f)
                    yield return new WaitForSeconds(holdDuration);
                Lower();
            }
        }
        else
        {
            onLowerComplete?.Invoke();
        }
    }

    private void UpdateChain(Vector2 platformPos)
    {
        UpdateSingleChain(chainRendererLeft, platformPos);
        UpdateSingleChain(chainRendererRight, platformPos);
    }

    private void UpdateSingleChain(SpriteRenderer chain, Vector2 platformPos)
    {
        if (chain == null) return;
        float anchorY = chain.transform.position.y;
        float chainLength = Mathf.Max(0f, anchorY - (platformPos.y + _platformHalfHeight));
        chain.size = new Vector2(chain.size.x, chainLength);
    }
}
