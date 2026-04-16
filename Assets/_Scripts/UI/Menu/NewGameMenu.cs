using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Меню NewGame: показывает 3 слота.
/// Пустой слот — создать новое сохранение.
/// Занятый слот — перезаписать сохранение и начать заново.
/// </summary>
public class NewGameMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Header("Кнопки слотов (Image на каждой кнопке)")]
    [SerializeField] private Image slot1Image;
    [SerializeField] private Image slot2Image;
    [SerializeField] private Image slot3Image;

    [Header("Спрайты")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite save1Sprite;
    [SerializeField] private Sprite save2Sprite;
    [SerializeField] private Sprite save3Sprite;

    [Header("Навигация")]
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

    public void OnSlot1() => StartNewGame(1);
    public void OnSlot2() => StartNewGame(2);
    public void OnSlot3() => StartNewGame(3);

    private void StartNewGame(int slot)
    {
        // Перезаписываем старый слот (если был)
        if (SaveManager.Exists(slot))
            SaveManager.Delete(slot);

        // Создаём заглушку, чтобы слот сразу отображался как занятый.
        // health=0 — признак «новой игры»; SaveLoader не будет читать эти значения.
        SaveManager.Save(slot, 0f, 0f, Utility.SceneNames.Demo);

        SaveVariables.ActiveSlot = slot;

        // PlayerPersistentState не трогаем — PlayerHealth возьмёт значения по умолчанию из ScriptableStats
        _sceneTransition.TransitionTo(Utility.SceneNames.Demo);
    }
}
