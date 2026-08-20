using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [Header("Fade Image")]
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: Fade Image is NOT assigned!");
            return;
        }

        // Make sure the fade image is active.
        fadeImage.gameObject.SetActive(true);

        // Start completely transparent.
        SetAlpha(0f);
    }

    // =========================================================
    // PUBLIC FADE FUNCTIONS
    // =========================================================

    public IEnumerator FadeOutRoutine()
    {
        // Transparent -> Black
        yield return FadeTo(1f);
    }

    public IEnumerator FadeInRoutine()
    {
        // Black -> Transparent
        yield return FadeTo(0f);
    }

    public void FadeOut()
    {
        StartCoroutine(FadeTo(1f));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeTo(0f));
    }

    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: Fade Image is missing!");
            yield break;
        }

        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / fadeDuration;

            // Smooth fade
            t = Mathf.SmoothStep(0f, 1f, t);

            float alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            SetAlpha(alpha);

            yield return null;
        }

        // Make absolutely sure we reach the target.
        SetAlpha(targetAlpha);

        Debug.Log(
            "Fade completed. Alpha = " + targetAlpha
        );
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;

        color.a = alpha;

        fadeImage.color = color;
    }
}