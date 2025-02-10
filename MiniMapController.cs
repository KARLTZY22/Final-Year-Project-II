using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    public Transform player; // The player or object the camera will follow

    [Header("Settings")]
    public float height = 10f; // Height of the minimap camera above the player
    public Vector3 offset = Vector3.zero; // Optional offset for fine-tuning the position
    public bool rotateWithPlayer = false; // Should the minimap rotate with the player?

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing in MinimapController.");
            return;
        }

        // Update the minimap camera's position
        Vector3 newPosition = player.position + Vector3.up * height + offset;
        transform.position = newPosition;

        // Rotate the minimap camera if needed
        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Always look straight down
        }
    }
}
