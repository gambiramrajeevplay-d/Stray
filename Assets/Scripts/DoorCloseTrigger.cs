using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    [SerializeField] private DoorOpen door;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        Debug.Log("Player entered closing trigger!");

        if (door != null)
        {
            door.CloseDoor();
        }
        else
        {
            Debug.LogError("Door reference is missing on DoorCloseTrigger!");
        }
    }
}