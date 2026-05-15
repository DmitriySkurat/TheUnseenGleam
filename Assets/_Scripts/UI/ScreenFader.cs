using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour, IService
{
    [SerializeField] private float _defaultDuration = 0.4f;

    private Image _overlay;
    private GameObject _canvasGO;
    private Coroutine _fadeCoroutine;

    public Task InitializeAsync()
    {
        BuildOverlay();
        SceneManager.sceneLoaded += OnSceneLoaded;
        return Task.CompletedTask;
    }

    private void BuildOverlay()
    {
        _canvasGO = new GameObject("ScreenFadeCanvas");
        DontDestroyOnLoad(_canvasGO);

        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        _canvasGO.AddComponent<CanvasScaler>();
        _canvasGO.AddComponent<GraphicRaycaster>();

        var imageGO = new GameObject("FadeOverlay");
        imageGO.transform.SetParent(_canvasGO.transform, false);

        _overlay = imageGO.AddComponent<Image>();
        _overlay.color = new Color(0f, 0f, 0f, 0f);

        var rt = _overlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_overlay != null && _overlay.color.a > 0f)
            FadeIn();
    }

    public void FadeIn(float duration = -1f)
    {
        float d = duration < 0f ? _defaultDuration : duration;
        SwapCoroutine(FadeCoroutine(1f, 0f, d, null));
    }

    public void FadeOut(float duration = -1f, Action onComplete = null)
    {
        float d = duration < 0f ? _defaultDuration : duration;
        SwapCoroutine(FadeCoroutine(0f, 1f, d, onComplete));
    }

    public void SetBlack()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        SetAlpha(1f);
    }

    public void FadeOutAndLoad(string sceneName, float duration = -1f)
    {
        if (_overlay != null && _overlay.color.a >= 1f)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }
        FadeOut(duration, () => SceneManager.LoadScene(sceneName));
    }

    private void SwapCoroutine(IEnumerator routine)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(routine);
    }

    private IEnumerator FadeCoroutine(float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAlpha(to);
        onComplete?.Invoke();
    }

    private void SetAlpha(float a)
    {
        if (_overlay == null) return;
        var c = _overlay.color;
        c.a = a;
        _overlay.color = c;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_canvasGO != null)
            Destroy(_canvasGO);
    }
}
