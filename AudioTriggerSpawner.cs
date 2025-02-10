using UnityEngine;

public class AudioTriggerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject triggerZonePrefab;
    public int numberOfTriggers = 10;
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 50f;

    [Header("Area Settings")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    public float spawnHeight = 0.5f; // Height above ground

    private void Start()
    {
        SpawnTriggerZones();
    }

    private void SpawnTriggerZones()
    {
        for (int i = 0; i < numberOfTriggers; i++)
        {
            SpawnSingleTrigger();
        }
    }

    private void SpawnSingleTrigger()
    {
        Vector3 randomPosition = GetRandomPosition();

        // Check if position is valid
        if (randomPosition != Vector3.zero)
        {
            GameObject trigger = Instantiate(triggerZonePrefab, randomPosition, Quaternion.identity);
            trigger.transform.parent = transform; // Parent to spawner for organization
        }
    }

    private Vector3 GetRandomPosition()
    {
        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Get random position
            Vector3 randomPos = new Vector3(
                Random.Range(minX, maxX),
                100f, // Start high for raycast
                Random.Range(minZ, maxZ)
            );

            // Raycast to find ground
            RaycastHit hit;
            if (Physics.Raycast(randomPos, Vector3.down, out hit))
            {
                // Check if the point is not too close to other triggers
                Collider[] nearbyColliders = Physics.OverlapSphere(hit.point, minSpawnDistance);
                bool tooClose = false;

                foreach (Collider col in nearbyColliders)
                {
                    if (col.GetComponent<AudioTriggerZone>() != null)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    // Return position slightly above ground
                    return hit.point + Vector3.up * spawnHeight;
                }
            }

            attempts++;
        }

        return Vector3.zero;
    }

    // Optional: Visualize spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);
        Vector3 size = new Vector3(maxX - minX, 2, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }

}
