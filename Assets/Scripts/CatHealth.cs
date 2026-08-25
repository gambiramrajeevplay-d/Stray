using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [SerializeField]
    private float currentHealth;

    [Header("Health UI")]
    [Tooltip("Image used as the health fill.")]
    public Image healthFillImage;

    [Tooltip("Text showing the current health.")]
    public TMP_Text healthText;

    [Header("Death")]
    [Tooltip("Optional panel shown when the cat dies.")]
    public GameObject deathPanel;

    [Tooltip("Disable cat movement when the cat dies.")]
    public bool disableControlOnDeath = true;

    [Header("Settings")]
    public bool destroyOnDeath = false;

    [Tooltip("Delay before destroying the cat.")]
    public float destroyDelay = 1f;

    private CatController catController;

    private bool isDead = false;

    // =========================================================
    // PUBLIC
    // =========================================================

    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    public bool IsDead => isDead;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        catController =
            GetComponent<CatController>();

        currentHealth =
            maxHealth;

        // =====================================================
        // DEATH PANEL
        // =====================================================

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // =====================================================
        // HEALTH UI
        // =====================================================

        UpdateHealthUI();
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (damage <= 0f)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        Debug.Log(
            "Cat took " +
            damage +
            " damage. Health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        UpdateHealthUI();

        // =====================================================
        // DEATH
        // =====================================================

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // =========================================================
    // HEAL
    // =========================================================

    public void Heal(float amount)
    {
        if (isDead)
            return;

        if (amount <= 0f)
            return;

        currentHealth += amount;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        UpdateHealthUI();

        Debug.Log(
            "Cat healed by " +
            amount +
            ". Health: " +
            currentHealth +
            "/" +
            maxHealth
        );
    }

    // =========================================================
    // FULL HEAL
    // =========================================================

    public void FullHeal()
    {
        if (isDead)
            return;

        currentHealth =
            maxHealth;

        UpdateHealthUI();
    }

    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        currentHealth = 0f;

        UpdateHealthUI();

        Debug.Log(
            "CAT DIED!"
        );

        // =====================================================
        // DISABLE CAT CONTROL
        // =====================================================

        if (disableControlOnDeath &&
            catController != null)
        {
            catController.SetControl(false);
        }

        // =====================================================
        // DEATH PANEL
        // =====================================================

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        // =====================================================
        // DESTROY
        // =====================================================

        if (destroyOnDeath)
        {
            Destroy(
                gameObject,
                destroyDelay
            );
        }
    }

    // =========================================================
    // UPDATE HEALTH UI
    // =========================================================

    private void UpdateHealthUI()
    {
        // =====================================================
        // HEALTH FILL IMAGE
        // =====================================================

        if (healthFillImage != null)
        {
            if (maxHealth > 0f)
            {
                healthFillImage.fillAmount =
                    currentHealth / maxHealth;
            }
            else
            {
                healthFillImage.fillAmount = 0f;
            }
        }

        // =====================================================
        // HEALTH TEXT
        // =====================================================

        if (healthText != null)
        {
            healthText.text =
                Mathf.CeilToInt(
                    currentHealth
                ).ToString();
        }
    }

    // =========================================================
    // RESET HEALTH
    // =========================================================

    public void ResetHealth()
    {
        isDead = false;

        currentHealth =
            maxHealth;

        if (catController != null)
        {
            catController.SetControl(true);
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        UpdateHealthUI();
    }

    // =========================================================
    // SET HEALTH
    // =========================================================

    public void SetHealth(float value)
    {
        if (isDead)
            return;

        currentHealth =
            Mathf.Clamp(
                value,
                0f,
                maxHealth
            );

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
}