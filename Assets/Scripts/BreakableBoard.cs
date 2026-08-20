using System.Collections;
using UnityEngine;

public class BreakableBoard : MonoBehaviour
{
    [Header("Break On Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Pieces (Pre-fractured Glass Parts)")]
    [SerializeField] private Rigidbody[] pieces;

    [Header("Glass Break Movement")]
    [Tooltip("Main force pushing the glass toward world -X.")]
    [SerializeField] private float leftForce = 3f;

    [Tooltip("Force pulling the glass downward.")]
    [SerializeField] private float downwardForce = 2f;

    [Tooltip("Small random variation between pieces.")]
    [SerializeField] private float randomForce = 0.5f;

    [Tooltip("Random spinning force applied to each piece.")]
    [SerializeField] private float randomTorque = 3f;

    [Header("Physics")]
    [SerializeField] private float pieceDrag = 0.2f;
    [SerializeField] private float pieceAngularDrag = 1f;

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 4f;

    [Header("Disable On Break")]
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Audio")]
    [SerializeField] private AudioSource breakAudioSource;
    [SerializeField] private AudioClip breakClip;

    private bool broken = false;


    // =========================================================
    // PLAYER ENTERS
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (broken)
            return;

        if (!other.CompareTag(playerTag))
            return;

        BreakGlass();
    }


    // =========================================================
    // BREAK GLASS
    // =========================================================

    private void BreakGlass()
    {
        if (broken)
            return;

        broken = true;

        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        PlayBreakSound();

        // -----------------------------------------------------
        // DISABLE ORIGINAL BOARD COLLIDERS
        // -----------------------------------------------------

        DisableColliders();

        // -----------------------------------------------------
        // BREAK PIECES
        // -----------------------------------------------------

        BreakPieces();

        // -----------------------------------------------------
        // DESTROY ORIGINAL BOARD
        // -----------------------------------------------------

        StartCoroutine(DestroyAfterDelay());
    }


    // =========================================================
    // BREAK PIECES
    // =========================================================

    private void BreakPieces()
    {
        foreach (Rigidbody piece in pieces)
        {
            if (piece == null)
                continue;

            // -------------------------------------------------
            // ENABLE PHYSICS
            // -------------------------------------------------

            piece.isKinematic = false;
            piece.useGravity = true;

            piece.drag = pieceDrag;
            piece.angularDrag = pieceAngularDrag;

            // -------------------------------------------------
            // RESET EXISTING VELOCITY
            // -------------------------------------------------

            piece.velocity = Vector3.zero;
            piece.angularVelocity = Vector3.zero;

            // -------------------------------------------------
            // MAIN MOVEMENT
            //
            // -X + DOWN
            // -------------------------------------------------

            Vector3 force =
                Vector3.left * leftForce;

            force +=
                Vector3.down * downwardForce;

            // -------------------------------------------------
            // RANDOM VARIATION
            // -------------------------------------------------

            Vector3 random =
                Random.insideUnitSphere *
                randomForce;

            // Don't allow random force to push pieces upward.
            if (random.y > 0f)
            {
                random.y = 0f;
            }

            force += random;

            // -------------------------------------------------
            // APPLY FORCE
            // -------------------------------------------------

            piece.AddForce(
                force,
                ForceMode.Impulse
            );

            // -------------------------------------------------
            // RANDOM ROTATION
            // -------------------------------------------------

            piece.AddTorque(
                Random.insideUnitSphere *
                randomTorque,
                ForceMode.Impulse
            );
        }
    }


    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayBreakSound()
    {
        if (breakAudioSource == null)
            return;

        if (breakClip != null)
        {
            breakAudioSource.PlayOneShot(
                breakClip
            );
        }
        else
        {
            breakAudioSource.Play();
        }
    }


    // =========================================================
    // DISABLE COLLIDERS
    // =========================================================

    private void DisableColliders()
    {
        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(
            destroyDelay
        );

        Destroy(gameObject);
    }
}