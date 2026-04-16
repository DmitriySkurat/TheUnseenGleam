using UnityEngine;

public class GameSettingsLoader : MonoBehaviour
{
    public static void LoadAndApplySavedSettings()
    {
        float volume = PlayerPrefs.HasKey("VolumePreference") ? PlayerPrefs.GetFloat("VolumePreference") : 1.0f;
        bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference") ? PlayerPrefs.GetInt("FullScreenPreference") == 1 : true;
        int resolutionIndex = PlayerPrefs.HasKey("ResolutionPreference") ? PlayerPrefs.GetInt("ResolutionPreference") : -1;

        AudioListener.volume = volume;

        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);
        }

        if (Camera.main != null)
            Camera.main.aspect = (float)Screen.width / Screen.height;
    }
}
