using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;
    
    public static bool isPaused = false;
    public GameObject pauseGameMenu;
    public GameObject optionsMenu;
    private OptionsController optionsController;
    
    private InputManager _inputManager;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        
        if (optionsMenu)
            optionsController = optionsMenu.GetComponent<OptionsController>();

        // Reset pause state on scene start
        isPaused = false;

        if (pauseGameMenu)
            pauseGameMenu.SetActive(false);
            
        _inputManager.OnEscape += HandleEscapeGameplay;
        _inputManager.OnCloseWindow += HandleEscapeUI;
    }

    public void Dispose()
    {
        // Ensure time scale is reset when the scene is unloaded
        Time.timeScale = 1f;

        _inputManager.OnEscape -= HandleEscapeGameplay;
        _inputManager.OnCloseWindow -= HandleEscapeUI;
    }

    private void HandleEscapeGameplay()
    {
        Pause();
    }

    private void HandleEscapeUI()
    {
        if (optionsMenu && optionsMenu.activeSelf)
            return;

        Resume();
    }

    public void Resume()
    {
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        _inputManager.EnableGameplay();
    }

    public void Pause()
    {
        pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        _inputManager.EnableUI();
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Utility.SceneLoader.Load(SceneNames.Menu);
    }
}