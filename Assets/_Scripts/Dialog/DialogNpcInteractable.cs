using UnityEngine;

public class DialogNpcInteractable : Interactable
{
    [SerializeField] private DialogData _dialogData;

    public override string InteractionPrompt => "E — поговорить";

    private DialogManager _dialogManager;

    public override void Initialize()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _defaultColor = _sr.color;

        // DialogManager инициализируется позже (Order = UI), поэтому берём при первом взаимодействии
    }

    public override void OnInteract(Interactor interactor)
    {
        if (_dialogManager == null)
        {
            if (!Services.IsRegistered<DialogManager>())
            {
                Debug.LogError("DialogManager не зарегистрирован. Добавьте его в сцену.", this);
                return;
            }
            _dialogManager = Services.Get<DialogManager>();
        }

        if (_dialogManager.IsDialogActive) return;
        _dialogManager.StartDialog(_dialogData);
    }
}
