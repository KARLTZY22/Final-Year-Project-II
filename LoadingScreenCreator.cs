using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenCreator : MonoBehaviour
{
    // Reference this script to your SceneTransitionManager GameObject

    [Header("Colors")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color sliderBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color sliderFillColor = new Color(0.2f, 0.6f, 1f, 1f);

    [Header("Text Settings")]
    public Color loadingTextColor = Color.white;
    public Color tipTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public float loadingTextSize = 36f;
    public float tipTextSize = 24f;

    private void Start()
    {
        CreateLoadingScreen();
    }

    private void CreateLoadingScreen()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("LoadingScreen");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Background
        GameObject background = CreateImageObject(canvasObj, "Background", backgroundColor);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        SetFullScreenRect(bgRect);

        // Create Loading Panel
        GameObject panel = CreateImageObject(canvasObj, "LoadingPanel", panelColor);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetCenteredRect(panelRect, new Vector2(800, 400));

        // Create Slider
        GameObject sliderObj = CreateSlider(panel);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        SetAnchored(sliderRect, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.3f));

        // Create Loading Text
        GameObject loadingTextObj = CreateTextObject(panel, "LoadingText", "Loading...",
            loadingTextColor, loadingTextSize);
        RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
        SetAnchored(loadingTextRect, new Vector2(0, 0.7f), new Vector2(1, 0.9f));

        // Create Tip Text
        GameObject tipTextObj = CreateTextObject(panel, "TipText", "Loading tip goes here",
            tipTextColor, tipTextSize);
        RectTransform tipTextRect = tipTextObj.GetComponent<RectTransform>();
        SetAnchored(tipTextRect, new Vector2(0, 0.4f), new Vector2(1, 0.6f));

        // Create Fade Overlay
        GameObject fadeObj = CreateImageObject(canvasObj, "FadeOverlay", Color.black);
        CanvasGroup fadeGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0;
        RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
        SetFullScreenRect(fadeRect);

        // Set up references in SceneTransitionManager
        SceneTransitionManager transitionManager = GetComponent<SceneTransitionManager>();
        if (transitionManager != null)
        {
            transitionManager.loadingScreen = canvasObj;
            transitionManager.loadingSlider = sliderObj.GetComponent<Slider>();
            transitionManager.loadingText = loadingTextObj.GetComponent<TextMeshProUGUI>();
            transitionManager.tipText = tipTextObj.GetComponent<TextMeshProUGUI>();
            transitionManager.fadeCanvasGroup = fadeGroup;
        }

        // Initially hide the loading screen
        canvasObj.SetActive(false);
    }

    private GameObject CreateImageObject(GameObject parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return obj;
    }

    private GameObject CreateTextObject(GameObject parent, string name, string text,
        Color color, float fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        return obj;
    }

    private GameObject CreateSlider(GameObject parent)
    {
        // Create Slider GameObject
        GameObject sliderObj = new GameObject("LoadingSlider");
        sliderObj.transform.SetParent(parent.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();

        // Background
        GameObject background = CreateImageObject(sliderObj, "Background", sliderBackgroundColor);
        background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        SetSliderFillArea(fillAreaRect);

        // Fill
        GameObject fill = CreateImageObject(fillArea, "Fill", sliderFillColor);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        SetFullScreenRect(fillRect);

        // Configure Slider
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = null; // No handle for loading bar
        slider.targetGraphic = background.GetComponent<Image>();
        slider.interactable = false;
        slider.value = 0;
        slider.transition = Selectable.Transition.None;

        return sliderObj;
    }

    private void SetFullScreenRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetCenteredRect(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetAnchored(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetSliderFillArea(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = new Vector2(-20, -20); // Add padding
        rect.anchoredPosition = Vector2.zero;
    }
}
