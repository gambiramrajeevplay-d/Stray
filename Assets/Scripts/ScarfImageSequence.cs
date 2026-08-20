using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScarfImageSequence : MonoBehaviour
{
    [Header("UI")]
    public GameObject sequencePanel;
    public Image sequenceImage;

    [Header("Images")]
    public Sprite[] images;

    [Header("Timing")]
    public float imageDuration = 2f;

    [Header("Fade Controller")]
    public FadeController fadeController;

    [Header("Sequence")]
    public bool playOnce = true;

    private bool hasPlayed = false;
    private Coroutine sequenceCoroutine;

    private void Awake()
    {
        // Hide sequence panel initially.
        if (sequencePanel != null)
        {
            sequencePanel.SetActive(false);
        }

        // Find FadeController automatically.
        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<FadeController>();

            if (fadeController != null)
            {
                Debug.Log(
                    "ScarfImageSequence: FadeController found automatically."
                );
            }
            else
            {
                Debug.LogWarning(
                    "ScarfImageSequence: FadeController not found."
                );
            }
        }
    }

    // =========================================================
    // PLAY SEQUENCE
    // =========================================================

    public void PlaySequence()
    {
        if (playOnce && hasPlayed)
        {
            Debug.Log(
                "ScarfImageSequence: Sequence already played."
            );

            return;
        }

        if (images == null || images.Length == 0)
        {
            Debug.LogWarning(
                "ScarfImageSequence: No images assigned."
            );

            return;
        }

        if (sequencePanel == null)
        {
            Debug.LogWarning(
                "ScarfImageSequence: Sequence Panel is missing."
            );

            return;
        }

        if (sequenceImage == null)
        {
            Debug.LogWarning(
                "ScarfImageSequence: Sequence Image is missing."
            );

            return;
        }

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        sequenceCoroutine =
            StartCoroutine(
                PlaySequenceRoutine()
            );
    }

    // =========================================================
    // SEQUENCE
    // =========================================================

    private IEnumerator PlaySequenceRoutine()
    {
        hasPlayed = true;

        // =====================================================
        // SHOW PANEL
        // =====================================================

        sequencePanel.SetActive(true);

        Debug.Log(
            "Scarf image sequence started."
        );

        // =====================================================
        // FIRST IMAGE
        // =====================================================

        sequenceImage.sprite = images[0];

        // First image is immediately visible.
        sequenceImage.enabled = true;

        // Fade from black into first image.
        if (fadeController != null)
        {
            yield return StartCoroutine(
                fadeController.FadeInRoutine()
            );
        }

        // =====================================================
        // SHOW FIRST IMAGE
        // =====================================================

        Debug.Log(
            "Showing scarf image 1/" +
            images.Length
        );

        yield return new WaitForSecondsRealtime(
            imageDuration
        );

        // =====================================================
        // NEXT IMAGES
        // =====================================================

        for (int i = 1; i < images.Length; i++)
        {
            // -------------------------------------------------
            // FADE SCREEN TO BLACK
            // -------------------------------------------------

            if (fadeController != null)
            {
                Debug.Log(
                    "Fading to black before image " +
                    (i + 1)
                );

                yield return StartCoroutine(
                    fadeController.FadeOutRoutine()
                );
            }

            // -------------------------------------------------
            // CHANGE IMAGE WHILE SCREEN IS BLACK
            // -------------------------------------------------

            sequenceImage.sprite = images[i];

            Debug.Log(
                "Changed to scarf image " +
                (i + 1) +
                "/" +
                images.Length
            );

            // -------------------------------------------------
            // FADE SCREEN BACK IN
            // -------------------------------------------------

            if (fadeController != null)
            {
                yield return StartCoroutine(
                    fadeController.FadeInRoutine()
                );
            }

            // -------------------------------------------------
            // SHOW IMAGE
            // -------------------------------------------------

            yield return new WaitForSecondsRealtime(
                imageDuration
            );
        }

        // =====================================================
        // SEQUENCE FINISHED
        // =====================================================

        sequencePanel.SetActive(false);

        sequenceCoroutine = null;

        Debug.Log(
            "Scarf image sequence finished."
        );

        // Tell GameManager.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScarfSequenceFinished();
        }
    }
}