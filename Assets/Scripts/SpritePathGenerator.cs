using UnityEngine;

public class SpritePathGenerator : MonoBehaviour
{
    [Header("Path Points")]
    public Transform[] points;

    [Header("Sprite Settings")]
    public GameObject spritePrefab;

    [Tooltip("Distance between each sprite.")]
    public float gap = 1f;

    [Tooltip("Automatically generate the path when the game starts.")]
    public bool generateOnStart = true;

    [Header("Ground Settings")]
    [Tooltip("Layer containing the ground/road.")]
    public LayerMask groundLayer;

    [Tooltip("Extra height added above the ground.")]
    public float heightOffset = 0.02f;

    [Tooltip("Maximum distance used to search for the ground.")]
    public float groundRaycastDistance = 10f;

    [Header("Rotation")]
    [Tooltip("Additional rotation applied to every sprite.")]
    public Vector3 rotationOffset = Vector3.zero;

    [Tooltip("If enabled, the sprite will rotate to follow the path direction.")]
    public bool rotateWithPath = true;

    [Header("Parent")]
    [Tooltip("Optional parent for generated sprites.")]
    public Transform spriteParent;

    private void Start()
    {
        if (generateOnStart)
        {
            GeneratePath();
        }
    }

    [ContextMenu("Generate Path")]
    public void GeneratePath()
    {
        if (spritePrefab == null)
        {
            Debug.LogError("Sprite Prefab is not assigned.");
            return;
        }

        if (points == null || points.Length < 2)
        {
            Debug.LogError("You need at least 2 path points.");
            return;
        }

        if (gap <= 0)
        {
            Debug.LogError("Gap must be greater than 0.");
            return;
        }

        ClearPath();

        // Distance remaining until the next sprite.
        float distanceUntilNextSprite = 0f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i] == null || points[i + 1] == null)
                continue;

            Vector3 start = points[i].position;
            Vector3 end = points[i + 1].position;

            // Use X/Z for the path.
            Vector3 direction = end - start;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance <= 0.001f)
                continue;

            direction.Normalize();

            float travelled = distanceUntilNextSprite;

            while (travelled <= distance)
            {
                Vector3 position = start + direction * travelled;

                // Find the actual ground underneath this position.
                position = GetGroundPosition(position, start.y);

                // Calculate rotation.
                Quaternion rotation = GetSpriteRotation(direction);

                GameObject sprite = Instantiate(
                    spritePrefab,
                    position,
                    rotation,
                    spriteParent != null ? spriteParent : transform
                );

                sprite.name = "PathSprite";

                travelled += gap;
            }

            // Continue the spacing smoothly into the next segment.
            distanceUntilNextSprite = travelled - distance;
        }
    }

    private Vector3 GetGroundPosition(Vector3 position, float fallbackY)
    {
        // Start the ray above the road.
        Vector3 rayOrigin = new Vector3(
            position.x,
            position.y + groundRaycastDistance * 0.5f,
            position.z
        );

        Ray ray = new Ray(rayOrigin, Vector3.down);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            groundRaycastDistance,
            groundLayer
        ))
        {
            // Place the sprite on the ground
            // and add the adjustable height offset.
            position.y = hit.point.y + heightOffset;
        }
        else
        {
            // Fallback if no ground is detected.
            position.y = fallbackY + heightOffset;
        }

        return position;
    }

    private Quaternion GetSpriteRotation(Vector3 direction)
    {
        Quaternion pathRotation = Quaternion.identity;

        if (rotateWithPath)
        {
            // Calculate direction along X/Z.
            float angle = Mathf.Atan2(
                direction.x,
                direction.z
            ) * Mathf.Rad2Deg;

            pathRotation = Quaternion.Euler(
                0f,
                angle,
                0f
            );
        }

        // Apply your manually adjustable X/Y/Z rotation.
        Quaternion additionalRotation =
            Quaternion.Euler(rotationOffset);

        return pathRotation * additionalRotation;
    }

    [ContextMenu("Clear Path")]
    public void ClearPath()
    {
        Transform parent =
            spriteParent != null ? spriteParent : transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}