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

        Debug.Log("Scarf collected by player.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScarfCollected();
        }
        else
        {
            Debug.LogWarning(
                "ScarfCollect: GameManager.Instance is missing!"
            );
        }

        Destroy(gameObject);
    }
}