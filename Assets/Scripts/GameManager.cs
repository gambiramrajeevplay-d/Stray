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
    [Tooltip("Timer that starts after the cutscene end text finishes.")]
    public Timer timer;

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
    [Tooltip("Text displayed after the cutscene and level appears.")]
    [SerializeField] private TMP_Text cutsceneEndText;

    [Tooltip("Message displayed after the cutscene.")]
    [SerializeField] private string cutsceneEndMessage = "Let's go!";

    [Tooltip("Time between each character.")]
    [SerializeField] private float typewriterSpeed = 0.05f;

    [Tooltip("How long the complete text remains visible.")]
    [SerializeField] private float cutsceneEndTextDuration = 3f;

    // =========================================================
    // LEVEL
    // =========================================================

    [Header("Level")]
    [Tooltip("The gameplay level GameObject.")]
    public GameObject levelGameObject;

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
                    "GameManager: Timer could not be found."
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
            scarfText.text = "";
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
        // CUTSCENE END TEXT
        // =====================================================

        if (cutsceneEndText != null)
        {
            cutsceneEndText.text = "";
            cutsceneEndText.gameObject.SetActive(false);
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

            // Clear previous text
            scarfText.text = "";

            Debug.Log(
                "Starting scarf text typewriter."
            );

            // Typewriter effect
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

            // Keep complete text visible
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
        // HIDE SCARF TEXT
        // =====================================================

        if (scarfText != null)
        {
            scarfText.gameObject.SetActive(false);
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
        // =====================================================
        // STOP CAT AUDIO
        // =====================================================

        if (catController != null)
        {
            catController.SetCatAudioEnabled(false);
        }

        // =====================================================
        // FIND IN-GAME SOUND
        // =====================================================

        FindInGameSound();

        // =====================================================
        // STOP IN-GAME SOUND
        // =====================================================

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
        // =====================================================
        // CAT AUDIO
        // =====================================================

        if (catController != null)
        {
            catController.SetCatAudioEnabled(true);
        }

        // =====================================================
        // IN-GAME SOUND
        // =====================================================

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
        // WAIT FOR TYPEWRITER + DISPLAY TIME
        // =====================================================

        yield return cutsceneEndTextCoroutine;

        cutsceneEndTextCoroutine = null;

        // =====================================================
        // START TIMER AFTER TEXT FINISHES
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
                "Cutscene End Text is not assigned."
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
        // KEEP TEXT VISIBLE
        // =====================================================

        yield return new WaitForSecondsRealtime(
            cutsceneEndTextDuration
        );

        // =====================================================
        // HIDE TEXT
        // =====================================================

        cutsceneEndText.gameObject.SetActive(false);
    }
}