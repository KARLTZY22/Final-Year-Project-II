using UnityEngine;

public class FireDamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 10f;
    public float tickRate = 0.5f;  // How often damage is applied

    [Header("Effects")]
    public AudioClip burnSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    private AudioSource audioSource;

    private float nextDamageTime;
    private PlayerHealth playerInFire;

    private void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && burnSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = burnSound;
            audioSource.loop = true;
            audioSource.volume = soundVolume;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (playerInFire != null && Time.time >= nextDamageTime)
        {
            // Apply damage
            float damage = damagePerSecond * tickRate;
            playerInFire.TakeDamage(damage);
            nextDamageTime = Time.time + tickRate;

            // Player dies if health reaches 0 (handled in PlayerHealth)
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInFire = other.GetComponent<PlayerHealth>();
            if (audioSource != null && burnSound != null)
            {
                audioSource.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInFire = null;
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }

    // Visualize the fire damage zone in editor
    private void OnDrawGizmos()
    {
        // Get the collider if it exists
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // Draw wire mesh to show damage area
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
            Gizmos.DrawWireMesh(null, transform.position, transform.rotation, transform.localScale);
        }
    }
}
