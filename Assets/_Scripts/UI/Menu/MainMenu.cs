using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ContinueGame()
    {
        Utility.SceneLoader.Load("Demo");
    }
    public void NewGame()
    {
        //SaveManager.DeleteSave();
        Utility.SceneLoader.Load("Demo");
    }
    public void ExitGame()
    {
        Debug.Log("Game is closed");
        Application.Quit();
    }
}