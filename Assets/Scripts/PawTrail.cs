using System.Collections;
using UnityEngine;

public class PawTrail : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";

    [Header("Paw Sprites")]
    [Tooltip("Sprite shown when the cat gets close.")]
    public Sprite normalSprite;

    [Tooltip("Sprite shown when the cat passes through the paw.")]
    public Sprite passedSprite;

    [Header("Detection")]
    [Tooltip("Distance at which the paw becomes visible.")]
    public float detectionDistance = 3f;

    [Header("Fade In")]
    public float fadeInDuration = 0.5f;

    [Header("Fade Out")]
    public float fadeOutDuration = 1f;

    [Header("Settings")]
    [Tooltip("If true, this paw can only be activated once.")]
    public bool playOnce = true;

    private Transform player;
    private SpriteRenderer spriteRenderer;

    private bool hasBeenActivated = false;
    private bool hasBeenPassed = false;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError(
                "PawTrail: SpriteRenderer is missing on " + gameObject.name
            );
        }

        // Start invisible.
        SetAlpha(0f);

        // Set normal sprite.
        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }

    private void Start()
    {
        // Find player automatically.
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "PawTrail: Could not find player with tag: " + playerTag
            );
        }
    }

    private void Update()
    {
        if (hasBeenPassed)
            return;

        if (player == null)
            return;

        // ==========================================
        // CHECK DISTANCE
        // ==========================================

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= detectionDistance)
        {
            ShowPaw();
        }
    }

    // =========================================================
    // SHOW PAW
    // =========================================================

    private void ShowPaw()
    {
        if (hasBeenActivated)
            return;

        hasBeenActivated = true;

        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }

        Debug.Log(
            "Paw activated: " + gameObject.name
        );

        StartFade(1f, fadeInDuration);
    }

    // =========================================================
    // CAT PASSES THROUGH PAW
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPassed)
            return;

        if (!other.CompareTag(playerTag))
            return;

        // Don't allow the paw to trigger
        // before it has become visible.
        if (!hasBeenActivated)
            return;

        hasBeenPassed = true;

        Debug.Log(
            "Cat passed through paw: " + gameObject.name
        );

        // Change sprite.
        if (passedSprite != null)
        {
            spriteRenderer.sprite = passedSprite;
        }

        // Fade out.
        StartFade(0f, fadeOutDuration);
    }

    // =========================================================
    // FADE
    // =========================================================

    private void StartFade(
        float targetAlpha,
        float duration
    )
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeRoutine(targetAlpha, duration)
        );
    }

    private IEnumerator FadeRoutine(
        float targetAlpha,
        float duration
    )
    {
        if (spriteRenderer == null)
            yield break;

        float startAlpha = spriteRenderer.color.a;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            float alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);

        fadeCoroutine = null;

        // If faded completely, disable renderer.
        if (targetAlpha <= 0f)
        {
            spriteRenderer.enabled = false;
        }
    }

    // =========================================================
    // ALPHA
    // =========================================================

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;

        color.a = alpha;

        spriteRenderer.color = color;

        // Enable renderer if we are showing the paw.
        if (alpha > 0f)
        {
            spriteRenderer.enabled = true;
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionDistance
        );
    }
}