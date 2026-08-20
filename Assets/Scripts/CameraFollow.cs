using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Usually the Camera Pivot on the cat.")]
    public Transform target;

    public string targetTag = "Camera Pivot";

    [Header("Camera Position")]
    [Tooltip("Distance and height behind the cat.")]
    public Vector3 offset = new Vector3(0f, 2.5f, -4f);

    [Header("Position Smoothing")]
    [Tooltip("Lower = more camera lag. Higher = tighter follow.")]
    public float positionSmoothTime = 0.15f;

    [Header("Rotation")]
    [Tooltip("How quickly the camera rotates behind the cat.")]
    public float rotationSmoothTime = 0.15f;

    [Tooltip("Maximum camera pitch.")]
    public float fixedPitch = 15f;

    [Header("Look At")]
    [Tooltip("Point above the target that the camera looks at.")]
    public Vector3 lookAtOffset = new Vector3(0f, 0.8f, 0f);

    [Tooltip("How quickly the camera looks at the target.")]
    public float lookAtSmoothTime = 0.12f;

    [Header("Collision")]
    public bool avoidObstacles = true;

    public LayerMask obstacleMask;

    public float collisionRadius = 0.3f;

    private Vector3 positionVelocity;

    private float currentYaw;
    private float yawVelocity;

    private Vector3 currentLookDirection;
    private Vector3 lookDirectionVelocity;

    private bool initialized;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // ---------------------------------------------------------
        // FIND TARGET
        // ---------------------------------------------------------

        if (target == null)
        {
            GameObject found =
                GameObject.FindGameObjectWithTag(targetTag);

            if (found != null)
            {
                target = found.transform;
            }
            else
            {
                Debug.LogWarning(
                    $"CameraFollow: No object tagged '{targetTag}' found."
                );

                return;
            }
        }

        // ---------------------------------------------------------
        // INITIAL YAW
        // ---------------------------------------------------------

        currentYaw =
            target.eulerAngles.y;

        // ---------------------------------------------------------
        // INITIAL ROTATION
        // ---------------------------------------------------------

        Quaternion initialRotation =
            Quaternion.Euler(
                fixedPitch,
                currentYaw,
                0f
            );

        // ---------------------------------------------------------
        // INITIAL POSITION
        // ---------------------------------------------------------

        Vector3 desiredPosition =
            target.position +
            initialRotation * offset;

        transform.position =
            desiredPosition;

        // ---------------------------------------------------------
        // INITIAL LOOK DIRECTION
        // ---------------------------------------------------------

        Vector3 lookTarget =
            target.position +
            lookAtOffset;

        currentLookDirection =
            (lookTarget - transform.position).normalized;

        if (currentLookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    currentLookDirection,
                    Vector3.up
                );
        }

        initialized = true;
    }

    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (target == null || !initialized)
            return;

        // ---------------------------------------------------------
        // FOLLOW CAT ROTATION
        // ---------------------------------------------------------

        float targetYaw =
            target.eulerAngles.y;

        currentYaw =
            Mathf.SmoothDampAngle(
                currentYaw,
                targetYaw,
                ref yawVelocity,
                rotationSmoothTime
            );

        // ---------------------------------------------------------
        // CAMERA POSITION ROTATION
        // ---------------------------------------------------------

        Quaternion cameraPositionRotation =
            Quaternion.Euler(
                0f,
                currentYaw,
                0f
            );

        // ---------------------------------------------------------
        // CAMERA POSITION
        // ---------------------------------------------------------

        Vector3 desiredPosition =
            target.position +
            cameraPositionRotation * offset;

        // ---------------------------------------------------------
        // CAMERA COLLISION
        // ---------------------------------------------------------

        if (avoidObstacles)
        {
            Vector3 castOrigin =
                target.position +
                Vector3.up * lookAtOffset.y;

            Vector3 direction =
                desiredPosition - castOrigin;

            float distance =
                direction.magnitude;

            if (distance > 0.01f)
            {
                if (Physics.SphereCast(
                    castOrigin,
                    collisionRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    distance,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore))
                {
                    desiredPosition =
                        castOrigin +
                        direction.normalized *
                        Mathf.Max(
                            0.1f,
                            hit.distance - collisionRadius
                        );
                }
            }
        }

        // ---------------------------------------------------------
        // SMOOTH CAMERA POSITION
        // ---------------------------------------------------------

        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );

        // ---------------------------------------------------------
        // LOOK AT CAT
        // ---------------------------------------------------------

        Vector3 lookTarget =
            target.position +
            lookAtOffset;

        Vector3 desiredLookDirection =
            lookTarget -
            transform.position;

        if (desiredLookDirection.sqrMagnitude >
            0.001f)
        {
            desiredLookDirection.Normalize();

            currentLookDirection =
                Vector3.SmoothDamp(
                    currentLookDirection,
                    desiredLookDirection,
                    ref lookDirectionVelocity,
                    lookAtSmoothTime
                );

            if (currentLookDirection.sqrMagnitude >
                0.001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        currentLookDirection,
                        Vector3.up
                    );
            }
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 lookTarget =
            target.position +
            lookAtOffset;

        Gizmos.DrawWireSphere(
            lookTarget,
            0.15f
        );

        Gizmos.DrawLine(
            transform.position,
            lookTarget
        );
    }
}