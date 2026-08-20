using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float closeSpeed = 8f;
    [SerializeField] private float openAngle = 90f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;

    private void Start()
    {
        closedRotation = transform.rotation;

        targetRotation = closedRotation *
                         Quaternion.Euler(0f, openAngle, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
            return;

        OpenDoor();
    }

    public void OpenDoor()
    {
        isOpen = true;

       // Debug.Log("Door Opening");
    }

    public void CloseDoor()
    {
        isOpen = false;

        //Debug.Log("Door Closing");
    }

    private void Update()
    {
        Quaternion desiredRotation = isOpen
            ? targetRotation
            : closedRotation;

        float speed = isOpen ? openSpeed : closeSpeed;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            speed * Time.deltaTime
        );
    }
}