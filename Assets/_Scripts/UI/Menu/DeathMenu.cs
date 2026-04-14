using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public GameObject deathGameMenu;

    private bool isDeathMenuActive = false;

    // private void Start()
    // {
    //     PlayerController.OnPlayerDied += ActivateDeathMenu;
    // }

    private void ActivateDeathMenu()
    {
        isDeathMenuActive = true;
        deathGameMenu.SetActive(true);
    }

    public void Respawn()
    {
        deathGameMenu.SetActive(false);
        isDeathMenuActive = false;

        Utility.SceneLoader.Load("Demo");
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        Utility.SceneLoader.Load("Menu");
    }

    // private void OnDestroy()
    // {
    //     PlayerController.OnPlayerDied -= ActivateDeathMenu;
    // }
}
