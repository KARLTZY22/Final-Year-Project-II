using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TutorialManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class TutorialMessage
    {
        public string title;
        [TextArea(3, 10)]
        public string message;
        public bool pauseGameDuringDisplay = false;
        public AudioClip voiceOver;
    }

    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI continueText;
    public Image backgroundPanel;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;

    [Header("Audio Settings")]
    public AudioSource messageAudioSource;
    public AudioSource voiceOverAudioSource;
    public AudioClip messageAppearSound;
    public AudioClip messageCloseSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Continue Settings")]
    public KeyCode continueKey = KeyCode.Space;
    public string continuePrompt = "Press SPACE to continue...";

    private Queue<TutorialMessage> messageQueue = new Queue<TutorialMessage>();
    private bool isDisplayingMessage = false;
    private float originalTimeScale;
    private CanvasGroup canvasGroup;
    private Coroutine currentMessageCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Find the root Canvas and make it persistent
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                DontDestroyOnLoad(rootCanvas.gameObject);
            }

            InitializeComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        if (messageAudioSource == null)
        {
            messageAudioSource = gameObject.AddComponent<AudioSource>();
            messageAudioSource.playOnAwake = false;
        }

        if (voiceOverAudioSource == null)
        {
            voiceOverAudioSource = gameObject.AddComponent<AudioSource>();
            voiceOverAudioSource.playOnAwake = false;
        }

        if (tutorialPanel != null)
        {
            canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }
            tutorialPanel.SetActive(false);
        }
    }

    public void QueueMessage(string title, string message, AudioClip voiceClip = null, bool pauseGame = false)
    {
        if (tutorialPanel == null)
        {
            Debug.LogWarning("Tutorial panel is missing!");
            return;
        }

        TutorialMessage tutorialMessage = new TutorialMessage
        {
            title = title,
            message = message,
            voiceOver = voiceClip,
            pauseGameDuringDisplay = pauseGame
        };

        messageQueue.Enqueue(tutorialMessage);

        if (!isDisplayingMessage)
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        if (messageQueue.Count > 0)
        {
            currentMessageCoroutine = StartCoroutine(DisplayMessageSequence(messageQueue.Dequeue()));
        }
        else
        {
            StartCoroutine(HideMessage());
        }
    }

    private IEnumerator DisplayMessageSequence(TutorialMessage message)
    {
        if (tutorialPanel == null) yield break;

        isDisplayingMessage = true;

        if (message.pauseGameDuringDisplay)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        StopAllAudio();

        tutorialPanel.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));

        if (titleText != null) titleText.text = message.title;
        if (messageText != null) messageText.text = message.message;
        if (continueText != null) continueText.text = continuePrompt;

        if (messageAppearSound != null && messageAudioSource != null)
        {
            messageAudioSource.PlayOneShot(messageAppearSound, soundVolume);
        }

        if (message.voiceOver != null && voiceOverAudioSource != null)
        {
            voiceOverAudioSource.clip = message.voiceOver;
            voiceOverAudioSource.volume = soundVolume;
            voiceOverAudioSource.Play();
        }

        while (!Input.GetKeyDown(continueKey))
        {
            yield return null;
        }

        ShowNextMessage();
    }

    private IEnumerator HideMessage()
    {
        if (tutorialPanel == null) yield break;

        StopAllAudio();

        if (messageCloseSound != null && messageAudioSource != null)
        {
            messageAudioSource.PlayOneShot(messageCloseSound, soundVolume);
        }

        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));

        if (Time.timeScale == 0f)
        {
            Time.timeScale = originalTimeScale;
        }

        tutorialPanel.SetActive(false);
        isDisplayingMessage = false;
        currentMessageCoroutine = null;
    }

    private void StopAllAudio()
    {
        if (voiceOverAudioSource != null && voiceOverAudioSource.isPlaying)
        {
            voiceOverAudioSource.Stop();
        }

        if (messageAudioSource != null && messageAudioSource.isPlaying)
        {
            messageAudioSource.Stop();
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    public void ClearAllMessages()
    {
        messageQueue.Clear();
        if (isDisplayingMessage)
        {
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
            }
            StopAllAudio();
            StartCoroutine(HideMessage());
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}