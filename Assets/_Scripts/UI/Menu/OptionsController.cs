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

    private static readonly (int width, int height, string label)[] PredefinedResolutions =
    {
        (1280,  720,  "1280×720 (HD)"),
        (1920, 1080, "1920×1080 (Full HD)"),
        (2560, 1440, "2560×1440 (QHD)"),
        (3840, 2160, "3840×2160 (4K UHD)"),
    };

    private const int FpsUnlimitedValue = 241;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();
        _inputManager.OnEscape += HandleEscape;

        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var res in PredefinedResolutions)
            options.Add(res.label);
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
        _inputManager.OnEscape -= HandleEscape;
        fpsSlider.onValueChanged.RemoveAllListeners();
    }

    public void HandleEscape()
    {
        if (gameObject.activeSelf)
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

        if (index >= 0 && index < PredefinedResolutions.Length)
        {
            var res = PredefinedResolutions[index];
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
            : GetDefaultResolutionIndex();
        savedIndex = Mathf.Clamp(savedIndex, 0, PredefinedResolutions.Length - 1);
        resolutionDropdown.value = savedIndex;

        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference")
            ? PlayerPrefs.GetInt("FullScreenPreference") == 1
            : true;
        fullScreenToggle.isOn = isFullscreen;

        var res = PredefinedResolutions[savedIndex];
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

    private int GetDefaultResolutionIndex()
    {
        for (int i = 0; i < PredefinedResolutions.Length; i++)
            if (PredefinedResolutions[i].width == 1920 && PredefinedResolutions[i].height == 1080)
                return i;
        return 0;
    }
}
