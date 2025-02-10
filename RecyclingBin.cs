using UnityEngine;
using TMPro;
using System.Collections;

public class RecyclingBin : MonoBehaviour
{
    [Header("Bin Settings")]
    public TrashType acceptedType;
    public Material binMaterial;
    public float interactionRadius = 2f; // Added interaction radius

    [Header("UI Elements")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI warningText;
    public float warningDuration = 3f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionCooldown = 0.5f;

    [Header("Effects")]
    public AudioClip depositSound;
    public AudioClip wrongBinSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    private bool playerInRange = false;
    private bool canInteract = true;
    private Transform player;
    private AudioSource audioSource;
    private InventorySystem playerInventory;

    private void Start()
    {
        // Set up bin identification
        gameObject.tag = acceptedType.ToString() + "Bin";

        // Set up material if provided
        if (binMaterial != null)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.material = binMaterial;
        }

        // Initialize audio
        SetupAudio();

        // Initialize UI
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.volume = soundVolume;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check if player is in range using distance
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inRange = distanceToPlayer <= interactionRadius;

        // Update player in range status
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            UpdatePromptVisibility();
        }

        // Handle interaction
        if (playerInRange && canInteract && Input.GetKeyDown(interactKey))
        {
            HandleInteraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerInventory = other.GetComponent<InventorySystem>();
            UpdatePromptVisibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            playerInventory = null;
            playerInRange = false;
            UpdatePromptVisibility();
        }
    }

    private void UpdatePromptVisibility()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(playerInRange);
            if (playerInRange)
            {
                promptText.text = $"Press E to deposit {acceptedType} items";
            }
        }
    }

    private void HandleInteraction()
    {
        if (playerInventory == null) return;

        canInteract = false;
        StartCoroutine(ResetInteractionCooldown());

        int itemCount = playerInventory.GetItemCount(acceptedType);

        if (itemCount > 0)
        {
            // Deposit items
            playerInventory.DepositItems(acceptedType);

            // Play deposit sound
            if (depositSound != null)
                audioSource.PlayOneShot(depositSound, soundVolume);

            ShowMessage($"Deposited {itemCount} {acceptedType} items!", false);
        }
        else
        {
            // Play wrong bin sound
            if (wrongBinSound != null)
                audioSource.PlayOneShot(wrongBinSound, soundVolume);

            ShowMessage($"No {acceptedType} items to deposit!", true);
        }
    }

    private void ShowMessage(string message, bool isWarning)
    {
        TextMeshProUGUI textComponent = isWarning ? warningText : promptText;
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.gameObject.SetActive(true);

            if (isWarning)
            {
                StartCoroutine(HideMessage(textComponent));
            }
        }
    }

    private IEnumerator HideMessage(TextMeshProUGUI textComponent)
    {
        yield return new WaitForSeconds(warningDuration);
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(false);
        }
    }

    private IEnumerator ResetInteractionCooldown()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    // Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
