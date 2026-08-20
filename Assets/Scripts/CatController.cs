using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CatController : MonoBehaviour
{
    [Header("Control")]
    public bool canControl = true;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float velocitySmoothTime = 0.18f;

    [Header("Turning")]
    public float turnSpeed = 120f;
    public float turnSmoothTime = 0.08f;

    [Header("Jump")]
    public float jumpHeight = 1.8f;
    public float fallGravityMultiplier = 2.2f;
    public float lowJumpGravityMultiplier = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundMask;

    [Header("Automatic Stair Detection")]
    public Transform stairRayOrigin;
    public float stairRayDistance = 1.2f;
    public LayerMask stairMask;
    public float stairRayHeight = 0.6f;
    public float stairEndDelay = 0.25f;
    public float stairCooldown = 0.4f;

    [Header("Automatic Stair Movement")]
    public float stairUpAngle = -25f;

    [Tooltip("Fixed Y rotation while the cat is climbing stairs.")]
    public float stairLockYRotation = -90f;

    [Tooltip("How quickly the cat rotates to the stair lock rotation.")]
    public float stairRotationSmoothTime = 0.15f;

    public float stairMoveSpeed = 3.5f;
    public float stairVerticalSmoothTime = 0.08f;

    [Header("Animator")]
    public Animator animator;
    public float stairAnimationSpeed = 3.5f;

    [Header("Cube Pushing")]
    [SerializeField] private float pushMinSpeed = 0.15f;

    [Header("Animation Speed Matching")]
    [SerializeField] private float animationReferenceSpeed = 4f;
    [SerializeField] private float minimumAnimationSpeed = 0.5f;
    [SerializeField] private float maximumAnimationSpeed = 1.5f;

    // =========================================================
    // COMPONENTS
    // =========================================================

    private Rigidbody rb;
    private CapsuleCollider catCollider;
    private Transform tf;

    // =========================================================
    // GROUND
    // =========================================================

    private bool isGrounded;
    private float baseGravityY;

    // =========================================================
    // INPUT
    // =========================================================

    private float inputX;
    private float inputZ;
    private bool jumpQueued;

    // =========================================================
    // MOVEMENT
    // =========================================================

    private float smoothForwardSpeed;
    private float forwardSpeedSmoothRef;

    // =========================================================
    // TURNING
    // =========================================================

    private float smoothTurnSpeed;
    private float turnSpeedSmoothRef;

    // =========================================================
    // STAIR
    // =========================================================

    private float targetStairAngle;
    private float currentStairAngle;
    private float stairAngleVelocity;

    private bool isAutoClimbing;

    private float stairNotDetectedTimer;
    private float stairCooldownTimer;

    private float stairVerticalVelocity;

    // =========================================================
    // STAIR Y ROTATION
    // =========================================================

    private float stairYawVelocity;

    // =========================================================
    // BOX RIDING
    // =========================================================

    private PushableCube currentBox;

    // =========================================================
    // ANIMATOR HASHES
    // =========================================================

    private static readonly int VelXHash =
        Animator.StringToHash("VelocityX");

    private static readonly int VelZHash =
        Animator.StringToHash("VelocityZ");

    private static readonly int GroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    // =========================================================
    // PUBLIC
    // =========================================================

    public float InputX => inputX;
    public float InputZ => inputZ;

    public bool IsAutoClimbing => isAutoClimbing;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        catCollider = GetComponent<CapsuleCollider>();
        tf = transform;

        rb.freezeRotation = true;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.useGravity = false;

        baseGravityY =
            Physics.gravity.y;

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        currentStairAngle = 0f;
        targetStairAngle = 0f;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (stairCooldownTimer > 0f)
        {
            stairCooldownTimer -=
                Time.deltaTime;
        }

        // =====================================================
        // CONTROL LOCK
        // =====================================================

        if (!canControl)
        {
            inputX = 0f;
            inputZ = 0f;
            jumpQueued = false;

            if (isAutoClimbing)
            {
                FinishAutoClimb();
            }

            UpdateAnimator();

            return;
        }

        // =====================================================
        // STAIR DETECTION
        // =====================================================

        CheckForStairs();

        // =====================================================
        // INPUT
        // =====================================================

        if (isAutoClimbing)
        {
            // Completely lock player movement input.
            inputX = 0f;

            // Automatically move forward.
            inputZ = 1f;

            // No jumping while climbing.
            jumpQueued = false;
        }
        else
        {
            inputX =
                Input.GetAxisRaw("Horizontal");

            inputZ =
                Input.GetAxisRaw("Vertical");

            // No backward movement.
            if (inputZ < 0f)
            {
                inputZ = 0f;
            }

            // =================================================
            // SINGLE JUMP
            // =================================================

            if (isGrounded &&
                (Input.GetKeyDown(KeyCode.JoystickButton0) ||
                 Input.GetKeyDown(KeyCode.Space)))
            {
                jumpQueued = true;
            }
        }

        // =====================================================
        // ANIMATOR
        // =====================================================

        UpdateAnimator();
    }

    // =========================================================
    // ANIMATOR
    // =========================================================

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        if (isAutoClimbing)
        {
            animator.SetFloat(
                VelZHash,
                Mathf.Max(
                    smoothForwardSpeed,
                    stairAnimationSpeed
                )
            );

            animator.SetFloat(
                VelXHash,
                0f
            );

            animator.SetBool(
                GroundedHash,
                true
            );
        }
        else
        {
            animator.SetFloat(
                VelZHash,
                smoothForwardSpeed
            );

            animator.SetFloat(
                VelXHash,
                0f
            );

            animator.SetBool(
                GroundedHash,
                isGrounded
            );
        }

        // =====================================================
        // ANIMATION SPEED
        // =====================================================

        float actualSpeed =
            Mathf.Abs(
                smoothForwardSpeed
            );

        if (actualSpeed > 0.01f)
        {
            animator.speed =
                Mathf.Clamp(
                    actualSpeed /
                    animationReferenceSpeed,
                    minimumAnimationSpeed,
                    maximumAnimationSpeed
                );
        }
        else
        {
            animator.speed = 1f;
        }
    }

    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        // =====================================================
        // GROUND CHECK
        // =====================================================

        if (groundCheck != null)
        {
            isGrounded =
                Physics.CheckSphere(
                    groundCheck.position,
                    groundCheckRadius,
                    groundMask,
                    QueryTriggerInteraction.Ignore
                );
        }
        else
        {
            isGrounded = false;
        }

        // =====================================================
        // CONTROL LOCK
        // =====================================================

        if (!canControl)
        {
            smoothForwardSpeed =
                Mathf.SmoothDamp(
                    smoothForwardSpeed,
                    0f,
                    ref forwardSpeedSmoothRef,
                    velocitySmoothTime
                );

            smoothTurnSpeed =
                Mathf.SmoothDamp(
                    smoothTurnSpeed,
                    0f,
                    ref turnSpeedSmoothRef,
                    turnSmoothTime
                );

            Vector3 lockedVelocity =
                rb.velocity;

            lockedVelocity.x = 0f;
            lockedVelocity.z = 0f;

            if (!isGrounded)
            {
                float gravityMultiplier =
                    lockedVelocity.y < 0f
                        ? fallGravityMultiplier
                        : lowJumpGravityMultiplier;

                lockedVelocity.y +=
                    baseGravityY *
                    gravityMultiplier *
                    Time.fixedDeltaTime;
            }
            else
            {
                lockedVelocity.y = 0f;
            }

            rb.velocity =
                lockedVelocity;

            return;
        }

        // =====================================================
        // TARGET SPEED
        // =====================================================

        float targetSpeed;

        if (isAutoClimbing)
        {
            targetSpeed =
                stairMoveSpeed;
        }
        else
        {
            targetSpeed =
                inputZ * moveSpeed;
        }

        smoothForwardSpeed =
            Mathf.SmoothDamp(
                smoothForwardSpeed,
                targetSpeed,
                ref forwardSpeedSmoothRef,
                velocitySmoothTime
            );

        // =====================================================
        // YAW
        // =====================================================

        float currentYaw =
            rb.rotation.eulerAngles.y;

        if (isAutoClimbing)
        {
            // =================================================
            // LOCK Y ROTATION TO -90
            // =================================================

            currentYaw =
                Mathf.SmoothDampAngle(
                    currentYaw,
                    stairLockYRotation,
                    ref stairYawVelocity,
                    stairRotationSmoothTime
                );

            // Player turning is completely disabled.
            smoothTurnSpeed = 0f;
            turnSpeedSmoothRef = 0f;
        }
        else
        {
            // =================================================
            // NORMAL ROTATION
            // =================================================

            float targetTurn =
                inputX * turnSpeed;

            smoothTurnSpeed =
                Mathf.SmoothDamp(
                    smoothTurnSpeed,
                    targetTurn,
                    ref turnSpeedSmoothRef,
                    turnSmoothTime
                );

            if (Mathf.Abs(smoothTurnSpeed) > 0.01f)
            {
                currentYaw +=
                    smoothTurnSpeed *
                    Time.fixedDeltaTime;
            }
        }

        // =====================================================
        // STAIR ROTATION
        // =====================================================

        currentStairAngle =
            Mathf.SmoothDamp(
                currentStairAngle,
                targetStairAngle,
                ref stairAngleVelocity,
                stairRotationSmoothTime
            );

        Quaternion finalRotation =
            Quaternion.Euler(
                currentStairAngle,
                currentYaw,
                0f
            );

        rb.MoveRotation(
            finalRotation
        );

        // =====================================================
        // VELOCITY
        // =====================================================

        Vector3 velocity =
            rb.velocity;

        // =====================================================
        // AUTO STAIR MOVEMENT
        // =====================================================

        if (isAutoClimbing)
        {
            // Cat always moves in its locked forward
            // direction while climbing.
            Vector3 stairForward =
                finalRotation *
                Vector3.forward;

            Vector3 stairMovement =
                stairForward *
                smoothForwardSpeed;

            velocity.x =
                stairMovement.x;

            velocity.z =
                stairMovement.z;

            float targetVerticalVelocity =
                stairMovement.y;

            stairVerticalVelocity =
                Mathf.SmoothDamp(
                    stairVerticalVelocity,
                    targetVerticalVelocity,
                    ref stairVerticalVelocity,
                    stairVerticalSmoothTime
                );

            velocity.y =
                stairVerticalVelocity;
        }
        else
        {
            // =================================================
            // NORMAL MOVEMENT
            // =================================================

            Vector3 forward =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f
                ) * Vector3.forward;

            Vector3 movement =
                forward *
                smoothForwardSpeed;

            velocity.x =
                movement.x;

            velocity.z =
                movement.z;

            // =================================================
            // SINGLE JUMP
            // =================================================

            if (jumpQueued)
            {
                if (isGrounded)
                {
                    float gravity =
                        Mathf.Abs(
                            baseGravityY
                        );

                    velocity.y =
                        Mathf.Sqrt(
                            2f *
                            jumpHeight *
                            gravity
                        );

                    // =================================================
                    // JUMP ANIMATION
                    // =================================================

                    if (animator != null)
                    {
                        animator.ResetTrigger(
                            JumpHash
                        );

                        animator.SetTrigger(
                            JumpHash
                        );
                    }

                    currentBox = null;
                }

                jumpQueued = false;
            }

            // =================================================
            // GRAVITY
            // =================================================

            float gravityMultiplier =
                velocity.y < 0f
                    ? fallGravityMultiplier
                    : lowJumpGravityMultiplier;

            velocity.y +=
                baseGravityY *
                gravityMultiplier *
                Time.fixedDeltaTime;
        }

        // =====================================================
        // APPLY VELOCITY
        // =====================================================

        rb.velocity =
            velocity;
    }

    // =========================================================
    // PUSH BOX
    // =========================================================

    private void OnCollisionStay(
        Collision collision)
    {
        PushableCube cube =
            collision.collider
                .GetComponent<PushableCube>();

        if (cube == null)
            return;

        Vector3 catVelocity =
            rb.velocity;

        Vector3 horizontalVelocity =
            new Vector3(
                catVelocity.x,
                0f,
                catVelocity.z
            );

        bool standingOnBox = false;

        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint contact =
                collision.GetContact(i);

            if (contact.normal.y > 0.5f)
            {
                standingOnBox = true;
                break;
            }
        }

        if (standingOnBox)
        {
            currentBox = cube;
        }

        if (horizontalVelocity.magnitude <
            pushMinSpeed)
        {
            return;
        }

        Vector3 pushDirection =
            horizontalVelocity.normalized;

        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint contact =
                collision.GetContact(i);

            Vector3 directionToCube =
                contact.point -
                rb.position;

            directionToCube.y = 0f;

            if (directionToCube.sqrMagnitude <
                0.001f)
            {
                continue;
            }

            directionToCube.Normalize();

            float pushDot =
                Vector3.Dot(
                    pushDirection,
                    directionToCube
                );

            if (pushDot > 0.25f)
            {
                cube.Push(
                    pushDirection,
                    horizontalVelocity.magnitude
                );

                break;
            }
        }
    }

    // =========================================================
    // CLEAR BOX
    // =========================================================

    private void OnCollisionExit(
        Collision collision)
    {
        PushableCube cube =
            collision.collider
                .GetComponent<PushableCube>();

        if (cube != null &&
            cube == currentBox)
        {
            currentBox = null;
        }
    }

    // =========================================================
    // STAIRS
    // =========================================================

    private void CheckForStairs()
    {
        if (!canControl)
            return;

        if (stairCooldownTimer > 0f)
            return;

        Vector3 rayOrigin;

        if (stairRayOrigin != null)
        {
            rayOrigin =
                stairRayOrigin.position;
        }
        else
        {
            rayOrigin =
                tf.position +
                Vector3.up *
                stairRayHeight;
        }

        Vector3 rayDirection =
            tf.forward;

        bool stairDetected =
            Physics.Raycast(
                rayOrigin,
                rayDirection,
                out RaycastHit hit,
                stairRayDistance,
                stairMask,
                QueryTriggerInteraction.Ignore
            );

        if (!isAutoClimbing)
        {
            if (stairDetected)
            {
                StartAutoClimb();
            }

            return;
        }

        if (stairDetected)
        {
            stairNotDetectedTimer = 0f;
        }
        else
        {
            stairNotDetectedTimer +=
                Time.deltaTime;

            if (stairNotDetectedTimer >=
                stairEndDelay)
            {
                FinishAutoClimb();
            }
        }
    }

    // =========================================================
    // START STAIR CLIMB
    // =========================================================

    private void StartAutoClimb()
    {
        if (!canControl)
            return;

        if (isAutoClimbing)
            return;

        isAutoClimbing = true;

        stairNotDetectedTimer = 0f;

        inputX = 0f;
        inputZ = 1f;

        jumpQueued = false;

        // Reset turning.
        smoothTurnSpeed = 0f;
        turnSpeedSmoothRef = 0f;
        stairYawVelocity = 0f;

        if (smoothForwardSpeed < 0.1f)
        {
            smoothForwardSpeed = 0f;
        }

        targetStairAngle =
            stairUpAngle;

        stairVerticalVelocity = 0f;

        Debug.Log(
            "Cat started stair climb. Y rotation locked to " +
            stairLockYRotation
        );
    }

    // =========================================================
    // FINISH STAIR CLIMB
    // =========================================================

    private void FinishAutoClimb()
    {
        if (!isAutoClimbing)
            return;

        isAutoClimbing = false;

        stairNotDetectedTimer = 0f;

        targetStairAngle = 0f;

        stairVerticalVelocity = 0f;

        stairCooldownTimer =
            stairCooldown;

        forwardSpeedSmoothRef = 0f;

        // Reset turning so normal control
        // starts cleanly again.
        smoothTurnSpeed = 0f;
        turnSpeedSmoothRef = 0f;
        stairYawVelocity = 0f;

        inputX = 0f;
        inputZ = 0f;

        Debug.Log(
            "Cat finished stair climb. Normal movement restored."
        );
    }

    // =========================================================
    // CONTROL
    // =========================================================

    public void SetControl(bool value)
    {
        canControl = value;

        if (!canControl)
        {
            inputX = 0f;
            inputZ = 0f;
            jumpQueued = false;

            if (isAutoClimbing)
            {
                FinishAutoClimb();
            }

            Debug.Log(
                "Cat controls DISABLED."
            );
        }
        else
        {
            Debug.Log(
                "Cat controls ENABLED."
            );
        }
    }

    // =========================================================
    // MANUAL STAIR CONTROL
    // =========================================================

    public void SetStairAngle(
        float angle)
    {
        targetStairAngle = angle;
    }

    public void ResetStairAngle()
    {
        targetStairAngle = 0f;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        Vector3 rayOrigin;

        if (stairRayOrigin != null)
        {
            rayOrigin =
                stairRayOrigin.position;
        }
        else
        {
            rayOrigin =
                transform.position +
                Vector3.up *
                stairRayHeight;
        }

        Gizmos.color =
            isAutoClimbing
                ? Color.green
                : Color.red;

        Gizmos.DrawRay(
            rayOrigin,
            transform.forward *
            stairRayDistance
        );
    }
}