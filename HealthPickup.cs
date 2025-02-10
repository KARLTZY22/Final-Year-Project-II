using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing Settings")]
    public float healAmount = 25f;
    public bool destroyOnUse = true;

    [Header("Movement")]
    public float rotationSpeed = 90f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.5f;
    private Vector3 startPosition;

    [Header("Effects")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float volume = 0.7f;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        // Only heal if player isn't at full health
        if (playerHealth.currentHealth < playerHealth.maxHealth)
        {
            // Use negative damage value for healing
            playerHealth.TakeDamage(-healAmount);

            // Play sound effect
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }

            if (destroyOnUse)
            {
                Destroy(gameObject);
            }
        }
    }
}
