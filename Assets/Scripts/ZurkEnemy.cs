using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ZurkEnemy : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";
    public Transform target;

    // =========================================================
    // ATTACK POINTS
    // =========================================================

    [Header("Attack Points")]

    public string attackPointParentTag = "Cat.001";

    public bool getAttackPointsAutomatically = true;

    public List<Transform> attackPoints =
        new List<Transform>();

    public bool chooseClosestPoint = true;

    public bool allowSharedAttackPoint = false;

    // =========================================================
    // DETECTION
    // =========================================================

    [Header("Detection")]

    public float detectionRange = 8f;

    public float attackRange = 1.2f;

    public bool stopWhenCatLeavesDetectionRange = true;

    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]

    public float moveSpeed = 3.5f;

    public float rotationSpeed = 10f;

    // =========================================================
    // ROTATION
    // =========================================================

    [Header("Rotation")]

    public float fixedXRotation = -80f;

    // =========================================================
    // JUMP
    // =========================================================

    [Header("Jump To Attack Point")]

    public float jumpDuration = 0.35f;

    public float jumpHeight = 0.6f;

    // =========================================================
    // ATTACHED
    // =========================================================

    [Header("Attached")]

    [Tooltip("How long the Zurk stays attached.")]
    public float attachedDuration = 5f;

    public float wiggleAmount = 0.03f;

    public float wiggleSpeed = 15f;

    // =========================================================
    // CAT DAMAGE
    // =========================================================

    [Header("Cat Damage")]

    [Tooltip("Damage dealt every second while attached.")]
    public float damagePerSecond = 1f;

    [Tooltip("If true, damage is applied immediately when attached.")]
    public bool damageImmediately = true;

    // =========================================================
    // BEHAVIOUR
    // =========================================================

    [Header("Behaviour")]

    public bool destroyAfterDetach = false;

    public bool startImmediately = false;

    // =========================================================
    // RIGIDBODY
    // =========================================================

    [Header("Rigidbody")]

    public bool configureRigidbody = true;

    // =========================================================
    // PRIVATE
    // =========================================================

    private Rigidbody rb;
    private Collider zurkCollider;

    private bool chasing;
    private bool jumping;
    private bool attached;

    private bool hasDetectedCat;
    private bool hasLostCat;

    private Transform attackPointParent;
    private Transform currentAttackPoint;

    private Coroutine jumpCoroutine;
    private Coroutine damageCoroutine;

    private float wiggleOffset;

    // =========================================================
    // GLOBAL OCCUPIED POINTS
    // =========================================================

    private static readonly HashSet<Transform>
        occupiedAttackPoints =
        new HashSet<Transform>();

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb =
            GetComponent<Rigidbody>();

        zurkCollider =
            GetComponent<Collider>();

        wiggleOffset =
            Random.Range(0f, 100f);

        // =====================================================
        // RIGIDBODY
        // =====================================================

        if (configureRigidbody && rb != null)
        {
            rb.isKinematic = true;

            rb.useGravity = false;

            rb.interpolation =
                RigidbodyInterpolation.Interpolate;
        }

        // =====================================================
        // COLLIDER
        // =====================================================

        if (zurkCollider != null)
        {
            zurkCollider.isTrigger = true;
        }

        // =====================================================
        // ROTATION
        // =====================================================

        SetFixedXRotation();

        // =====================================================
        // FIND PLAYER
        // =====================================================

        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag(
                    playerTag
                );

            if (player != null)
            {
                target =
                    player.transform;
            }
            else
            {
                Debug.LogWarning(
                    "ZurkEnemy: Player with tag '" +
                    playerTag +
                    "' was not found."
                );
            }
        }

        // =====================================================
        // FIND ATTACK POINTS
        // =====================================================

        if (getAttackPointsAutomatically)
        {
            FindAttackPoints();
        }

        // =====================================================
        // START IMMEDIATELY
        // =====================================================

        if (startImmediately)
        {
            chasing = true;
            hasDetectedCat = true;
        }
    }

    // =========================================================
    // FIND ATTACK POINTS
    // =========================================================

    private void FindAttackPoints()
    {
        attackPoints.Clear();

        GameObject catMesh =
            GameObject.FindGameObjectWithTag(
                attackPointParentTag
            );

        if (catMesh == null)
        {
            Debug.LogWarning(
                "ZurkEnemy: Could not find GameObject with tag '" +
                attackPointParentTag +
                "'."
            );

            return;
        }

        attackPointParent =
            catMesh.transform;

        for (int i = 0;
             i < attackPointParent.childCount;
             i++)
        {
            Transform child =
                attackPointParent.GetChild(i);

            if (child == null)
                continue;

            attackPoints.Add(child);
        }

        Debug.Log(
            "ZurkEnemy: Found " +
            attackPoints.Count +
            " attack points under " +
            attackPointParent.name
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (target == null)
            return;

        // =====================================================
        // ATTACHED
        // =====================================================

        if (attached)
        {
            FollowAttackPoint();

            return;
        }

        // =====================================================
        // KEEP X ROTATION
        // =====================================================

        SetFixedXRotation();

        // =====================================================
        // JUMPING
        // =====================================================

        if (jumping)
            return;

        // =====================================================
        // DISTANCE
        // =====================================================

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        // =====================================================
        // INITIAL DETECTION
        // =====================================================

        if (!hasDetectedCat)
        {
            if (distance <= detectionRange)
            {
                hasDetectedCat = true;

                chasing = true;

                Debug.Log(
                    name +
                    " detected the cat."
                );
            }
            else
            {
                return;
            }
        }

        // =====================================================
        // CAT LEFT DETECTION RANGE
        // =====================================================

        if (hasDetectedCat &&
            !hasLostCat &&
            stopWhenCatLeavesDetectionRange)
        {
            if (distance > detectionRange)
            {
                hasLostCat = true;

                chasing = false;

                Debug.Log(
                    name +
                    " lost the cat and stopped at its current position."
                );

                return;
            }
        }

        // =====================================================
        // LOST CAT
        // =====================================================

        if (hasLostCat)
        {
            return;
        }

        // =====================================================
        // ATTACK
        // =====================================================

        if (distance <= attackRange)
        {
            StartAttack();

            return;
        }

        // =====================================================
        // CHASE
        // =====================================================

        if (chasing)
        {
            MoveTowardsCat();
        }
    }

    // =========================================================
    // MOVE TOWARDS CAT
    // =========================================================

    private void MoveTowardsCat()
    {
        Vector3 direction =
            target.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        direction.Normalize();

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;

        Quaternion lookRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        Vector3 euler =
            lookRotation.eulerAngles;

        euler.x =
            fixedXRotation;

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(euler),
                rotationSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // START ATTACK
    // =========================================================

    private void StartAttack()
    {
        if (jumping ||
            attached ||
            hasLostCat)
        {
            return;
        }

        currentAttackPoint =
            FindFreeAttackPoint();

        if (currentAttackPoint == null)
        {
            return;
        }

        // =====================================================
        // RESERVE POINT
        // =====================================================

        if (!allowSharedAttackPoint)
        {
            occupiedAttackPoints.Add(
                currentAttackPoint
            );
        }

        jumping = true;

        if (jumpCoroutine != null)
        {
            StopCoroutine(
                jumpCoroutine
            );
        }

        jumpCoroutine =
            StartCoroutine(
                JumpToAttackPoint()
            );
    }

    // =========================================================
    // FIND FREE ATTACK POINT
    // =========================================================

    private Transform FindFreeAttackPoint()
    {
        if (attackPoints == null ||
            attackPoints.Count == 0)
        {
            FindAttackPoints();

            if (attackPoints.Count == 0)
            {
                return null;
            }
        }

        Transform selectedPoint = null;

        float closestDistance =
            Mathf.Infinity;

        for (int i = 0;
             i < attackPoints.Count;
             i++)
        {
            Transform point =
                attackPoints[i];

            if (point == null)
                continue;

            if (!allowSharedAttackPoint &&
                occupiedAttackPoints.Contains(point))
            {
                continue;
            }

            if (!chooseClosestPoint)
            {
                return point;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    point.position
                );

            if (distance <
                closestDistance)
            {
                closestDistance =
                    distance;

                selectedPoint =
                    point;
            }
        }

        return selectedPoint;
    }

    // =========================================================
    // JUMP TO ATTACK POINT
    // =========================================================

    private IEnumerator JumpToAttackPoint()
    {
        if (currentAttackPoint == null)
        {
            jumping = false;

            yield break;
        }

        Vector3 startPosition =
            transform.position;

        float timer = 0f;

        while (timer < jumpDuration)
        {
            if (currentAttackPoint == null)
            {
                jumping = false;

                ReleaseAttackPoint();

                yield break;
            }

            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    jumpDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            Vector3 targetPosition =
                currentAttackPoint.position;

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            position.y +=
                Mathf.Sin(
                    t * Mathf.PI
                ) *
                jumpHeight;

            transform.position =
                position;

            Vector3 direction =
                targetPosition -
                transform.position;

            if (direction.sqrMagnitude >
                0.001f)
            {
                direction.Normalize();

                Quaternion lookRotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up
                    );

                Vector3 euler =
                    lookRotation.eulerAngles;

                euler.x =
                    fixedXRotation;

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.Euler(euler),
                        15f *
                        Time.deltaTime
                    );
            }

            yield return null;
        }

        // =====================================================
        // ATTACH
        // =====================================================

        AttachToAttackPoint();
    }

    // =========================================================
    // ATTACH
    // =========================================================

    private void AttachToAttackPoint()
    {
        if (currentAttackPoint == null)
        {
            jumping = false;

            ReleaseAttackPoint();

            return;
        }

        jumping = false;

        attached = true;

        transform.position =
            currentAttackPoint.position;

        ApplyAttackPointRotation();

        Debug.Log(
            name +
            " attached to " +
            currentAttackPoint.name
        );

        // =====================================================
        // START DAMAGE
        // =====================================================

        if (damageCoroutine != null)
        {
            StopCoroutine(
                damageCoroutine
            );
        }

        damageCoroutine =
            StartCoroutine(
                DamageCatRoutine()
            );

        // =====================================================
        // DETACH TIMER
        // =====================================================

        if (attachedDuration > 0f)
        {
            StartCoroutine(
                DetachAfterTime()
            );
        }
    }

    // =========================================================
    // DAMAGE CAT
    // =========================================================

    private IEnumerator DamageCatRoutine()
    {
        CatHealth catHealth =
            target.GetComponent<CatHealth>();

        if (catHealth == null)
        {
            Debug.LogWarning(
                name +
                ": CatHealth component was not found on target."
            );

            yield break;
        }

        // =====================================================
        // IMMEDIATE DAMAGE
        // =====================================================

        if (damageImmediately)
        {
            catHealth.TakeDamage(
                damagePerSecond
            );

            Debug.Log(
                name +
                " dealt " +
                damagePerSecond +
                " damage to the cat."
            );
        }

        // =====================================================
        // DAMAGE EVERY SECOND
        // =====================================================

        while (attached)
        {
            yield return new WaitForSeconds(
                1f
            );

            // Zurk may have detached during the wait.
            if (!attached)
                yield break;

            // Cat may have died.
            if (catHealth == null ||
                catHealth.IsDead)
            {
                yield break;
            }

            catHealth.TakeDamage(
                damagePerSecond
            );

            Debug.Log(
                name +
                " dealt " +
                damagePerSecond +
                " damage to the cat."
            );
        }
    }

    // =========================================================
    // FOLLOW ATTACK POINT
    // =========================================================

    private void FollowAttackPoint()
    {
        if (currentAttackPoint == null)
        {
            attached = false;

            StopDamageCoroutine();

            ReleaseAttackPoint();

            chasing = false;

            return;
        }

        Vector3 position =
            currentAttackPoint.position;

        float sideWiggle =
            Mathf.Sin(
                Time.time *
                wiggleSpeed +
                wiggleOffset
            ) *
            wiggleAmount;

        float verticalWiggle =
            Mathf.Sin(
                Time.time *
                wiggleSpeed *
                0.8f +
                wiggleOffset
            ) *
            wiggleAmount;

        position +=
            currentAttackPoint.right *
            sideWiggle;

        position +=
            currentAttackPoint.up *
            verticalWiggle;

        transform.position =
            position;

        ApplyAttackPointRotation();
    }

    // =========================================================
    // ATTACK POINT ROTATION
    // =========================================================

    private void ApplyAttackPointRotation()
    {
        if (currentAttackPoint == null)
            return;

        Vector3 euler =
            currentAttackPoint.eulerAngles;

        euler.x =
            fixedXRotation;

        transform.rotation =
            Quaternion.Euler(
                euler
            );
    }

    // =========================================================
    // STOP DAMAGE
    // =========================================================

    private void StopDamageCoroutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(
                damageCoroutine
            );

            damageCoroutine = null;
        }
    }

    // =========================================================
    // DETACH TIMER
    // =========================================================

    private IEnumerator DetachAfterTime()
    {
        yield return new WaitForSeconds(
            attachedDuration
        );

        DetachFromCat();
    }

    // =========================================================
    // DETACH
    // =========================================================

    public void DetachFromCat()
    {
        if (!attached)
            return;

        attached = false;

        // Stop damage immediately.
        StopDamageCoroutine();

        ReleaseAttackPoint();

        currentAttackPoint = null;

        SetFixedXRotation();

        if (destroyAfterDetach)
        {
            Destroy(gameObject);

            return;
        }

        if (hasLostCat)
        {
            chasing = false;

            return;
        }

        chasing = true;

        Debug.Log(
            name +
            " detached from the cat."
        );
    }

    // =========================================================
    // RELEASE POINT
    // =========================================================

    private void ReleaseAttackPoint()
    {
        if (currentAttackPoint == null)
            return;

        occupiedAttackPoints.Remove(
            currentAttackPoint
        );
    }

    // =========================================================
    // KILL
    // =========================================================

    public void Kill()
    {
        StopAllCoroutines();

        StopDamageCoroutine();

        ReleaseAttackPoint();

        attached = false;
        jumping = false;

        currentAttackPoint = null;

        Destroy(gameObject);
    }

    // =========================================================
    // FIX X ROTATION
    // =========================================================

    private void SetFixedXRotation()
    {
        Vector3 rotation =
            transform.eulerAngles;

        rotation.x =
            fixedXRotation;

        transform.eulerAngles =
            rotation;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        if (attackPoints != null)
        {
            for (int i = 0;
                 i < attackPoints.Count;
                 i++)
            {
                Transform point =
                    attackPoints[i];

                if (point == null)
                    continue;

                Gizmos.color =
                    Color.cyan;

                Gizmos.DrawWireSphere(
                    point.position,
                    0.08f
                );
            }
        }
    }
}