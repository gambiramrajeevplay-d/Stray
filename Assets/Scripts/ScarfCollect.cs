
using UnityEngine;

public class ScarfCollect : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Collection Particle Effect")]
    [Tooltip("Particle effect prefab that plays when the scarf is collected.")]
    [SerializeField] private ParticleSystem collectionParticle;

    [Tooltip("Point where the collection particle effect will spawn.")]
    [SerializeField] private Transform particleSpawnPoint;

    [Header("Collection Audio")]
    [Tooltip("Audio clip that plays when the scarf is collected.")]
    [SerializeField] private AudioClip collectionAudioClip;

    [Tooltip("Volume of the collection sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float audioVolume = 1f;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag(playerTag))
            return;

        collected = true;

        Debug.Log("Scarf collected by player.");

        // =====================================================
        // PLAY COLLECTION PARTICLE EFFECT
        // =====================================================

        if (collectionParticle != null)
        {
            // Use the assigned spawn point.
            // If no spawn point is assigned, use the scarf position.
            Vector3 spawnPosition = particleSpawnPoint != null
                ? particleSpawnPoint.position
                : transform.position;

            Quaternion spawnRotation = particleSpawnPoint != null
                ? particleSpawnPoint.rotation
                : Quaternion.identity;

            ParticleSystem particle = Instantiate(
                collectionParticle,
                spawnPosition,
                spawnRotation
            );

            // Destroy the particle after it finishes.
            Destroy(
                particle.gameObject,
                particle.main.duration +
                particle.main.startLifetime.constantMax
            );

            Debug.Log("Scarf collection particle played.");
        }
        else
        {
            Debug.LogWarning(
                "ScarfCollect: Collection Particle is not assigned!"
            );
        }

        // =====================================================
        // PLAY COLLECTION AUDIO
        // =====================================================

        if (collectionAudioClip != null)
        {
            // Create a temporary GameObject for the audio.
            GameObject audioObject = new GameObject(
                "ScarfCollectionAudio"
            );

            // Place it at the scarf/particle position.
            Vector3 audioPosition = particleSpawnPoint != null
                ? particleSpawnPoint.position
                : transform.position;

            audioObject.transform.position = audioPosition;

            // Create AudioSource automatically.
            AudioSource audioSource =
                audioObject.AddComponent<AudioSource>();

            audioSource.clip = collectionAudioClip;
            audioSource.volume = audioVolume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            // Play the sound.
            audioSource.Play();

            // Destroy the temporary AudioSource GameObject
            // after the clip finishes.
            Destroy(
                audioObject,
                collectionAudioClip.length
            );

            Debug.Log("Scarf collection audio played.");
        }
        else
        {
            Debug.LogWarning(
                "ScarfCollect: Collection Audio Clip is not assigned!"
            );
        }

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