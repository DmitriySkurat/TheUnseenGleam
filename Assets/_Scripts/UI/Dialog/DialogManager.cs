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
    private DialogData _currentDialog;
    private int _lineIndex;
    private bool _isTyping;
    private Coroutine _typingCoroutine;

    public bool IsDialogActive { get; private set; }

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        Services.Register(this);
        _dialogPanel.SetActive(false);
    }

    public void Dispose()
    {
        if (IsDialogActive)
            EndDialog();
        Services.Unregister<DialogManager>();
    }

    public void StartDialog(DialogData data)
    {
        if (IsDialogActive || data == null) return;

        _currentDialog = data;
        _lineIndex = 0;
        IsDialogActive = true;

        _playerPortrait.sprite = data.playerSprite;
        _npcPortrait.sprite = data.npcSprite;
        _playerPortrait.gameObject.SetActive(data.playerSprite != null);
        _npcPortrait.gameObject.SetActive(data.npcSprite != null);

        _dialogPanel.SetActive(true);
        Time.timeScale = 0f;

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

        _speakerNameText.text = isPlayer ? _currentDialog.playerName : _currentDialog.npcName;
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
            yield return new WaitForSecondsRealtime(_typingSpeed);
        }
        _isTyping = false;
        if (_continueIndicator) _continueIndicator.SetActive(true);
    }

    private void OnAdvance()
    {
        if (!IsDialogActive) return;

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

    private void EndDialog()
    {
        IsDialogActive = false;
        _dialogPanel.SetActive(false);
        Time.timeScale = 1f;

        _inputManager.OnInteractStarted -= OnAdvance;
        _inputManager.OnJumpStarted -= OnAdvance;

        _currentDialog = null;
    }
}
