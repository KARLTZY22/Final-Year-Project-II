using UnityEngine;
using System.Collections;

public class SimpleAudioTrigger : MonoBehaviour
{
    public AudioClip triggerSound;
    public bool playOnce = true;
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;
    private bool hasPlayed = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = triggerSound;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playOnce || !hasPlayed)
            {
                if (audioSource != null && triggerSound != null)
                {
                    audioSource.Play();
                    hasPlayed = true;
                }
            }
        }
    }
}