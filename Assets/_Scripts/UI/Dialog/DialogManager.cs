using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogManager : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Header("Panel")]
    [SerializeField] private GameObject _dialogPanel;

    [Header("Portraits")]
    [SerializeField] private Image _playerPortrait;
    [SerializeField] private Image _npcPortrait;

    [Header("Text")]
    [SerializeField] private Text _speakerNameText;
    [SerializeField] private Text _dialogText;
    [SerializeField] private GameObject _continueIndicator;

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.03f;

    private static readonly Color _dimColor = new Color(0.45f, 0.45f, 0.45f);

    private InputManager _inputManager;
    private PlayerContext _playerCtx;
    private DialogData _currentDialog;
    private string _currentNpcName;
    private int _lineIndex;
    private bool _isTyping;
    private Coroutine _typingCoroutine;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        if (Services.IsRegistered<PlayerContext>())
            _playerCtx = Services.Get<PlayerContext>();
        Services.Register(this);
        _dialogPanel.SetActive(false);
    }

    public void Dispose()
    {
        if (_playerCtx != null && _playerCtx.isInDialog)
            EndDialog();
        Services.Unregister<DialogManager>();
    }

    public void StartDialog(DialogData data, string npcName = "NPC")
    {
        if (_playerCtx != null && _playerCtx.isInDialog) return;
        if (data == null) return;

        _currentDialog = data;
        _currentNpcName = npcName;
        _lineIndex = 0;

        _playerPortrait.sprite = data.playerSprite;
        _npcPortrait.sprite = data.npcSprite;
        _playerPortrait.gameObject.SetActive(data.playerSprite != null);
        _npcPortrait.gameObject.SetActive(data.npcSprite != null);

        if (_playerCtx != null) _playerCtx.isInDialog = true;
        _dialogPanel.SetActive(true);

        _inputManager.OnInteractStarted += OnAdvance;
        _inputManager.OnJumpStarted += OnAdvance;

        ShowLine();
    }

    private void ShowLine()
    {
        if (_lineIndex >= _currentDialog.lines.Count)
        {
            EndDialog();
            return;
        }

        var line = _currentDialog.lines[_lineIndex];
        bool isPlayer = line.speaker == DialogSpeaker.Lian;

        _speakerNameText.text = isPlayer ? _currentDialog.playerName : _currentNpcName;
        _playerPortrait.color = isPlayer ? Color.white : _dimColor;
        _npcPortrait.color = isPlayer ? _dimColor : Color.white;

        if (_continueIndicator) _continueIndicator.SetActive(false);

        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string text)
    {
        _isTyping = true;
        _dialogText.text = "";
        foreach (char c in text)
        {
            _dialogText.text += c;
            yield return new WaitForSeconds(_typingSpeed);
        }
        _isTyping = false;
        if (_continueIndicator) _continueIndicator.SetActive(true);
    }

    private void OnAdvance()
    {
        if (_playerCtx == null || !_playerCtx.isInDialog) return;

        if (_isTyping)
            FinishLine();
        else
        {
            _lineIndex++;
            ShowLine();
        }
    }

    private void FinishLine()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _isTyping = false;
        _dialogText.text = _currentDialog.lines[_lineIndex].text;
        if (_continueIndicator) _continueIndicator.SetActive(true);
    }

    public void Cancel()
    {
        if (_playerCtx == null || !_playerCtx.isInDialog) return;
        EndDialog();
    }

    private void EndDialog()
    {
        if (_playerCtx != null)
        {
            _playerCtx.isInDialog = false;
            _playerCtx.timeLastInteraction = Time.time;
        }
        _dialogPanel.SetActive(false);

        _inputManager.OnInteractStarted -= OnAdvance;
        _inputManager.OnJumpStarted -= OnAdvance;

        _currentDialog = null;
    }
}
