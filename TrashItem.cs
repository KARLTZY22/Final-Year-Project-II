using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class TrashItem : MonoBehaviour
{
    public TrashType trashType;
    public Sprite icon;

    private void Start()
    {
        gameObject.tag = trashType.ToString() + "Trash";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventorySystem inventory = other.GetComponent<InventorySystem>();
            if (inventory != null && inventory.CanAddItem())
            {
                inventory.AddItem(trashType);
                gameObject.SetActive(false);
            }
        }
    }
}

