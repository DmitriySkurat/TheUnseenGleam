using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Плавно переводит освещение катсцены из дня в ночь и фиксирует ночное состояние.
/// Запуск: автоматически через Start или вручную через Activate().
/// </summary>
public class CutsceneDayToNightController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.PostProcessing;

    [Header("Spotlight")]
    [Tooltip("Отдельный spotlight сцены, имитирующий дневной свет")]
    [SerializeField] private Light2D _spotlight;
    [Tooltip("Целевая интенсивность spotlight ночью")]
    [SerializeField, Range(0f, 2f)] private float _nightSpotIntensity = 0f;

    [Header("Global Light")]
    [Tooltip("Целевая интенсивность глобального света ночью")]
    [SerializeField, Range(0f, 2f)] private float _nightGlobalIntensity = 0.08f;
    [Tooltip("Цвет глобального света ночью")]
    [SerializeField] private Color _nightGlobalColor = new Color(0.15f, 0.2f, 0.35f);

    [Header("Transition")]
    [Tooltip("Продолжительность перехода в секундах")]
    [SerializeField] private float _duration = 90f;

    [Header("Activation")]
    [SerializeField] private bool _activateOnStart = true;

    private LightSystem _lightSystem;
    private bool _activated;
    private bool _nightLocked;

    public void Initialize()
    {
        _lightSystem = Services.Get<LightSystem>();
    }

    public void Dispose() { }

    private void Start()
    {
        if (_activateOnStart)
            Activate();
    }

    // LateUpdate выполняется после всех Update — перезаписывает любые контроллеры освещения.
    private void LateUpdate()
    {
        if (!_nightLocked) return;
        _lightSystem.UpdateGlobalLight(_nightGlobalIntensity, _nightGlobalColor);
        if (_spotlight != null)
            _spotlight.intensity = _nightSpotIntensity;
    }

    public void Activate()
    {
        if (_activated) return;
        _activated = true;
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        float startGlobalIntensity = _lightSystem.GlobalLightIntensity;
        Color startGlobalColor     = _lightSystem.GlobalLightColor;
        float startSpotIntensity   = _spotlight != null ? _spotlight.intensity : 0f;

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);

            float globalIntensity = Mathf.Lerp(startGlobalIntensity, _nightGlobalIntensity, t);
            Color globalColor     = Color.Lerp(startGlobalColor, _nightGlobalColor, t);
            _lightSystem.UpdateGlobalLight(globalIntensity, globalColor);

            if (_spotlight != null)
                _spotlight.intensity = Mathf.Lerp(startSpotIntensity, _nightSpotIntensity, t);

            yield return null;
        }

        _nightLocked = true;
    }
}
