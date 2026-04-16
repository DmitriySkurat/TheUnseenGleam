using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class OptionsController : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    public Slider slider;
    public Toggle fullScreenToggle;

    public Dropdown resolutionDropdown;
    Resolution[] resolutions;

    public GameObject menuButtonsParent;

    private InputManager _inputManager;

    public void Initialize()
    {
        _inputManager = Services.Get<InputManager>();

        _inputManager.OnEscape += HandleEscape;
        
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
    }
    
    public void Dispose()
    {
        _inputManager.OnEscape -= HandleEscape;
    }
    
    public void HandleEscape()
    {
        if (gameObject.activeSelf)
        {
            CloseSettings();
        }
    }
    
    
    public void Update()
    {
        AudioListener.volume = slider.value;    
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

        if (index >= 0 && index < resolutions.Length)
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, isFullscreen);
        else
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);

        AudioListener.volume = slider.value;

        if (Camera.main != null)
            Camera.main.aspect = (float)Screen.width / Screen.height;
    }
    

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullScreenPreference", fullScreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("VolumePreference", slider.value);
        PlayerPrefs.Save();

        Debug.Log("Settings saved");
    }
    
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.SetResolution(Screen.width, Screen.height, isFullScreen);
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        int savedIndex = PlayerPrefs.HasKey("ResolutionPreference")
            ? PlayerPrefs.GetInt("ResolutionPreference")
            : currentResolutionIndex;
        resolutionDropdown.value = savedIndex;

        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference")
            ? PlayerPrefs.GetInt("FullScreenPreference") == 1
            : true;

        fullScreenToggle.isOn = isFullscreen;

        if (savedIndex >= 0 && savedIndex < resolutions.Length)
            Screen.SetResolution(resolutions[savedIndex].width, resolutions[savedIndex].height, isFullscreen);
        else
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);

        slider.value = PlayerPrefs.HasKey("VolumePreference")
            ? PlayerPrefs.GetFloat("VolumePreference")
            : 1.0f;
    }
}
