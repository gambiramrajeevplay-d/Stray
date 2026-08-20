using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scarf")]
    public TMP_Text scarfText;
    public string scarfMessage = "You found the scarf!";
    public float scarfTextDuration = 3f;

    [Header("Scarf Panel")]
    public GameObject scarfPanel;

    [Header("Scarf Image Sequence")]
    public ScarfImageSequence scarfImageSequence;

    [Header("Fade")]
    public FadeController fadeController;

    [Header("Cat Control")]
    public CatController catController;

    [Header("Cutscene")]
    public GameObject cutsceneObject;

    [Tooltip("Camera used during the cutscene.")]
    public Camera cutsceneCamera;

    [Tooltip("How long the cutscene should play.")]
    public float cutsceneDuration = 10f;

    [Tooltip("Play the cutscene automatically when the game starts.")]
    public bool playCutsceneOnStart = false;

    [Tooltip("If enabled, the cutscene can only be played once.")]
    public bool playCutsceneOnce = true;

    [Header("Level")]
    [Tooltip("This GameObject will be enabled after the cutscene ends.")]
    public GameObject levelGameObject;

    private bool cutscenePlayed = false;
    private bool scarfCollected = false;

    private Coroutine scarfTextCoroutine;
    private Coroutine cutsceneCoroutine;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // =====================================================
        // FIND SCARF IMAGE SEQUENCE
        // =====================================================

        if (scarfImageSequence == null)
        {
            scarfImageSequence =
                FindFirstObjectByType<ScarfImageSequence>();

            if (scarfImageSequence != null)
            {
                Debug.Log(
                    "GameManager: ScarfImageSequence found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: ScarfImageSequence could not be found."
                );
            }
        }

        // =====================================================
        // FIND FADE CONTROLLER
        // =====================================================

        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<FadeController>();

            if (fadeController != null)
            {
                Debug.Log(
                    "GameManager: FadeController found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: FadeController could not be found."
                );
            }
        }

        // =====================================================
        // FIND CAT CONTROLLER
        // =====================================================

        if (catController == null)
        {
            catController =
                FindFirstObjectByType<CatController>();

            if (catController != null)
            {
                Debug.Log(
                    "GameManager: CatController found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: CatController could not be found."
                );
            }
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // =====================================================
        // SCARF TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.gameObject.SetActive(false);
        }

        // =====================================================
        // SCARF PANEL
        // =====================================================

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(false);
        }

        // =====================================================
        // LEVEL
        // =====================================================

        if (levelGameObject != null)
        {
            levelGameObject.SetActive(false);
        }

        // =====================================================
        // CUTSCENE
        // =====================================================

        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(false);
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        // =====================================================
        // CAT CONTROL
        // =====================================================

        if (catController != null)
        {
            catController.canControl = false;
        }

        // =====================================================
        // INITIAL FADE
        // =====================================================

        if (fadeController != null)
        {
            StartCoroutine(
                InitialFadeInRoutine()
            );
        }

        // =====================================================
        // AUTO CUTSCENE
        // =====================================================

        if (playCutsceneOnStart)
        {
            StartCutscene();
        }
    }

    // =========================================================
    // INITIAL FADE
    // =========================================================

    private IEnumerator InitialFadeInRoutine()
    {
        yield return null;

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }
    }

    // =========================================================
    // SCARF COLLECTED
    // =========================================================

    public void ScarfCollected()
    {
        if (scarfCollected)
            return;

        scarfCollected = true;

        Debug.Log(
            "SCARF COLLECTED"
        );

        if (scarfTextCoroutine != null)
        {
            StopCoroutine(
                scarfTextCoroutine
            );
        }

        scarfTextCoroutine = StartCoroutine(
            ScarfCollectedRoutine()
        );
    }

    // =========================================================
    // SCARF ROUTINE
    // =========================================================

    private IEnumerator ScarfCollectedRoutine()
    {
        // =====================================================
        // DISABLE CAT CONTROL
        // =====================================================

        SetCatControl(false);

        // =====================================================
        // HIDE SCARF PANEL
        // =====================================================

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(false);
        }

        // =====================================================
        // SHOW TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.text = scarfMessage;
            scarfText.gameObject.SetActive(true);
        }

        Debug.Log("Showing scarf text.");

        // =====================================================
        // WAIT
        // =====================================================

        yield return new WaitForSecondsRealtime(
            scarfTextDuration
        );

        // =====================================================
        // FADE TO BLACK
        // =====================================================

        if (fadeController != null)
        {
            Debug.Log("Fading into scarf image sequence.");

            yield return StartCoroutine(
                fadeController.FadeOutRoutine()
            );
        }

        // =====================================================
        // HIDE TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.gameObject.SetActive(false);
        }

        // =====================================================
        // START IMAGE SEQUENCE WHILE SCREEN IS BLACK
        // =====================================================

        if (scarfImageSequence != null)
        {
            Debug.Log("Starting scarf image sequence.");

            scarfImageSequence.PlaySequence();
        }
        else
        {
            Debug.LogWarning(
                "Scarf Image Sequence is NOT assigned."
            );

            FinishScarfSequence();
        }

        // =====================================================
        // FADE INTO IMAGE SEQUENCE
        // =====================================================

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        scarfTextCoroutine = null;
    }

    // =========================================================
    // SCARF SEQUENCE FINISHED
    // =========================================================

    public void ScarfSequenceFinished()
    {
        StartCoroutine(
            ScarfSequenceFinishedRoutine()
        );
    }

    private IEnumerator ScarfSequenceFinishedRoutine()
    {
        Debug.Log(
            "Scarf sequence finished."
        );

        // =====================================================
        // FADE TO BLACK
        // =====================================================

        if (fadeController != null)
        {
            Debug.Log(
                "Sequence finished. Fading to black."
            );

            yield return StartCoroutine(
                fadeController.FadeOutRoutine()
            );
        }

        // =====================================================
        // KEEP LEVEL ACTIVE
        // =====================================================

        // Level remains active.
        // We are only using the fade to transition.

        Debug.Log(
            "Level remains active after scarf sequence."
        );

        yield return null;

        // =====================================================
        // KEEP CAT CONTROL DISABLED
        // =====================================================

        SetCatControl(false);

        // =====================================================
        // SHOW FINAL SCARF PANEL
        // =====================================================

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(true);
        }

        // =====================================================
        // FADE BACK INTO LEVEL
        // =====================================================

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        Debug.Log(
            "Scarf panel shown. Cat controls remain disabled."
        );
    }

    // =========================================================
    // FALLBACK
    // =========================================================

    private void FinishScarfSequence()
    {
        // Level stays active.
        SetCatControl(false);

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(true);
        }
    }

    // =========================================================
    // CAT CONTROL
    // =========================================================

    public void SetCatControl(bool enabled)
    {
        if (catController == null)
        {
            catController =
                FindFirstObjectByType<CatController>();
        }

        if (catController != null)
        {
            catController.canControl = enabled;

            Debug.Log(
                "Cat Control = " +
                enabled
            );
        }
        else
        {
            Debug.LogWarning(
                "GameManager: CatController not found."
            );
        }
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartScene()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    // =========================================================
    // START CUTSCENE
    // =========================================================

    public void StartCutscene()
    {
        if (playCutsceneOnce &&
            cutscenePlayed)
        {
            Debug.Log(
                "Cutscene has already been played."
            );

            return;
        }

        if (cutsceneCoroutine != null)
        {
            StopCoroutine(
                cutsceneCoroutine
            );
        }

        cutsceneCoroutine =
            StartCoroutine(
                CutsceneRoutine()
            );
    }

    // =========================================================
    // CUTSCENE ROUTINE
    // =========================================================

    private IEnumerator CutsceneRoutine()
    {
        cutscenePlayed = true;

        // =====================================================
        // CAT CONTROL OFF
        // =====================================================

        SetCatControl(false);

        // =====================================================
        // LEVEL OFF
        // =====================================================

        if (levelGameObject != null)
        {
            levelGameObject.SetActive(false);
        }

        // =====================================================
        // CUTSCENE ON
        // =====================================================

        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(true);
        }

        // =====================================================
        // CUTSCENE CAMERA ON
        // =====================================================

        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(true);
            cutsceneCamera.enabled = true;
        }

        Debug.Log(
            "CUTSCENE STARTED"
        );

        // =====================================================
        // WAIT
        // =====================================================

        yield return new WaitForSecondsRealtime(
            cutsceneDuration
        );

        // =====================================================
        // FADE TO BLACK
        // =====================================================

        if (fadeController != null)
        {
            Debug.Log(
                "CUTSCENE: FADING TO BLACK"
            );

            yield return StartCoroutine(
                fadeController.FadeOutRoutine()
            );
        }

        // =====================================================
        // DISABLE CUTSCENE CAMERA
        // =====================================================

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        // =====================================================
        // DISABLE CUTSCENE OBJECT
        // =====================================================

        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(false);
        }

        // =====================================================
        // ENABLE LEVEL
        // =====================================================

        if (levelGameObject != null)
        {
            levelGameObject.SetActive(true);
        }

        Debug.Log(
            "LEVEL ENABLED WHILE SCREEN IS BLACK"
        );

        yield return null;

        // =====================================================
        // FADE INTO GAMEPLAY
        // =====================================================

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        // =====================================================
        // ENABLE CAT CONTROL
        // =====================================================

        SetCatControl(true);

        Debug.Log(
            "CUTSCENE FINISHED - GAMEPLAY ACTIVE"
        );

        cutsceneCoroutine = null;
    }
}