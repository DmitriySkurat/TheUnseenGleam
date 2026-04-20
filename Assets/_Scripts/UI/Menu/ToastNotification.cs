using UnityEngine;
using System.Collections;

public class ToastNotification : MonoBehaviour
{
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeDuration = 0.3f;

    private CanvasGroup _canvasGroup;
    private Coroutine _hideCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _hideCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSecondsRealtime(_displayDuration);

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = 1f - elapsed / _fadeDuration;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
