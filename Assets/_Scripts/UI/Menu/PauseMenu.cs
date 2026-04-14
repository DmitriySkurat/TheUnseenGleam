// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class PauseMenu : MonoBehaviour
// {
//     public static bool isPaused = false;
//     public GameObject pauseGameMenu;
//     public GameObject optionsMenu;
//     private OptionsController optionsController;

//     private void Start()
//     {
//         if (optionsMenu)
//             optionsController = optionsMenu.GetComponent<OptionsController>();

//         // Reset pause state on scene start
//         isPaused = false;
//         Time.timeScale = 1f;
//         if (pauseGameMenu)
//             pauseGameMenu.SetActive(false);
//     }

//     private void Update()
//     {
//         if (InputSystem.Escape())
//         {
//             if (optionsMenu && optionsMenu.activeSelf)
//             {
//                 if (optionsController)
//                     optionsController.CloseSettings();
//                 else
//                     optionsMenu.SetActive(false);
//                 return;
//             }
//             if (isPaused)
//             {
//                 Resume();
//             }
//             else
//             {
//                 Pause();
//             }
//         }
//     }

//     public void Resume()
//     {
//         pauseGameMenu.SetActive(false);
//         Time.timeScale = 1f;
//         isPaused = false;
//     }

//     public void Pause()
//     {
//         pauseGameMenu.SetActive(true);
//         Time.timeScale = 0f;
//         isPaused = true;
//     }

//     public void loadMenu()
//     {
//         Time.timeScale = 1f;
//         isPaused = false;
//         SceneManager.LoadScene("Menu");
//     }
// }