using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // =========================================================
    // SCARF
    // =========================================================

    [Header("Scarf")]
    [Tooltip("Collected Text inside Level > InGame UI Canvas.")]
    public TMP_Text scarfText;

    [Tooltip("Message displayed when the scarf is collected.")]
    public string scarfMessage = "You found the scarf!";

    [Tooltip("Time between each character of the scarf text.")]
    public float scarfTypewriterSpeed = 0.05f;

    [Tooltip("How long the complete scarf text remains visible.")]
    public float scarfTextDuration = 3f;

    // =========================================================
    // SCARF PANEL
    // =========================================================

    [Header("Scarf Panel")]
    public GameObject scarfPanel;

    // =========================================================
    // SCARF IMAGE SEQUENCE
    // =========================================================

    [Header("Scarf Image Sequence")]
    public ScarfImageSequence scarfImageSequence;

    // =========================================================
    // FADE
    // =========================================================

    [Header("Fade")]
    public FadeController fadeController;

    // =========================================================
    // CAT CONTROL
    // =========================================================

    [Header("Cat Control")]
    public CatController catController;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Timer")]
    [Tooltip("Timer component that starts after the cutscene end text finishes.")]
    public Timer timer;

    [Tooltip("TimerText inside Level > InGame UI Canvas.")]
    public TMP_Text timerText;

    // =========================================================
    // CUTSCENE
    // =========================================================

    [Header("Cutscene")]
    public GameObject cutsceneObject;

    [Tooltip("Camera used during the cutscene.")]
    public Camera cutsceneCamera;

    [Tooltip("Play the cutscene automatically when the game starts.")]
    public bool playCutsceneOnStart = false;

    [Tooltip("If enabled, the cutscene can only be played once.")]
    public bool playCutsceneOnce = true;

    // =========================================================
    // CAMERA CUTSCENE
    // =========================================================

    [Header("Camera Cutscene")]
    public CameraCutsceneMover cameraCutsceneMover;

    // =========================================================
    // CUTSCENE END TEXT
    // =========================================================

    [Header("Cutscene End Text")]
    [Tooltip("TypeWritter text inside Level > InGame UI Canvas.")]
    public TMP_Text cutsceneEndText;

    [Tooltip("Message displayed after the cutscene.")]
    public string cutsceneEndMessage = "Let's go!";

    [Tooltip("Time between each character.")]
    public float typewriterSpeed = 0.05f;

    [Tooltip("How long the complete text remains visible.")]
    public float cutsceneEndTextDuration = 3f;

    [Tooltip("Time to wait after the typewriter finishes before disabling TypeWritter.")]
    public float cutsceneEndDisableDelay = 1f;

    // =========================================================
    // LEVEL
    // =========================================================

    [Header("Level")]
    [Tooltip("The gameplay level GameObject.")]
    public GameObject levelGameObject;

    // =========================================================
    // IN-GAME UI
    // =========================================================

    [Header("In-Game UI")]
    [Tooltip("Name of the scarf collected text inside the Level.")]
    public string collectedTextObjectName = "Collected Text";

    [Tooltip("Name of the cutscene end text inside the Level.")]
    public string typeWritterObjectName = "TypeWritter";

    [Tooltip("Name of the timer text inside the Level.")]
    public string timerTextObjectName = "TimerText";

    // =========================================================
    // IN-GAME SOUND
    // =========================================================

    [Header("In-Game Sound")]
    [Tooltip("Name of the gameplay sound GameObject inside the Level.")]
    [SerializeField] private string inGameSoundObjectName = "InGameSound";

    private AudioSource inGameSoundAudioSource;
    private bool inGameSoundWasPlaying;

    // =========================================================
    // TRANSITION OPTIMIZATION
    // =========================================================

    [Header("Level Transition")]
    [Tooltip("Number of frames to allow the level to initialize while screen is black.")]
    [SerializeField] private int levelInitializeFrames = 5;

    // =========================================================
    // VARIABLES
    // =========================================================

    private bool cutscenePlayed = false;
    private bool scarfCollected = false;

    private Coroutine scarfTextCoroutine;
    private Coroutine cutsceneCoroutine;
    private Coroutine cutsceneEndTextCoroutine;
    private Coroutine cameraFinishedCoroutine;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // =====================================================
        // SINGLE GAME MANAGER
        // =====================================================

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

        // =====================================================
        // FIND CAMERA CUTSCENE MOVER
        // =====================================================

        if (cameraCutsceneMover == null)
        {
            cameraCutsceneMover =
                FindFirstObjectByType<CameraCutsceneMover>();

            if (cameraCutsceneMover != null)
            {
                Debug.Log(
                    "GameManager: CameraCutsceneMover found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: CameraCutsceneMover could not be found."
                );
            }
        }

        // =====================================================
        // FIND TIMER
        // =====================================================

        if (timer == null)
        {
            timer =
                FindFirstObjectByType<Timer>();

            if (timer != null)
            {
                Debug.Log(
                    "GameManager: Timer found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: Timer will be searched again " +
                    "after the Level is enabled."
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
        // SCARF PANEL
        // =====================================================

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(false);
        }

        // =====================================================
        // LEVEL OFF
        // =====================================================

        if (levelGameObject != null)
        {
            levelGameObject.SetActive(false);
        }

        // =====================================================
        // CUTSCENE OFF
        // =====================================================

        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(false);
        }

        // =====================================================
        // CUTSCENE CAMERA OFF
        // =====================================================

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        // =====================================================
        // CAT CONTROL OFF
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
    // FIND LEVEL UI TEXTS
    // =========================================================

    private void FindLevelUITexts()
    {
        if (levelGameObject == null)
        {
            Debug.LogWarning(
                "GameManager: Cannot find Level UI texts because " +
                "Level GameObject is not assigned."
            );

            return;
        }

        // =====================================================
        // COLLECTED TEXT
        // =====================================================

        if (scarfText == null)
        {
            Transform collectedTransform =
                FindChildRecursive(
                    levelGameObject.transform,
                    collectedTextObjectName
                );

            if (collectedTransform != null)
            {
                scarfText =
                    collectedTransform.GetComponent<TMP_Text>();

                if (scarfText != null)
                {
                    Debug.Log(
                        "GameManager: Collected Text found inside Level."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "GameManager: Collected Text was found, " +
                        "but it has no TMP_Text component."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: Could not find '" +
                    collectedTextObjectName +
                    "' inside Level."
                );
            }
        }

        // =====================================================
        // TYPEWRITTER
        // =====================================================

        if (cutsceneEndText == null)
        {
            Transform typeWritterTransform =
                FindChildRecursive(
                    levelGameObject.transform,
                    typeWritterObjectName
                );

            if (typeWritterTransform != null)
            {
                cutsceneEndText =
                    typeWritterTransform.GetComponent<TMP_Text>();

                if (cutsceneEndText != null)
                {
                    Debug.Log(
                        "GameManager: TypeWritter found inside Level."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "GameManager: TypeWritter was found, " +
                        "but it has no TMP_Text component."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: Could not find '" +
                    typeWritterObjectName +
                    "' inside Level."
                );
            }
        }

        // =====================================================
        // TIMER TEXT
        // =====================================================

        if (timerText == null)
        {
            Transform timerTextTransform =
                FindChildRecursive(
                    levelGameObject.transform,
                    timerTextObjectName
                );

            if (timerTextTransform != null)
            {
                timerText =
                    timerTextTransform.GetComponent<TMP_Text>();

                if (timerText != null)
                {
                    Debug.Log(
                        "GameManager: TimerText found inside Level."
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "GameManager: TimerText was found, " +
                        "but it has no TMP_Text component."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: Could not find '" +
                    timerTextObjectName +
                    "' inside Level."
                );
            }
        }

        // =====================================================
        // TIMER COMPONENT
        // =====================================================

        if (timer == null)
        {
            timer =
                FindFirstObjectByType<Timer>();

            if (timer != null)
            {
                Debug.Log(
                    "GameManager: Timer component found after Level activation."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: Timer component could not be found."
                );
            }
        }

        // =====================================================
        // ASSIGN TIMER TEXT TO TIMER COMPONENT
        // =====================================================

        if (timer != null &&
            timerText != null)
        {
            timer.timerText = timerText;
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

        // =====================================================
        // STOP TIMER IMMEDIATELY
        // =====================================================

        if (timer != null)
        {
            timer.StopTimer();

            Debug.Log(
                "Timer stopped because scarf was collected."
            );
        }

        // =====================================================
        // FIND UI IF NEEDED
        // =====================================================

        if (scarfText == null ||
            timerText == null)
        {
            FindLevelUITexts();
        }

        // =====================================================
        // STOP EXISTING SCARF TEXT COROUTINE
        // =====================================================

        if (scarfTextCoroutine != null)
        {
            StopCoroutine(
                scarfTextCoroutine
            );
        }

        scarfTextCoroutine =
            StartCoroutine(
                ScarfCollectedRoutine()
            );
    }

    // =========================================================
    // SCARF ROUTINE
    // =========================================================

    private IEnumerator ScarfCollectedRoutine()
    {
        // =====================================================
        // STOP GAMEPLAY AUDIO
        // =====================================================

        StopGameplayAudio();

        // =====================================================
        // DISABLE CAT CONTROL
        // =====================================================

        SetCatControl(false);

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(false);
        }

        // =====================================================
        // SHOW SCARF TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.gameObject.SetActive(true);

            scarfText.text = "";

            Debug.Log(
                "Starting scarf text typewriter."
            );

            yield return StartCoroutine(
                TypewriterRoutine(
                    scarfText,
                    scarfMessage,
                    scarfTypewriterSpeed
                )
            );

            Debug.Log(
                "Scarf text typewriter finished."
            );

            yield return new WaitForSecondsRealtime(
                scarfTextDuration
            );
        }

        // =====================================================
        // FADE INTO IMAGE SEQUENCE
        // =====================================================

        if (fadeController != null)
        {
            Debug.Log(
                "Fading into scarf image sequence."
            );

            yield return StartCoroutine(
                fadeController.FadeOutRoutine()
            );
        }

        // =====================================================
        // DISABLE COLLECTED TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.gameObject.SetActive(false);

            Debug.Log(
                "Collected Text DISABLED."
            );
        }

        // =====================================================
        // DISABLE TIMER TEXT
        // =====================================================

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);

            Debug.Log(
                "TimerText DISABLED because image sequence started."
            );
        }

        // =====================================================
        // PLAY IMAGE SEQUENCE
        // =====================================================

        if (scarfImageSequence != null)
        {
            Debug.Log(
                "Starting scarf image sequence."
            );

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
        // FADE IN
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
    // TYPEWRITER ROUTINE
    // =========================================================

    private IEnumerator TypewriterRoutine(
        TMP_Text textComponent,
        string message,
        float characterSpeed)
    {
        if (textComponent == null)
            yield break;

        if (message == null)
            message = "";

        textComponent.text = "";

        characterSpeed =
            Mathf.Max(
                0.001f,
                characterSpeed
            );

        for (int i = 0;
             i < message.Length;
             i++)
        {
            textComponent.text += message[i];

            yield return new WaitForSecondsRealtime(
                characterSpeed
            );
        }
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

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeOutRoutine()
            );
        }

        Debug.Log(
            "Level remains active after scarf sequence."
        );

        yield return null;

        SetCatControl(false);

        if (scarfPanel != null)
        {
            scarfPanel.SetActive(true);
        }

        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        Debug.Log(
            "Scarf panel shown. " +
            "Cat controls remain disabled."
        );
    }

    // =========================================================
    // FALLBACK
    // =========================================================

    private void FinishScarfSequence()
    {
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
    // GAMEPLAY AUDIO
    // =========================================================

    private void FindInGameSound()
    {
        inGameSoundAudioSource = null;

        if (levelGameObject == null)
        {
            Debug.LogWarning(
                "GameManager: Level GameObject is not assigned."
            );

            return;
        }

        Transform soundTransform =
            FindChildRecursive(
                levelGameObject.transform,
                inGameSoundObjectName
            );

        if (soundTransform != null)
        {
            inGameSoundAudioSource =
                soundTransform.GetComponent<AudioSource>();

            if (inGameSoundAudioSource != null)
            {
                Debug.Log(
                    "GameManager: InGameSound AudioSource found."
                );
            }
            else
            {
                Debug.LogWarning(
                    "GameManager: InGameSound found, " +
                    "but it has no AudioSource."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "GameManager: Could not find InGameSound " +
                "inside Level."
            );
        }
    }

    // =========================================================
    // FIND CHILD RECURSIVELY
    // =========================================================

    private Transform FindChildRecursive(
        Transform parent,
        string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName)
                return child;

            Transform result =
                FindChildRecursive(
                    child,
                    objectName
                );

            if (result != null)
                return result;
        }

        return null;
    }

    // =========================================================
    // STOP GAMEPLAY AUDIO
    // =========================================================

    private void StopGameplayAudio()
    {
        if (catController != null)
        {
            catController.SetCatAudioEnabled(false);
        }

        FindInGameSound();

        if (inGameSoundAudioSource != null)
        {
            inGameSoundWasPlaying =
                inGameSoundAudioSource.isPlaying;

            inGameSoundAudioSource.Stop();

            inGameSoundAudioSource.enabled = false;

            Debug.Log(
                "InGameSound DISABLED."
            );
        }
    }

    // =========================================================
    // RESUME GAMEPLAY AUDIO
    // =========================================================

    private void ResumeGameplayAudio()
    {
        if (catController != null)
        {
            catController.SetCatAudioEnabled(true);
        }

        if (inGameSoundAudioSource != null)
        {
            inGameSoundAudioSource.enabled = true;

            if (inGameSoundWasPlaying)
            {
                inGameSoundAudioSource.Play();
            }

            Debug.Log(
                "InGameSound ENABLED."
            );
        }
    }

    // =========================================================
    // CLOSE SCARF PANEL
    // =========================================================

    public void CloseScarfPanel()
    {
        if (scarfPanel != null)
        {
            scarfPanel.SetActive(false);
        }

        ResumeGameplayAudio();

        SetCatControl(true);

        Debug.Log(
            "Scarf panel closed. " +
            "Gameplay audio resumed."
        );
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "Level_Selection"
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

        SetCatControl(false);

        if (levelGameObject != null)
        {
            levelGameObject.SetActive(false);
        }

        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(true);
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(true);
            cutsceneCamera.enabled = true;
        }

        Debug.Log(
            "CUTSCENE CAMERA ENABLED"
        );

        if (cameraCutsceneMover == null)
        {
            cameraCutsceneMover =
                FindFirstObjectByType<CameraCutsceneMover>();
        }

        if (cameraCutsceneMover != null)
        {
            Debug.Log(
                "STARTING CAMERA MOVEMENT"
            );

            cameraCutsceneMover.StartCutscene();
        }
        else
        {
            Debug.LogError(
                "CameraCutsceneMover NOT FOUND!"
            );

            StartCoroutine(
                CameraCutsceneFinishedRoutine()
            );
        }

        yield break;
    }

    // =========================================================
    // CAMERA CUTSCENE FINISHED
    // =========================================================

    public void CameraCutsceneFinished()
    {
        if (cameraFinishedCoroutine != null)
        {
            return;
        }

        cameraFinishedCoroutine =
            StartCoroutine(
                CameraCutsceneFinishedRoutine()
            );
    }

    // =========================================================
    // CAMERA FINISHED ROUTINE
    // =========================================================

    private IEnumerator CameraCutsceneFinishedRoutine()
    {
        Debug.Log(
            "GAME MANAGER: CAMERA CUTSCENE FINISHED"
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
        // CUTSCENE CAMERA OFF
        // =====================================================

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        // =====================================================
        // CUTSCENE OBJECT OFF
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
            Debug.Log(
                "ENABLING LEVEL WHILE SCREEN IS BLACK"
            );

            levelGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "GameManager: Level GameObject is not assigned."
            );
        }

        // =====================================================
        // FIND LEVEL UI REFERENCES
        // =====================================================

        FindLevelUITexts();

        // =====================================================
        // FIND IN-GAME SOUND
        // =====================================================

        FindInGameSound();

        // =====================================================
        // INITIALIZE LEVEL
        // =====================================================

        int frames =
            Mathf.Max(
                1,
                levelInitializeFrames
            );

        Debug.Log(
            "INITIALIZING LEVEL FOR " +
            frames +
            " FRAMES"
        );

        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        // =====================================================
        // FADE INTO GAMEPLAY
        // =====================================================

        if (fadeController != null)
        {
            Debug.Log(
                "CUTSCENE: FADING INTO GAMEPLAY"
            );

            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        // =====================================================
        // CUTSCENE END TYPEWRITER
        // =====================================================

        if (cutsceneEndTextCoroutine != null)
        {
            StopCoroutine(
                cutsceneEndTextCoroutine
            );
        }

        cutsceneEndTextCoroutine =
            StartCoroutine(
                CutsceneEndTextRoutine()
            );

        // =====================================================
        // WAIT FOR TYPEWRITER + 1 SECOND
        // =====================================================

        yield return cutsceneEndTextCoroutine;

        cutsceneEndTextCoroutine = null;

        // =====================================================
        // START TIMER
        // =====================================================

        if (timer != null)
        {
            timer.StartTimer();

            Debug.Log(
                "TIMER STARTED AFTER " +
                "CUTSCENE END TYPEWRITER FINISHED"
            );
        }
        else
        {
            Debug.LogWarning(
                "GameManager: Timer reference is missing."
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
        cameraFinishedCoroutine = null;
    }

    // =========================================================
    // CUTSCENE END TYPEWRITER
    // =========================================================

    private IEnumerator CutsceneEndTextRoutine()
    {
        if (cutsceneEndText == null)
        {
            Debug.LogWarning(
                "GameManager: TypeWritter reference is missing."
            );

            yield break;
        }

        // =====================================================
        // SHOW TEXT
        // =====================================================

        cutsceneEndText.gameObject.SetActive(true);

        // =====================================================
        // TYPEWRITER
        // =====================================================

        Debug.Log(
            "Starting cutscene end text typewriter."
        );

        yield return StartCoroutine(
            TypewriterRoutine(
                cutsceneEndText,
                cutsceneEndMessage,
                typewriterSpeed
            )
        );

        Debug.Log(
            "Cutscene end text typewriter finished."
        );

        // =====================================================
        // WAIT 1 SECOND
        // =====================================================

        yield return new WaitForSecondsRealtime(
            cutsceneEndDisableDelay
        );

        // =====================================================
        // DISABLE TYPEWRITTER
        // =====================================================

        cutsceneEndText.gameObject.SetActive(false);

        Debug.Log(
            "TypeWritter DISABLED after " +
            cutsceneEndDisableDelay +
            " second."
        );
    }
}