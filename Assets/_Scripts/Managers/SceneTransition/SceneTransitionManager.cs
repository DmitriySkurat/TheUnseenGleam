using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using EntryPoint;

/// <summary>
/// Управляет переходом между сценами через аддитивную загрузку.
/// Новая сцена предзагружается в фоне, старая выгружается после активации новой.
/// Регистрируется как глобальный сервис в GameServiceRegistry.
/// </summary>
public class SceneTransitionManager : MonoBehaviour, IService
{
    public Task InitializeAsync() => Task.CompletedTask;

    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    /// <summary>
    /// Запускает переход в указанную сцену.
    /// Повторный вызов во время перехода игнорируется.
    /// </summary>
    public void TransitionTo(string targetSceneName)
    {
        //Utility.SceneLoader.Load(targetSceneName);
        if (_isTransitioning) return;
        StartCoroutine(TransitionCoroutine(targetSceneName));
    }

    private IEnumerator TransitionCoroutine(string targetSceneName)
    {
        _isTransitioning = true;

        Scene currentScene = SceneManager.GetActiveScene();

        // Начинаем аддитивную загрузку новой сцены в фоне (без немедленной активации)
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        // Ждём полной предзагрузки: прогресс 0.9 означает, что сцена в памяти,
        // но Awake ещё не вызван — игрок продолжает играть в старой сцене
        while (loadOp.progress < 0.9f)
            yield return null;

        // Записываем на диск, если выбран активный слот сохранения
        if (Services.IsRegistered<PlayerContext>() && SaveVariables.ActiveSlot >= 0)
        {
            var ctx = Services.Get<PlayerContext>();
            SaveManager.Save(SaveVariables.ActiveSlot, ctx.health.CurrentHealth, ctx.stamina, targetSceneName);
        }

        // Явно Dispose'им ISceneLifecycle старой игровой сцены ДО активации новой.
        // Это гарантирует корректный порядок: ISceneLifecycle → ISessionLifecycle,
        // а OnDestroy старого GameplayEntryPoint станет безопасным no-op (флаг _isDisposed).
        if (Services.IsRegistered<GameplayEntryPoint>())
        {
            Services.Get<GameplayEntryPoint>().DisposeObjects();
            Services.Unregister<GameplayEntryPoint>();
        }

        // Выгружаем сервисы старой сцены ДО активации новой,
        // чтобы новый GameplaySceneServiceRegistry мог зарегистрировать их без конфликта
        var oldRegistry = FindObjectOfType<GameplaySceneServiceRegistry>();
        oldRegistry?.Dispose();

        // Активируем новую сцену — запускаются Awake и Bootstrap нового EntryPoint
        loadOp.allowSceneActivation = true;

        // Даём кадр на инициализацию нового EntryPoint
        yield return null;

        // Устанавливаем новую сцену активной (освещение, physics, audio listener)
        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        // Выгружаем старую сцену (OnDestroy у её объектов вызовется здесь)
        yield return SceneManager.UnloadSceneAsync(currentScene);

        _isTransitioning = false;
    }
}
