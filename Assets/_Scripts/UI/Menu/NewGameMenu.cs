using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Меню NewGame: показывает 3 слота.
/// Пустой слот — создать новое сохранение.
/// Занятый слот — показать панель подтверждения перезаписи.
/// </summary>
public class NewGameMenu : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Header("Button Images")]
    [SerializeField] private Image slot1Image;
    [SerializeField] private Image slot2Image;
    [SerializeField] private Image slot3Image;

    [Header("Sprites")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite save1Sprite;
    [SerializeField] private Sprite save2Sprite;
    [SerializeField] private Sprite save3Sprite;

    [Header("Confirmation Panel")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Navigation")]
    public GameObject menuButtonsParent;

    private InputManager _inputManager;
    private SceneTransitionManager _sceneTransition;
    private int _pendingSlot;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        _sceneTransition = Services.Get<SceneTransitionManager>();

        _inputManager.OnCloseWindow += HandleCloseWindow;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        RefreshSlotImages();
    }

    public void Dispose()
    {
        _inputManager.OnCloseWindow -= HandleCloseWindow;
    }

    private void OnEnable()
    {
        SetMenuButtonsInteractable(false);
    }

    private void OnDisable()
    {
        SetMenuButtonsInteractable(true);
    }

    private void SetMenuButtonsInteractable(bool interactable)
    {
        if (menuButtonsParent == null) return;
        foreach (var btn in menuButtonsParent.GetComponentsInChildren<Button>())
            btn.interactable = interactable;
    }

    private void HandleCloseWindow()
    {
        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            OnConfirmNo();
            return;
        }

        gameObject.SetActive(false);

        // кнопки разблокируются через OnDisable
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

    public void OnSlot1() => TryStartNewGame(1);
    public void OnSlot2() => TryStartNewGame(2);
    public void OnSlot3() => TryStartNewGame(3);

    private void TryStartNewGame(int slot)
    {
        if (SaveManager.Exists(slot))
        {
            _pendingSlot = slot;
            ShowConfirmPanel(true);
        }
        else
        {
            StartNewGame(slot);
        }
    }

    public void OnConfirmYes()
    {
        ShowConfirmPanel(false);
        StartNewGame(_pendingSlot);
    }

    public void OnConfirmNo()
    {
        ShowConfirmPanel(false);
    }

    private void ShowConfirmPanel(bool show)
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(show);

        if (slotsPanel != null)
            slotsPanel.SetActive(!show);
    }

    private void StartNewGame(int slot)
    {
        if (SaveManager.Exists(slot))
            SaveManager.Delete(slot);

        // health=0 — признак «новой игры»; SaveLoader не будет читать эти значения.
        SaveManager.Save(slot, 0f, 0f, SceneNames.CutsceneVillage);

        SaveVariables.ActiveSlot = slot;
        SaveVariables.PendingSave = null;

        Utility.SceneLoader.Load(SceneNames.CutsceneVillage);
        // _sceneTransition.TransitionTo(SceneNames.Demo);
    }
}
