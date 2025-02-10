using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DestructibleMachine : MonoBehaviour
{
    [Header("Machine Settings")]
    public float maxHealth = 200f;
    private float currentHealth;

    [Header("Effects")]
    public GameObject destructionEffect;  // Optional particle effect
    public AudioClip hitSound;
    public AudioClip destructionSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    private AudioSource audioSource;
    private Scene2Manager scene2Manager;
    private bool isDestroyed = false;

    private void Start()
    {
        currentHealth = maxHealth;
        // Use FindFirstObjectByType instead of FindObjectOfType
        scene2Manager = Object.FindFirstObjectByType<Scene2Manager>();

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;

        currentHealth -= damage;

        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, soundVolume);
        }

        if (currentHealth <= 0)
        {
            DestroyMachine();
        }
    }

    private void DestroyMachine()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Play effects
        if (destructionEffect != null)
            Instantiate(destructionEffect, transform.position, transform.rotation);

        if (destructionSound != null && audioSource != null)
            audioSource.PlayOneShot(destructionSound, soundVolume);

        // Show victory
        var victoryManager = FindFirstObjectByType<VictoryManager>();
        if (victoryManager != null)
        {
            victoryManager.ShowVictory();
        }

        // Notify Scene2Manager
        var scene2Manager = FindFirstObjectByType<Scene2Manager>();
        if (scene2Manager != null)
        {
            scene2Manager.OnMachineDestroyed();
        }

        gameObject.SetActive(false);
    }
}