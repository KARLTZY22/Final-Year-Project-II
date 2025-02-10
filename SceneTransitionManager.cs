using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingScreen;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;
    public CanvasGroup fadeCanvasGroup;

    [Header("Loading Settings")]
    public float minimumLoadTime = 2f;
    public float fadeSpeed = 1f;
    public string[] loadingTips = {
        "Press 'R' to reload your weapon",
        "Watch out for toxic gas in level 2",
        "Collect clues to unlock the mystery",
        "The final boss has multiple attack patterns",
        "Use cover to avoid enemy fire"
    };

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isTransitioning = true;

        // Fade out current scene
        yield return StartCoroutine(FadeOut());

        // Show loading screen and initialize progress
        ShowLoadingScreen();
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        // Show random tip
        ShowRandomTip();

        // Start async load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float timeElapsed = 0f;
        float progressValue = 0f;

        while (!asyncLoad.isDone)
        {
            timeElapsed += Time.deltaTime;
            progressValue = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update loading UI
            UpdateLoadingProgress(progressValue);

            // Wait for both minimum time and loading to complete
            if (timeElapsed >= minimumLoadTime && asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Give a small delay for the scene to stabilize
        yield return new WaitForSeconds(0.5f);

        // Hide loading screen
        HideLoadingScreen();

        // Fade in new scene
        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private void ShowLoadingScreen()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
    }

    private void HideLoadingScreen()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    private void UpdateLoadingProgress(float progress)
    {
        if (loadingSlider != null)
            loadingSlider.value = progress;

        if (loadingText != null)
            loadingText.text = $"Loading... {(progress * 100):F0}%";
    }

    private void ShowRandomTip()
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
    }

    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0;

        while (fadeCanvasGroup.alpha < 1)
        {
            fadeCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.alpha = 1;

        while (fadeCanvasGroup.alpha > 0)
        {
            fadeCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        fadeCanvasGroup.gameObject.SetActive(false);
    }
}