using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxCapacity = 20;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryPanel;

    [Header("UI References")]
    public TextMeshProUGUI totalItemsText;
    public TextMeshProUGUI statusText;
    public GameObject inventoryFullMessage;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip depositSound;
    public AudioClip fullSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    private AudioSource audioSource;
    private int totalItems = 0;
    private QuestManager questManager;
    private WeaponManager weaponManager;
    private bool isScene2;

    private void Start()
    {
        InitializeComponents();
        UpdateAllSlots();
        CheckScene();
    }

    private void InitializeComponents()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find managers
        weaponManager = FindFirstObjectByType<WeaponManager>();
        questManager = FindFirstObjectByType<QuestManager>();

        // Initialize UI
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        if (inventoryFullMessage != null)
            inventoryFullMessage.SetActive(false);

        // Check slots
        if (inventorySlots == null || inventorySlots.Length == 0)
            Debug.LogError("No inventory slots assigned!");
    }

    private void CheckScene()
    {
        // Use the public method to check the scene state
        if (GameManager.Instance.IsScene2())  // Correctly use IsScene2 method
        {
            isScene2 = true;
        }
        else
        {
            isScene2 = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool newState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(newState);

            // Update UI when opening
            if (newState)
                UpdateAllSlots();
        }
    }

    public bool CanAddItem()
    {
        return totalItems < maxCapacity;
    }

    public void AddItem(TrashType type)
    {
        if (!CanAddItem())
        {
            ShowInventoryFullMessage();
            return;
        }

        InventorySlot slot = GetSlotByType(type);
        if (slot != null)
        {
            totalItems++;

            // Update slot display
            int currentCount = GetItemCount(type) + 1;
            slot.UpdateSlot(slot.icon.sprite, currentCount);
            slot.isEmpty = false;

            // Play sound
            if (pickupSound != null)
                audioSource.PlayOneShot(pickupSound, audioVolume);

            // Update UI
            UpdateTotalText();
            ShowStatus($"Picked up {type}");

            // Notify managers
            if (weaponManager != null)
            {
                weaponManager.AddTrashCollected(1);
            }

            if (questManager != null)
            {
                questManager.UpdateTrashCount(totalItems);
            }
        }
    }

    public void DepositItems(TrashType type)
    {
        InventorySlot slot = GetSlotByType(type);
        if (slot != null && !slot.isEmpty)
        {
            int amount = GetItemCount(type);
            totalItems -= amount;

            // Reset slot display
            slot.UpdateSlot(null, 0);
            slot.isEmpty = true;

            // Play sound
            if (depositSound != null)
                audioSource.PlayOneShot(depositSound, audioVolume);

            // Update UI
            UpdateTotalText();
            ShowStatus($"Deposited {amount} {type} items");
        }
    }

    private void ShowInventoryFullMessage()
    {
        if (inventoryFullMessage != null)
        {
            inventoryFullMessage.SetActive(true);
            Invoke(nameof(HideInventoryFullMessage), 2f);
        }

        if (fullSound != null)
            audioSource.PlayOneShot(fullSound, audioVolume);
    }

    private void HideInventoryFullMessage()
    {
        if (inventoryFullMessage != null)
            inventoryFullMessage.SetActive(false);
    }

    private InventorySlot GetSlotByType(TrashType type)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.trashType == type)
                return slot;
        }
        return null;
    }

    public int GetItemCount(TrashType type)
    {
        InventorySlot slot = GetSlotByType(type);
        if (slot != null && !slot.isEmpty)
        {
            if (slot.countText != null && int.TryParse(slot.countText.text, out int count))
            {
                return count;
            }
        }
        return 0;
    }

    public int GetTotalItems()
    {
        return totalItems;
    }

    private void UpdateAllSlots()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null)
            {
                // Force initial update
                slot.UpdateSlot(slot.icon.sprite, GetItemCount(slot.trashType));
            }
        }
        UpdateTotalText();
    }

    private void UpdateTotalText()
    {
        if (totalItemsText != null)
            totalItemsText.text = $"Items: {totalItems}/{maxCapacity}";

        if (questManager != null)
        {
            questManager.UpdateTrashCount(totalItems);
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            CancelInvoke(nameof(ClearStatus));
            Invoke(nameof(ClearStatus), 3f);
        }
    }

    private void ClearStatus()
    {
        if (statusText != null)
            statusText.text = "";
    }

    // Public getters for other scripts
    public bool IsFull() => totalItems >= maxCapacity;
    public float GetCapacityPercentage() => (float)totalItems / maxCapacity;

    private void OnDisable()
    {
        // Save state if needed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameState();
        }
    }
}
