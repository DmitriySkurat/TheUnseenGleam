using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    public static bool isPaused = false;
    public GameObject pauseGameMenu;
    public GameObject optionsMenu;

    [SerializeField] private CanvasGroup _backgroundOverlay;
    [SerializeField] private float _fadeDuration = 0.15f;

    private OptionsController optionsController;
    private InputManager _inputManager;
    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private Coroutine _overlayCoroutine;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();

        if (optionsMenu)
            optionsController = optionsMenu.GetComponent<OptionsController>();

        if (pauseGameMenu)
        {
            _canvasGroup = pauseGameMenu.GetComponent<CanvasGroup>();
            pauseGameMenu.SetActive(false);
        }

        if (_backgroundOverlay)
        {
            _backgroundOverlay.alpha = 0f;
            _backgroundOverlay.gameObject.SetActive(false);
        }

        isPaused = false;

        _inputManager.OnEscape += HandleEscapeGameplay;
        _inputManager.OnCloseWindow += HandleEscapeUI;
    }

    public void Dispose()
    {
        Time.timeScale = 1f;
        _inputManager.OnEscape -= HandleEscapeGameplay;
        _inputManager.OnCloseWindow -= HandleEscapeUI;
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
        {
            _inputManager.OnEscape -= HandleEscapeGameplay;
            _inputManager.OnCloseWindow -= HandleEscapeUI;
        }
    }

    private void HandleEscapeGameplay()
    {
        if (Services.IsRegistered<ScreenFader>() && Services.Get<ScreenFader>().IsTransitioning)
            return;
        Pause();
    }

    private void HandleEscapeUI()
    {
        if (optionsMenu && optionsMenu.activeSelf)
        {
            if (optionsController)
                optionsController.CloseSettings();
            else
                optionsMenu.SetActive(false);
            return;
        }

        Resume();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        _inputManager.EnableGameplay();
        StartFade(0f, () => pauseGameMenu.SetActive(false));
        if (_backgroundOverlay != null)
            StartOverlayFade(0f, () => _backgroundOverlay.gameObject.SetActive(false));
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        _inputManager.EnableUI();
        pauseGameMenu.SetActive(true);
        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.gameObject.SetActive(true);
            StartOverlayFade(1f, null);
        }
        StartFade(1f, null);
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Utility.SceneLoader.Load(SceneNames.Menu);
    }

    private void StartOverlayFade(float targetAlpha, System.Action onComplete)
    {
        if (_overlayCoroutine != null) StopCoroutine(_overlayCoroutine);
        _overlayCoroutine = StartCoroutine(FadeCanvasGroup(_backgroundOverlay, targetAlpha, onComplete));
    }

    private void StartFade(float targetAlpha, System.Action onComplete)
    {
        if (_canvasGroup == null) { onComplete?.Invoke(); return; }
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeCanvasGroup(_canvasGroup, targetAlpha, onComplete));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, System.Action onComplete)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
            yield return null;
        }

        group.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
