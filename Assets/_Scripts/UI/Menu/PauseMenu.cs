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
        Time.timeScale = 1f;
        if (pauseGameMenu)
            pauseGameMenu.SetActive(false);
            
        _inputManager.OnEscape += HandleEscape;
    }
    
    public void Dispose()
    {
        // Ensure time scale is reset when the scene is unloaded
        Time.timeScale = 1f;
        
        _inputManager.OnEscape -= HandleEscape;
    }

    private void HandleEscape()
    {
        if (optionsMenu && optionsMenu.activeSelf)
        {
            if (optionsController)
                optionsController.CloseSettings();
            else
                optionsMenu.SetActive(false);
            return;
        }
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
        
    }

    public void Resume()
    {
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Utility.SceneLoader.Load("Menu");
    }
}