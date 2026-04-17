using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionEndHandler : MonoBehaviour
{
    private bool _isEnding;

    public void EndSession(float delay = 1.5f)
    {
        if (_isEnding) return;
        _isEnding = true;
        
        Debug.Log($"Session will end in {delay} seconds...");
        
        StartCoroutine(EndSessionRoutine(delay));
    }

    private IEnumerator EndSessionRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"Session ended. Restarting scene...");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
