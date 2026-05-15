using System.Collections;
using UnityEngine;
using EntryPoint;

public class GrabbedDeathSequence : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private DeathMenu deathMenu;

    [SerializeField] private float offScreenTimeout = 4f;

    [Tooltip("Если задано — после затемнения загружается эта сцена вместо экрана смерти")]
    [SerializeField] private string _nextScene;

    private PlayerHealth _playerHealth;
    private PlayerContext _playerCtx;
    private Camera _camera;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _playerCtx = Services.Get<PlayerContext>();
        _playerHealth = _playerCtx.health;
        _playerHealth.OnDied += OnPlayerDied;

        _camera = Camera.main;
    }

    public void Dispose()
    {
        if (_playerHealth != null)
            _playerHealth.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (_playerCtx != null && _playerCtx.diedWhileGrabbed)
            StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (_playerCtx.cameraFollow != null)
            _playerCtx.cameraFollow.Freeze();

        float elapsed = 0f;
        while (elapsed < offScreenTimeout)
        {
            elapsed += Time.deltaTime;
            if (IsPlayerOffScreen())
                break;
            yield return null;
        }

        if (Services.IsRegistered<ScreenFader>())
        {
            bool done = false;
            Services.Get<ScreenFader>().FadeOut(onComplete: () => done = true);
            while (!done) yield return null;
        }

        if (!string.IsNullOrEmpty(_nextScene))
            UnityEngine.SceneManagement.SceneManager.LoadScene(_nextScene);
        else if (deathMenu != null)
            deathMenu.ShowNow();
    }

    private bool IsPlayerOffScreen()
    {
        if (_camera == null || _playerCtx?.transform == null)
            return false;

        Vector3 vp = _camera.WorldToViewportPoint(_playerCtx.transform.position);
        return vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f;
    }
}
