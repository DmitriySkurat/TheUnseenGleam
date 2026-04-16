using UnityEngine;

public class GameSettingsLoader : MonoBehaviour
{
    private static readonly (int width, int height)[] PredefinedResolutions =
    {
        (1280,  720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };

    public static void LoadAndApplySavedSettings()
    {
        float volume = PlayerPrefs.HasKey("VolumePreference") ? PlayerPrefs.GetFloat("VolumePreference") : 1.0f;
        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference") ? PlayerPrefs.GetInt("FullScreenPreference") == 1 : true;
        int resolutionIndex = PlayerPrefs.HasKey("ResolutionIndex") ? PlayerPrefs.GetInt("ResolutionIndex") : 1; // default Full HD
        int savedFps = PlayerPrefs.HasKey("FpsPreference") ? PlayerPrefs.GetInt("FpsPreference") : -1;

        AudioListener.volume = volume;
        Application.targetFrameRate = savedFps; // -1 = unlimited

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, PredefinedResolutions.Length - 1);
        var res = PredefinedResolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen);
    }
}
