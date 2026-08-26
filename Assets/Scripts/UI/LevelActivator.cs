using UnityEngine;

public class LevelActivator : MonoBehaviour
{
    public GameObject[] levels;

    private void Awake()
    {
        int levelToLoad = PlayerPrefs.GetInt(
            StringsData.levelToLoad,
            0
        );

        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelActivator: No levels assigned.");
            return;
        }

        if (levelToLoad < 0 || levelToLoad >= levels.Length)
        {
            Debug.LogWarning(
                "LevelActivator: Invalid level index " +
                levelToLoad +
                ". Loading level 0 instead."
            );

            levelToLoad = 0;
        }

        // Disable all levels first
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                levels[i].SetActive(false);
            }
        }

        // Enable selected level
        if (levels[levelToLoad] != null)
        {
            levels[levelToLoad].SetActive(true);

            Debug.Log(
                "Level loaded: " + levelToLoad
            );
        }
    }
}