using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OptionsController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    public Slider slider;
    public Slider fpsSlider;
    public Text fpsLabel;
    public Toggle fullScreenToggle;
    public Dropdown resolutionDropdown;
    public GameObject menuButtonsParent;

    private InputManager _inputManager;

    private const int FpsUnlimitedValue = 241;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        _inputManager.OnCloseWindow += HandleCloseWindow;

        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var res in GameResolutions.resolutions)
            options.Add(GameResolutions.ToLabel(res.width, res.height));
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();

        fpsSlider.minValue = 24;
        fpsSlider.maxValue = FpsUnlimitedValue;
        fpsSlider.wholeNumbers = true;
        fpsSlider.onValueChanged.AddListener(_ => UpdateFpsLabel());

        LoadSettings();
    }

    public void Dispose()
    {
        _inputManager.OnCloseWindow -= HandleCloseWindow;
        fpsSlider.onValueChanged.RemoveAllListeners();
    }

    public void HandleCloseWindow()
    {
        if (gameObject.activeSelf && !PauseMenu.isPaused)
            CloseSettings();
    }

    public void Update()
    {
        AudioListener.volume = slider.value;
    }

    private void UpdateFpsLabel()
    {
        if (fpsLabel == null) return;
        int val = Mathf.RoundToInt(fpsSlider.value);
        fpsLabel.text = val >= FpsUnlimitedValue ? "Unl" : val.ToString();
    }

    public void CloseSettings()
    {
        ApplyCurrentSettings();
        SaveSettings();
        gameObject.SetActive(false);

        if (menuButtonsParent != null)
            menuButtonsParent.SetActive(true);
    }

    private void ApplyCurrentSettings()
    {
        int index = resolutionDropdown.value;
        bool isFullscreen = fullScreenToggle.isOn;

        if (index >= 0 && index < GameResolutions.resolutions.Length)
        {
            var res = GameResolutions.resolutions[index];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);
        }

        AudioListener.volume = slider.value;

        int fps = Mathf.RoundToInt(fpsSlider.value);
        Application.targetFrameRate = fps >= FpsUnlimitedValue ? -1 : fps;

        StartCoroutine(ApplyCameraAspectNextFrame());
    }

    private IEnumerator ApplyCameraAspectNextFrame()
    {
        yield return null;
        if (Camera.main != null)
        {
            Camera.main.rect = new Rect(0, 0, 1, 1);
            Camera.main.aspect = (float)Screen.width / Screen.height;
        }
    }

    public void SetFullScreen(bool isFullScreen)
    {
        // Applied on CloseSettings via ApplyCurrentSettings
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullScreenPreference", fullScreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("VolumePreference", slider.value);
        int fps = Mathf.RoundToInt(fpsSlider.value);
        PlayerPrefs.SetInt("FpsPreference", fps >= FpsUnlimitedValue ? -1 : fps);
        PlayerPrefs.Save();

        Debug.Log("Settings saved");
    }

    public void LoadSettings()
    {
        int savedIndex = PlayerPrefs.HasKey("ResolutionIndex")
            ? PlayerPrefs.GetInt("ResolutionIndex")
            : GameResolutions.DefaultIndex;
        savedIndex = Mathf.Clamp(savedIndex, 0, GameResolutions.resolutions.Length - 1);
        resolutionDropdown.value = savedIndex;

        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference")
            ? PlayerPrefs.GetInt("FullScreenPreference") == 1
            : true;
        fullScreenToggle.isOn = isFullscreen;

        var res = GameResolutions.resolutions[savedIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen);

        slider.value = PlayerPrefs.HasKey("VolumePreference")
            ? PlayerPrefs.GetFloat("VolumePreference")
            : 1.0f;

        int savedFps = PlayerPrefs.HasKey("FpsPreference")
            ? PlayerPrefs.GetInt("FpsPreference")
            : -1;
        fpsSlider.value = savedFps == -1 ? FpsUnlimitedValue : Mathf.Clamp(savedFps, 24, FpsUnlimitedValue - 1);
        UpdateFpsLabel();
    }
}
