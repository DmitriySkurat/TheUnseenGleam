using UnityEngine;

public class NewGameMenu : MonoBehaviour, ISceneLifecycle
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
        _inputManager.OnEscape -= HandleEscape;
    }
    
    private void HandleEscape()
    {
        gameObject.SetActive(false);
        
        if (menuButtonsParent != null)
            menuButtonsParent.SetActive(true);
    }
    
    public void OnSlot1()
    {
        
    }
    
    public void OnSlot2()
    {
        
    }
    
    public void OnSlot3()
    {
        
    }
}