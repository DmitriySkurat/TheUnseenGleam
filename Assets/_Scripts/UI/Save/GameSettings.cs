// using UnityEngine;

// public class GameSettings : MonoBehaviour 
// {
//     public void Start()
//     {
//         ApplySavedSettings();
//     }

//     public static void ApplySavedSettings()
//     {
//         float volume = PlayerPrefs.HasKey("VolumePreference") ? PlayerPrefs.GetFloat("VolumePreference") : 1.0f;
//         bool isFullscreen = PlayerPrefs.HasKey("FullScreenPreference") ? PlayerPrefs.GetInt("FullScreenPreference") == 1 : true;
//         int resolutionIndex = PlayerPrefs.HasKey("ResolutionPreference") ? PlayerPrefs.GetInt("ResolutionPreference") : -1;

//         AudioListener.volume = volume;
//         Screen.fullScreen = isFullscreen;

//         if (resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
//         {
//             Resolution res = Screen.resolutions[resolutionIndex];
//             Screen.SetResolution(res.width, res.height, Screen.fullScreen);
//         }
//     }
// }
