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

    // ============================================================
    // AUDIO
    // ============================================================

    [Header("Footstep Audio")]
    [Tooltip("Assign the cat footstep sound clip.")]
    [SerializeField] private AudioClip footstepClip;

    [Tooltip("Time between each footstep.")]
    [SerializeField] private float footstepInterval = 0.35f;

    [Tooltip("Minimum movement speed required to play footsteps.")]
    [SerializeField] private float minimumFootstepSpeed = 0.1f;

    [Tooltip("Footstep volume.")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 1f;

    [Header("Meow Audio")]
    [Tooltip("Assign the cat meow sound clip.")]
    [SerializeField] private AudioClip meowClip;

    [Tooltip("Time between automatic meows.")]
    [SerializeField] private float meowInterval = 5f;

    [Tooltip("Meow volume.")]
    [Range(0f, 1f)]
    [SerializeField] private float meowVolume = 1f;

    // Automatically created AudioSources
    private AudioSource footstepAudioSource;
    private AudioSource meowAudioSource;

    // Controls whether cat audio is allowed to play
    private bool catAudioEnabled = true;

    // ============================================================
    // COMPONENTS
    // ============================================================

    private Rigidbody rb;
    private CapsuleCollider catCollider;
    private Transform tf;

    // GROUND
    private bool isGrounded;
    private float baseGravityY;

    // INPUT
    private float inputX;
    private float inputZ;
    private bool jumpQueued;

    // MOVEMENT
    private float smoothForwardSpeed;
    private float forwardSpeedSmoothRef;

    // TURNING
    private float smoothTurnSpeed;
    private float turnSpeedSmoothRef;

    // STAIR
    private float targetStairAngle;
    private float currentStairAngle;
    private float stairAngleVelocity;
    private bool isAutoClimbing;
    private float stairNotDetectedTimer;
    private float stairCooldownTimer;
    private float stairVerticalVelocity;

    // STAIR Y ROTATION
    private float stairYawVelocity;

    // BOX RIDING
    private PushableCube currentBox;

    // AUDIO TIMERS
    private float footstepTimer;
    private float meowTimer;

    // ANIMATOR HASHES
    private static readonly int VelXHash =
        Animator.StringToHash("VelocityX");

    private static readonly int VelZHash =
        Animator.StringToHash("VelocityZ");

    private static readonly int GroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    // PUBLIC
    public float InputX => inputX;
    public float InputZ => inputZ;
    public bool IsAutoClimbing => isAutoClimbing;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        CreateAudioSources();
    }

    // ============================================================
    // CREATE AUDIO SOURCES
    // ============================================================

    private void CreateAudioSources()
    {
        // ========================================================
        // FOOTSTEP AUDIO SOURCE
        // ========================================================

        Transform existingFootstep =
            transform.Find("FootstepAudio");

        if (existingFootstep != null)
        {
            footstepAudioSource =
                existingFootstep.GetComponent<AudioSource>();
        }

        if (footstepAudioSource == null)
        {
            GameObject footstepObject =
                new GameObject("FootstepAudio");

            footstepObject.transform.SetParent(transform);

            footstepObject.transform.localPosition =
                Vector3.zero;

            footstepObject.transform.localRotation =
                Quaternion.identity;

            footstepAudioSource =
                footstepObject.AddComponent<AudioSource>();
        }

        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = false;
        footstepAudioSource.spatialBlend = 1f;
        footstepAudioSource.volume = footstepVolume;

        // ========================================================
        // MEOW AUDIO SOURCE
        // ========================================================

        Transform existingMeow =
            transform.Find("MeowAudio");

        if (existingMeow != null)
        {
            meowAudioSource =
                existingMeow.GetComponent<AudioSource>();
        }

        if (meowAudioSource == null)
        {
            GameObject meowObject =
                new GameObject("MeowAudio");

            meowObject.transform.SetParent(transform);

            meowObject.transform.localPosition =
                Vector3.zero;

            meowObject.transform.localRotation =
                Quaternion.identity;

            meowAudioSource =
                meowObject.AddComponent<AudioSource>();
        }

        meowAudioSource.playOnAwake = false;
        meowAudioSource.loop = false;
        meowAudioSource.spatialBlend = 1f;
        meowAudioSource.volume = meowVolume;
    }

    // ============================================================
    // START
    // ============================================================

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
            animator = GetComponent<Animator>();

        // Make sure both AudioSources exist
        if (footstepAudioSource == null ||
            meowAudioSource == null)
        {
            CreateAudioSources();
        }

        footstepAudioSource.volume =
            footstepVolume;

        meowAudioSource.volume =
            meowVolume;

        // Start meow timer
        meowTimer =
            meowInterval;

        footstepTimer = 0f;

        currentStairAngle = 0f;
        targetStairAngle = 0f;
    }

    // ============================================================
    // ENABLE / DISABLE CAT AUDIO
    // ============================================================

    public void SetCatAudioEnabled(bool enabled)
    {
        catAudioEnabled = enabled;

        // --------------------------------------------------------
        // FOOTSTEP
        // --------------------------------------------------------

        if (footstepAudioSource != null)
        {
            if (!enabled)
            {
                footstepAudioSource.Stop();
            }

            footstepAudioSource.enabled =
                enabled;
        }

        // --------------------------------------------------------
        // MEOW
        // --------------------------------------------------------

        if (meowAudioSource != null)
        {
            if (!enabled)
            {
                meowAudioSource.Stop();
            }

            meowAudioSource.enabled =
                enabled;
        }

        // Reset footstep timer
        footstepTimer = 0f;

        // Restart meow countdown when audio is enabled again
        if (enabled)
        {
            meowTimer =
                meowInterval;
        }

        Debug.Log(
            "Cat Audio = " +
            (enabled ? "ENABLED" : "DISABLED")
        );
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (stairCooldownTimer > 0f)
            stairCooldownTimer -=
                Time.deltaTime;

        // Only update meow if audio is enabled
        if (catAudioEnabled)
        {
            UpdateMeowAudio();
        }

        if (!canControl)
        {
            inputX = 0f;
            inputZ = 0f;
            jumpQueued = false;

            if (isAutoClimbing)
                FinishAutoClimb();

            UpdateAnimator();

            if (catAudioEnabled)
                UpdateFootstepAudio();

            return;
        }

        CheckForStairs();

        if (isAutoClimbing)
        {
            inputX = 0f;
            inputZ = 1f;
            jumpQueued = false;
        }
        else
        {
            inputX =
                Input.GetAxisRaw("Horizontal");

            inputZ =
                Input.GetAxisRaw("Vertical");

            // Prevent moving backwards
            if (inputZ < 0f)
                inputZ = 0f;

            if (isGrounded &&
                (Input.GetKeyDown(
                    KeyCode.JoystickButton0) ||
                 Input.GetKeyDown(
                    KeyCode.Space)))
            {
                jumpQueued = true;
            }
        }

        UpdateAnimator();

        if (catAudioEnabled)
        {
            UpdateFootstepAudio();
        }
    }

    // ============================================================
    // FOOTSTEP AUDIO
    // ============================================================

    private void UpdateFootstepAudio()
    {
        if (!catAudioEnabled)
            return;

        if (footstepAudioSource == null ||
            footstepClip == null)
            return;

        bool isMoving =
            Mathf.Abs(
                smoothForwardSpeed) >
            minimumFootstepSpeed;

        // No footsteps while in air
        if (!isGrounded)
        {
            footstepTimer = 0f;
            return;
        }

        // No footsteps while standing
        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -=
            Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlayFootstep();

            footstepTimer =
                Mathf.Max(
                    0.05f,
                    footstepInterval);
        }
    }

    private void PlayFootstep()
    {
        if (!catAudioEnabled)
            return;

        if (footstepAudioSource == null ||
            footstepClip == null)
            return;

        footstepAudioSource.PlayOneShot(
            footstepClip,
            footstepVolume);
    }

    // ============================================================
    // MEOW AUDIO
    // ============================================================

    private void UpdateMeowAudio()
    {
        if (!catAudioEnabled)
            return;

        if (meowAudioSource == null ||
            meowClip == null)
            return;

        meowTimer -=
            Time.deltaTime;

        if (meowTimer <= 0f)
        {
            PlayMeow();

            meowTimer =
                Mathf.Max(
                    0.1f,
                    meowInterval);
        }
    }

    private void PlayMeow()
    {
        if (!catAudioEnabled)
            return;

        if (meowAudioSource == null ||
            meowClip == null)
            return;

        meowAudioSource.PlayOneShot(
            meowClip,
            meowVolume);
    }

    // ============================================================
    // ANIMATOR
    // ============================================================

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
                    stairAnimationSpeed));

            animator.SetFloat(
                VelXHash,
                0f);

            animator.SetBool(
                GroundedHash,
                true);
        }
        else
        {
            animator.SetFloat(
                VelZHash,
                smoothForwardSpeed);

            animator.SetFloat(
                VelXHash,
                0f);

            animator.SetBool(
                GroundedHash,
                isGrounded);
        }

        float actualSpeed =
            Mathf.Abs(
                smoothForwardSpeed);

        if (actualSpeed > 0.01f)
        {
            animator.speed =
                Mathf.Clamp(
                    actualSpeed /
                    animationReferenceSpeed,
                    minimumAnimationSpeed,
                    maximumAnimationSpeed);
        }
        else
        {
            animator.speed = 1f;
        }
    }

    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        if (groundCheck != null)
        {
            isGrounded =
                Physics.CheckSphere(
                    groundCheck.position,
                    groundCheckRadius,
                    groundMask,
                    QueryTriggerInteraction.Ignore);
        }
        else
        {
            isGrounded = false;
        }

        if (!canControl)
        {
            smoothForwardSpeed =
                Mathf.SmoothDamp(
                    smoothForwardSpeed,
                    0f,
                    ref forwardSpeedSmoothRef,
                    velocitySmoothTime);

            smoothTurnSpeed =
                Mathf.SmoothDamp(
                    smoothTurnSpeed,
                    0f,
                    ref turnSpeedSmoothRef,
                    turnSmoothTime);

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

        float targetSpeed;

        if (isAutoClimbing)
            targetSpeed =
                stairMoveSpeed;
        else
            targetSpeed =
                inputZ * moveSpeed;

        smoothForwardSpeed =
            Mathf.SmoothDamp(
                smoothForwardSpeed,
                targetSpeed,
                ref forwardSpeedSmoothRef,
                velocitySmoothTime);

        float currentYaw =
            rb.rotation.eulerAngles.y;

        if (isAutoClimbing)
        {
            currentYaw =
                Mathf.SmoothDampAngle(
                    currentYaw,
                    stairLockYRotation,
                    ref stairYawVelocity,
                    stairRotationSmoothTime);

            smoothTurnSpeed = 0f;
            turnSpeedSmoothRef = 0f;
        }
        else
        {
            float targetTurn =
                inputX * turnSpeed;

            smoothTurnSpeed =
                Mathf.SmoothDamp(
                    smoothTurnSpeed,
                    targetTurn,
                    ref turnSpeedSmoothRef,
                    turnSmoothTime);

            if (Mathf.Abs(
                smoothTurnSpeed) > 0.01f)
            {
                currentYaw +=
                    smoothTurnSpeed *
                    Time.fixedDeltaTime;
            }
        }

        currentStairAngle =
            Mathf.SmoothDamp(
                currentStairAngle,
                targetStairAngle,
                ref stairAngleVelocity,
                stairRotationSmoothTime);

        Quaternion finalRotation =
            Quaternion.Euler(
                currentStairAngle,
                currentYaw,
                0f);

        rb.MoveRotation(
            finalRotation);

        Vector3 velocity =
            rb.velocity;

        // ========================================================
        // STAIR MOVEMENT
        // ========================================================

        if (isAutoClimbing)
        {
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
                    stairVerticalSmoothTime);

            velocity.y =
                stairVerticalVelocity;
        }
        else
        {
            Vector3 forward =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f) *
                Vector3.forward;

            Vector3 movement =
                forward *
                smoothForwardSpeed;

            velocity.x =
                movement.x;

            velocity.z =
                movement.z;

            // ====================================================
            // JUMP
            // ====================================================

            if (jumpQueued)
            {
                if (isGrounded)
                {
                    float gravity =
                        Mathf.Abs(
                            baseGravityY);

                    velocity.y =
                        Mathf.Sqrt(
                            2f *
                            jumpHeight *
                            gravity);

                    if (animator != null)
                    {
                        animator.ResetTrigger(
                            JumpHash);

                        animator.SetTrigger(
                            JumpHash);
                    }

                    footstepTimer = 0f;
                    currentBox = null;
                }

                jumpQueued = false;
            }

            float gravityMultiplier =
                velocity.y < 0f
                    ? fallGravityMultiplier
                    : lowJumpGravityMultiplier;

            velocity.y +=
                baseGravityY *
                gravityMultiplier *
                Time.fixedDeltaTime;
        }

        rb.velocity =
            velocity;
    }

    // ============================================================
    // PUSHABLE CUBE
    // ============================================================

    private void OnCollisionStay(
        Collision collision)
    {
        PushableCube cube =
            collision.collider.GetComponent<
                PushableCube>();

        if (cube == null)
            return;

        Vector3 catVelocity =
            rb.velocity;

        Vector3 horizontalVelocity =
            new Vector3(
                catVelocity.x,
                0f,
                catVelocity.z);

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
            currentBox = cube;

        if (horizontalVelocity.magnitude <
            pushMinSpeed)
            return;

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
                continue;

            directionToCube.Normalize();

            float pushDot =
                Vector3.Dot(
                    pushDirection,
                    directionToCube);

            if (pushDot > 0.25f)
            {
                cube.Push(
                    pushDirection,
                    horizontalVelocity.magnitude);

                break;
            }
        }
    }

    private void OnCollisionExit(
        Collision collision)
    {
        PushableCube cube =
            collision.collider.GetComponent<
                PushableCube>();

        if (cube != null &&
            cube == currentBox)
        {
            currentBox = null;
        }
    }

    // ============================================================
    // STAIR DETECTION
    // ============================================================

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
                QueryTriggerInteraction.Ignore);

        if (!isAutoClimbing)
        {
            if (stairDetected)
                StartAutoClimb();

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

    // ============================================================
    // START STAIR CLIMB
    // ============================================================

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

        smoothTurnSpeed = 0f;
        turnSpeedSmoothRef = 0f;
        stairYawVelocity = 0f;

        footstepTimer = 0f;

        if (smoothForwardSpeed < 0.1f)
            smoothForwardSpeed = 0f;

        targetStairAngle =
            stairUpAngle;

        stairVerticalVelocity = 0f;

        Debug.Log(
            "Cat started stair climb. " +
            "Y rotation locked to " +
            stairLockYRotation);
    }

    // ============================================================
    // FINISH STAIR CLIMB
    // ============================================================

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

        smoothTurnSpeed = 0f;
        turnSpeedSmoothRef = 0f;
        stairYawVelocity = 0f;

        inputX = 0f;
        inputZ = 0f;

        footstepTimer = 0f;

        Vector3 velocity =
            rb.velocity;

        velocity.y = 0f;

        rb.velocity =
            velocity;

        Debug.Log(
            "Cat finished stair climb. " +
            "Normal movement restored.");
    }

    // ============================================================
    // CONTROL
    // ============================================================

    public void SetControl(bool value)
    {
        canControl = value;

        if (!canControl)
        {
            inputX = 0f;
            inputZ = 0f;
            jumpQueued = false;
            footstepTimer = 0f;

            if (isAutoClimbing)
                FinishAutoClimb();

            Debug.Log(
                "Cat controls DISABLED.");
        }
        else
        {
            Debug.Log(
                "Cat controls ENABLED.");
        }
    }

    // ============================================================
    // STAIR ANGLE
    // ============================================================

    public void SetStairAngle(float angle)
    {
        targetStairAngle = angle;
    }

    public void ResetStairAngle()
    {
        targetStairAngle = 0f;
    }

    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius);
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
            stairRayDistance);
    }
}