using UnityEngine;

public class DialogNpcInteractable : Interactable
{
    [SerializeField] private DialogData _dialogData;
    [SerializeField] private string _npcName = "NPC";

    private DialogManager _dialogManager;
    private PlayerContext _playerCtx;

    public override void Initialize()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _defaultColor = _sr.color;
    }

    public override void OnInteract(Interactor interactor)
    {
        if (_playerCtx == null && Services.IsRegistered<PlayerContext>())
            _playerCtx = Services.Get<PlayerContext>();

        if (_playerCtx != null && _playerCtx.isInDialog) return;

        if (_dialogManager == null)
        {
            if (!Services.IsRegistered<DialogManager>())
            {
                Debug.LogError("DialogManager не зарегистрирован.", this);
                return;
            }
            _dialogManager = Services.Get<DialogManager>();
        }

        _dialogManager.StartDialog(_dialogData, _npcName);
    }
}
