
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float closeSpeed = 8f;
    [SerializeField] private float openAngle = 90f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Door Audio")]
    [Tooltip("Sound played once when the door starts opening.")]
    [SerializeField] private AudioClip doorOpenClip;

    [Tooltip("Volume of the door opening sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float doorOpenVolume = 1f;

    private AudioSource doorAudioSource;

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;

    private void Start()
    {
        // Store the closed rotation
        closedRotation = transform.rotation;

        // Calculate the open rotation
        targetRotation = closedRotation *
                         Quaternion.Euler(0f, openAngle, 0f);

        // Create a new AudioSource automatically
        CreateAudioSource();
    }

    private void CreateAudioSource()
    {
        GameObject audioObject = new GameObject("DoorAudio");
        audioObject.transform.SetParent(transform);
        audioObject.transform.localPosition = Vector3.zero;

        doorAudioSource = audioObject.AddComponent<AudioSource>();

        doorAudioSource.playOnAwake = false;
        doorAudioSource.loop = false;
        doorAudioSource.volume = doorOpenVolume;
        doorAudioSource.spatialBlend = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        OpenDoor();
    }

    public void OpenDoor()
    {
        // Prevent the sound from playing again
        // if the door is already open.
        if (isOpen)
            return;

        isOpen = true;

        PlayDoorOpenSound();
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    private void PlayDoorOpenSound()
    {
        if (doorAudioSource == null || doorOpenClip == null)
            return;

        doorAudioSource.Stop();
        doorAudioSource.clip = doorOpenClip;
        doorAudioSource.volume = doorOpenVolume;
        doorAudioSource.Play();
    }

    private void Update()
    {
        Quaternion desiredRotation = isOpen
            ? targetRotation
            : closedRotation;

        float speed = isOpen
            ? openSpeed
            : closeSpeed;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            speed * Time.deltaTime
        );
    }
}