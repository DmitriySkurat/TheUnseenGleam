using UnityEngine;

public class SavesMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;


    public GameObject menuButtonsParent;


    private InputManager _inputManager;
    
    
    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        
        _inputManager.OnEscape += HandleEscape;
    }
    
    public void Dispose()
    {
        
    }
    
    private void HandleEscape()
    {
        // Закрываем меню сохранений при нажатии Escape
        gameObject.SetActive(false);
        
        if (menuButtonsParent != null)
            menuButtonsParent.SetActive(true);
    }
    
    
    public void OnSave1()
    {
        
    }
    
    public void OnSave2()
    {
        
    }
    
    public void OnSave3()
    {
        
    }
}