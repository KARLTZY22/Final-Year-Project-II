using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClueEntryItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public void SetupClueEntry(string title, string description, Sprite icon)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (iconImage != null && icon != null)
            iconImage.sprite = icon;
    }
}
