using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public GameObject deathGameMenu;

    private bool isDeathMenuActive = false;

    private void Start()
    {
        PlayerController.OnPlayerDied += ActivateDeathMenu;
    }

    private void ActivateDeathMenu()
    {
        isDeathMenuActive = true;
        deathGameMenu.SetActive(true);
    }

    public void Respawn()
    {
        deathGameMenu.SetActive(false);
        isDeathMenuActive = false;

        SceneManager.LoadScene("Demo");
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerDied -= ActivateDeathMenu;
    }
}
