using UnityEngine;

public class OldManNPC : Interactable
{
    [Header("Dialog")]
    [SerializeField] private DialogData _dialogData;
    [SerializeField] private string _npcName = "Старик";

    [Header("Reward")]
    [SerializeField] private ItemData _mirrorItem;

    private DialogManager _dialogManager;
    private PlayerContext _playerCtx;
    private bool _dialogStarted;
    private bool _done;

    public override void Initialize()
    {
        base.Initialize();
        if (Services.IsRegistered<PlayerContext>())
            _playerCtx = Services.Get<PlayerContext>();
    }

    void Update()
    {
        if (!_dialogStarted || _done) return;

        if (_playerCtx?.transform != null)
        {
            float dir = _playerCtx.transform.position.x - transform.position.x;
            Vector3 s = transform.localScale;
            if (Mathf.Abs(dir) > 0.05f)
                s.x = dir > 0f ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (_playerCtx == null || _playerCtx.isInDialog) return;

        _done = true;
        GiveMirror();
    }

    private void GiveMirror()
    {
        if (_mirrorItem == null) return;

        var inventory = _playerCtx?.inventory;
        var hotbar = _playerCtx?.hotbar;
        if (inventory == null) return;

        if (_mirrorItem.CanUse && hotbar != null && !hotbar.CanFitItem(_mirrorItem, 1))
            return;

        inventory.Add(_mirrorItem, 1);
    }

    public override bool CanBeInteractedBy(Interactor interactor)
    {
        if (_done || _dialogStarted) return false;
        return base.CanBeInteractedBy(interactor);
    }

    public override void OnInteract(Interactor interactor)
    {
        if (_done || _dialogStarted) return;
        if (interactor is not PlayerInteractor) return;

        if (_dialogManager == null && Services.IsRegistered<DialogManager>())
            _dialogManager = Services.Get<DialogManager>();

        if (_dialogManager == null) return;

        _dialogManager.StartDialog(_dialogData, _npcName);
        _dialogStarted = true;
    }
}
