using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Меню Continue: показывает 3 слота.
/// Если слот занят — загружает сохранение и переходит в сохранённую сцену.
/// Если слот пуст — ничего не делает.
/// </summary>
public class SavesMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Header("Button Images")]
    [SerializeField] private Image slot1Image;
    [SerializeField] private Image slot2Image;
    [SerializeField] private Image slot3Image;

    [Header("Button Images")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite save1Sprite;
    [SerializeField] private Sprite save2Sprite;
    [SerializeField] private Sprite save3Sprite;

    [Header("Navigation")]
    public GameObject menuButtonsParent;

    private InputManager _inputManager;
    private SceneTransitionManager _sceneTransition;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        _sceneTransition = Services.Get<SceneTransitionManager>();

        _inputManager.OnEscape += HandleEscape;

        RefreshSlotImages();
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

    private void RefreshSlotImages()
    {
        SetSlotImage(slot1Image, 1, save1Sprite);
        SetSlotImage(slot2Image, 2, save2Sprite);
        SetSlotImage(slot3Image, 3, save3Sprite);
    }

    private void SetSlotImage(Image image, int slot, Sprite saveSprite)
    {
        if (image == null) return;
        image.sprite = SaveManager.Exists(slot) ? saveSprite : emptySprite;
    }

    public void OnSlot1() => TryLoadSlot(1);
    public void OnSlot2() => TryLoadSlot(2);
    public void OnSlot3() => TryLoadSlot(3);

    private void TryLoadSlot(int slot)
    {
        var data = SaveManager.Load(slot);
        if (data == null)
            return;

        SaveVariables.ActiveSlot = slot;
        Services.Get<PlayerPersistentState>().Save(data.health, data.stamina);

        Utility.SceneLoader.Load(data.sceneName);

        //_sceneTransition.TransitionTo(data.sceneName);
    }
}
