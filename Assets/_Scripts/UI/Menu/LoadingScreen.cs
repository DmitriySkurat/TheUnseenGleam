using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _duration = 1.5f;

    private void Awake()
    {
        if (_fillImage != null)
            _fillImage.fillAmount = 0f;
    }

    public IEnumerator Play()
    {
        if (_fillImage == null)
        {
            Debug.LogError("[LoadingScreen] Fill Image не назначен в инспекторе!", this);
            yield break;
        }

        _fillImage.fillAmount = 0f;
        yield return null; // пропускаем кадр с большим deltaTime от инициализации

        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _fillImage.fillAmount = Mathf.Clamp01(elapsed / _duration);
            yield return null;
        }

        _fillImage.fillAmount = 1f;
    }
}
