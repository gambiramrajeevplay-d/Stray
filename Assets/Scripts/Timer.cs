using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("How many seconds the timer should run.")]
    public float timerDuration = 60f;

    [Tooltip("TimerText inside Level > InGame UI Canvas.")]
    public TMP_Text timerText;

    private float currentTime;
    private bool timerRunning = false;

    private void Start()
    {
        // =====================================================
        // GET TIMER TEXT FROM GAMEMANAGER FIRST
        // =====================================================

        if (GameManager.Instance != null &&
            GameManager.Instance.timerText != null)
        {
            timerText = GameManager.Instance.timerText;
        }

        // =====================================================
        // FALLBACK - FIND USING TAG
        // =====================================================

        if (timerText == null)
        {
            GameObject timerObject =
                GameObject.FindGameObjectWithTag("TimerText");

            if (timerObject != null)
            {
                timerText =
                    timerObject.GetComponent<TMP_Text>();
            }
            else
            {
                Debug.LogWarning(
                    "Timer: No GameObject with the tag 'TimerText' was found."
                );
            }
        }

        // =====================================================
        // TIMER DOES NOT START AUTOMATICALLY
        // =====================================================

        timerRunning = false;

        // =====================================================
        // INITIAL TIMER VALUE
        // =====================================================

        currentTime = timerDuration;

        UpdateTimerText();

        // =====================================================
        // MAKE SURE SCARF PANEL IS HIDDEN
        // =====================================================

        if (GameManager.Instance != null &&
            GameManager.Instance.scarfPanel != null)
        {
            GameManager.Instance.scarfPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            timerRunning = false;

            UpdateTimerText();

            ShowScarfPanel();

            return;
        }

        UpdateTimerText();
    }

    // =========================================================
    // START TIMER
    // =========================================================

    public void StartTimer()
    {
        currentTime = timerDuration;

        timerRunning = true;

        // Make sure TimerText is available
        if (timerText == null &&
            GameManager.Instance != null)
        {
            timerText =
                GameManager.Instance.timerText;
        }

        // Make TimerText visible when timer starts
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        UpdateTimerText();

        Debug.Log(
            "Timer started: " +
            timerDuration +
            " seconds."
        );
    }

    // =========================================================
    // TIMER TEXT
    // =========================================================

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int minutes =
            Mathf.FloorToInt(currentTime / 60f);

        int seconds =
            Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }

    // =========================================================
    // SHOW SCARF PANEL
    // =========================================================

    private void ShowScarfPanel()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning(
                "Timer: GameManager.Instance not found."
            );

            return;
        }

        if (GameManager.Instance.scarfPanel == null)
        {
            Debug.LogWarning(
                "Timer: Scarf Panel is not assigned in GameManager."
            );

            return;
        }

        GameManager.Instance.scarfPanel.SetActive(true);

        // Disable cat controls when timer ends
        GameManager.Instance.SetCatControl(false);

        Debug.Log(
            "Timer finished - Scarf Panel shown."
        );
    }

    // =========================================================
    // STOP TIMER
    // =========================================================

    public void StopTimer()
    {
        timerRunning = false;

        Debug.Log(
            "Timer stopped."
        );
    }

    // =========================================================
    // RESTART TIMER
    // =========================================================

    public void RestartTimer()
    {
        currentTime = timerDuration;

        timerRunning = true;

        // Show TimerText again
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.scarfPanel != null)
        {
            GameManager.Instance.scarfPanel.SetActive(false);
        }

        UpdateTimerText();

        Debug.Log(
            "Timer restarted."
        );
    }
}