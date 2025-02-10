using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.2f; // Scale multiplier for hover effect
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale; // Store the original button scale
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GetComponent<UnityEngine.UI.Button>() != null) // Ensure it's a button
        {
            transform.localScale = originalScale * scaleFactor; // Scale only the button
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GetComponent<UnityEngine.UI.Button>() != null)
        {
            transform.localScale = originalScale; // Reset scale
        }
    }
}
