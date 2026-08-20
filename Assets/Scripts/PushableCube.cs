using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PushableCube : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 3.5f;
    [SerializeField] private float maxPushSpeed = 3f;

    [Header("Smoothing")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;

    [Header("Physics")]
    [SerializeField] private float drag = 2f;

    [Header("Allowed Movement")]
    [SerializeField] private bool allowXMovement = true;
    [SerializeField] private bool allowZMovement = true;

    private Rigidbody rb;

    private Vector3 targetVelocity;

    // Velocity from the previous physics frame.
    private Vector3 previousPosition;

    private Vector3 surfaceVelocity;

    // =========================================================
    // START
    // =========================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();

        rb.mass = 5f;

        rb.drag = drag;

        rb.angularDrag = 5f;

        rb.useGravity = true;

        rb.isKinematic = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        // Don't let the cube tip over.
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        previousPosition =
            transform.position;
    }

    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        // -----------------------------------------------------
        // CALCULATE ACTUAL SURFACE VELOCITY
        // -----------------------------------------------------

        surfaceVelocity =
            (rb.position - previousPosition) /
            Time.fixedDeltaTime;

        surfaceVelocity.y = 0f;

        previousPosition =
            rb.position;

        // -----------------------------------------------------
        // SMOOTH MOVEMENT
        // -----------------------------------------------------

        Vector3 currentVelocity =
            rb.velocity;

        Vector3 horizontalVelocity =
            new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

        float smoothRate =
            targetVelocity.sqrMagnitude > 0.01f
                ? acceleration
                : deceleration;

        horizontalVelocity =
            Vector3.Lerp(
                horizontalVelocity,
                targetVelocity,
                smoothRate *
                Time.fixedDeltaTime
            );

        horizontalVelocity =
            Vector3.ClampMagnitude(
                horizontalVelocity,
                maxPushSpeed
            );

        rb.velocity =
            new Vector3(
                horizontalVelocity.x,
                rb.velocity.y,
                horizontalVelocity.z
            );

        // -----------------------------------------------------
        // SLOW TARGET VELOCITY DOWN
        // -----------------------------------------------------

        targetVelocity =
            Vector3.Lerp(
                targetVelocity,
                Vector3.zero,
                deceleration *
                Time.fixedDeltaTime
            );
    }

    // =========================================================
    // PUSH
    // =========================================================

    public void Push(
        Vector3 direction,
        float catSpeed)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        direction.Normalize();

        float speedFactor =
            Mathf.Clamp01(
                catSpeed / 2.5f
            );

        Vector3 pushVelocity =
            direction *
            pushForce *
            speedFactor;

        if (!allowXMovement)
        {
            pushVelocity.x = 0f;
        }

        if (!allowZMovement)
        {
            pushVelocity.z = 0f;
        }

        targetVelocity =
            Vector3.ClampMagnitude(
                pushVelocity,
                maxPushSpeed
            );
    }

    // =========================================================
    // GET BOX VELOCITY
    // =========================================================

    public Vector3 GetSurfaceVelocity()
    {
        return surfaceVelocity;
    }
}