using UnityEngine;

public class GameSettingsLoader : MonoBehaviour
{
    public static void LoadAndApplySavedSettings()
    {
        float volume = PlayerPrefs.HasKey("VolumePreference") ? PlayerPrefs.GetFloat("VolumePreference") : 1.0f;
        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference") ? PlayerPrefs.GetInt("FullScreenPreference") == 1 : true;
        int resolutionIndex = PlayerPrefs.HasKey("ResolutionIndex") ? PlayerPrefs.GetInt("ResolutionIndex") : GameResolutions.DefaultIndex;
        int savedFps = PlayerPrefs.HasKey("FpsPreference") ? PlayerPrefs.GetInt("FpsPreference") : -1;

        AudioListener.volume = volume;
        Application.targetFrameRate = savedFps; // -1 = unlimited

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, GameResolutions.resolutions.Length - 1);
        var res = GameResolutions.resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen);
    }
}
