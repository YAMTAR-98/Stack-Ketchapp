
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    [ContextMenu("LoadMainMenu Start Scene")]
    public void LoadMainMenu()
    {
        LoadScene("StartScene");
        Debug.Log("StartScene Loaded");
    }
}
