using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using EntryPoint;

public class GrabbedDeathSequence : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [SerializeField] private Image fadeOverlay;
    [SerializeField] private DeathMenu deathMenu;

    [SerializeField] private float offScreenTimeout = 4f;
    [SerializeField] private float fadeDuration = 1f;

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

        if (fadeOverlay != null)
        {
            var c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.gameObject.SetActive(false);
        }
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

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                var c = fadeOverlay.color;
                c.a = Mathf.Clamp01(t / fadeDuration);
                fadeOverlay.color = c;
                yield return null;
            }
        }

        if (!string.IsNullOrEmpty(_nextScene))
            Utility.SceneLoader.Load(_nextScene);
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
