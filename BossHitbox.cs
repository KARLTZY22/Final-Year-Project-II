using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    public float damage = 25f;  // Set this in inspector
    public AudioClip hitSound;  // Add hit sound
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    private AudioSource audioSource;

    private void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                // Play hit sound
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound, soundVolume);
                }
            }
        }
    }
}
