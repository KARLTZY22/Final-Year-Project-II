using UnityEngine;
using System.Collections;

public class AudioTriggerZone : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip triggerSound;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Player Settings")]
    public float pauseDuration = 0.5f; // How long to pause the player

    private AudioSource audioSource;
    private bool hasTriggered = false;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = volume;
        audioSource.clip = triggerSound;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(TriggerSequence(other.gameObject));
        }
    }

    private IEnumerator TriggerSequence(GameObject player)
    {
        // Get player's movement components
        CharacterController controller = player.GetComponent<CharacterController>();

        // Store original state
        bool wasControllerEnabled = false;
        if (controller != null)
        {
            wasControllerEnabled = controller.enabled;
            controller.enabled = false;
        }

        // Play sound
        if (audioSource != null && triggerSound != null)
        {
            audioSource.Play();
        }

        // Wait for specified duration
        yield return new WaitForSeconds(pauseDuration);

        // Restore player movement
        if (controller != null)
        {
            controller.enabled = wasControllerEnabled;
        }

        // Destroy the trigger after use
        Destroy(gameObject);
    }
}
