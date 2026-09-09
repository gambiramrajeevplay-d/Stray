using System.Collections;
using UnityEngine;

public class CameraCutsceneMover : MonoBehaviour
{
    [Header("Camera")]
    public Camera cutsceneCamera;

    [Header("Camera Points")]
    public Transform[] cameraPoints;

    [Header("Single Look At Target")]
    public Transform lookAtTarget;

    [Header("Movement")]
    [Tooltip("Camera movement speed in units per second.")]
    public float movementSpeed = 1f;

    [Header("Start")]
    public bool playOnStart = false;

    private Coroutine movementCoroutine;

    private void Start()
    {
        if (playOnStart)
        {
            StartCutscene();
        }
    }

    public void StartCutscene()
    {
        if (cutsceneCamera == null)
        {
            Debug.LogError(
                "CameraCutsceneMover: Camera is NOT assigned!"
            );
            return;
        }

        if (cameraPoints == null || cameraPoints.Length < 2)
        {
            Debug.LogError(
                "CameraCutsceneMover: Assign at least 2 Camera Points!"
            );
            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveCamera()
    {
        // ==========================================
        // START AT POINT 0
        // ==========================================

        cutsceneCamera.transform.position =
            cameraPoints[0].position;

        LookAt();

        // ==========================================
        // MOVE THROUGH EACH POINT
        // ==========================================

        for (int i = 1; i < cameraPoints.Length; i++)
        {
            Vector3 targetPosition =
                cameraPoints[i].position;

            // ======================================
            // MOVE TO TARGET
            // ======================================

            while (
                Vector3.Distance(
                    cutsceneCamera.transform.position,
                    targetPosition
                ) > 0.01f
            )
            {
                cutsceneCamera.transform.position =
                    Vector3.MoveTowards(
                        cutsceneCamera.transform.position,
                        targetPosition,
                        movementSpeed * Time.deltaTime
                    );

                // Keep looking at the same target
                LookAt();

                yield return null;
            }

            // ======================================
            // STOP EXACTLY AT POINT
            // ======================================

            cutsceneCamera.transform.position =
                targetPosition;

            LookAt();

            Debug.Log(
                "Camera stopped at Point " + i
            );
        }

        // ==========================================
        // FINISHED
        // ==========================================

        movementCoroutine = null;

        Debug.Log(
            "CAMERA MOVEMENT FINISHED"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CameraCutsceneFinished();
        }
    }

    // ==============================================
    // LOOK AT ONE TARGET
    // ==============================================

    private void LookAt()
    {
        if (lookAtTarget == null)
            return;

        Vector3 direction =
            lookAtTarget.position -
            cutsceneCamera.transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        cutsceneCamera.transform.rotation =
            Quaternion.LookRotation(direction);
    }
}