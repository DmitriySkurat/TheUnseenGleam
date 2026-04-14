using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour
{
    public GameObject vicrotyGameMenu;

    private bool isVictoryMenuActive = false;

    private void Start()
    {
        PlayerController.OnPlayerWin += ActivateVicrotyMenu;
    }

    private void ActivateVicrotyMenu()
    {
        isVictoryMenuActive = true;
        vicrotyGameMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Respawn()
    {
        SceneManager.LoadScene("Demo");
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerWin -= ActivateVicrotyMenu;
    }
}
