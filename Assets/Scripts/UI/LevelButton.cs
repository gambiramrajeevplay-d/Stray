using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadLevel()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is not assigned!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}