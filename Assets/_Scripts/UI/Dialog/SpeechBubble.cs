using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private GameObject _bubbleRoot;
    [SerializeField] private Text _text;
    [SerializeField] private float _typingSpeed = 0.04f;
    [SerializeField] private float _defaultDuration = 3f;
    [SerializeField] private string _sortingLayerName = "UI";
    [SerializeField] private int _sortingOrder = 100;

    private Coroutine _routine;

    private void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.sortingLayerName = _sortingLayerName;
            canvas.sortingOrder = _sortingOrder;
        }
        _bubbleRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        // Prevent bubble from flipping when character faces left (scale.x < 0)
        var s = _bubbleRoot.transform.localScale;
        if (s.x < 0f)
        {
            s.x = -s.x;
            _bubbleRoot.transform.localScale = s;
        }
    }

    public void Say(string text, float duration = -1f)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(new[] { text }, duration < 0f ? _defaultDuration : duration));
    }

    public void SaySequence(MonologueData data)
    {
        if (data == null) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(data.lines, data.durationPerLine));
    }

    public void Hide()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        _bubbleRoot.SetActive(false);
    }

    private IEnumerator ShowRoutine(string[] lines, float duration)
    {
        _bubbleRoot.SetActive(true);

        foreach (var line in lines)
        {
            _text.text = "";
            foreach (char c in line)
            {
                _text.text += c;
                yield return new WaitForSeconds(_typingSpeed);
            }
            yield return new WaitForSeconds(duration);
        }

        _bubbleRoot.SetActive(false);
        _routine = null;
    }
}
