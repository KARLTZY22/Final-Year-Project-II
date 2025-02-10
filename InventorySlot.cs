using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;
    public TrashType trashType;
    public bool isEmpty = true;

    void Start()
    {
        // Make sure icon is assigned
        if (icon != null)
        {
            icon.enabled = false; // Start with icon hidden
        }
    }

    public void UpdateSlot(Sprite iconSprite, int count)
    {
        isEmpty = count <= 0;

        if (!isEmpty && icon != null)
        {
            icon.sprite = iconSprite;
            icon.enabled = true;  // Show icon when there are items
            countText.text = count.ToString();
            countText.enabled = true;
        }
        else
        {
            icon.enabled = false;
            countText.enabled = false;
        }
    }

}
