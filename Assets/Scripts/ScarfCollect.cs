using UnityEngine;

public class ScarfCollect : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag(playerTag))
            return;

        collected = true;

        Debug.Log(
            "Scarf collected by player."
        );

        // =====================================================
        // STOP TIMER IMMEDIATELY
        // =====================================================

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.timer != null)
            {
                GameManager.Instance.timer.StopTimer();

                Debug.Log(
                    "Timer stopped because scarf was collected."
                );
            }

            // =================================================
            // START SCARF COLLECTION SEQUENCE
            // =================================================

            GameManager.Instance.ScarfCollected();
        }
        else
        {
            Debug.LogWarning(
                "ScarfCollect: GameManager.Instance is missing!"
            );
        }

        // =====================================================
        // DESTROY SCARF
        // =====================================================

        Destroy(gameObject);
    }
}