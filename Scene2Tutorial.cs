using UnityEngine;

public class Scene2Tutorial : MonoBehaviour
{
    [System.Serializable]
    public class TutorialMessage
    {
        public string title;
        [TextArea(3, 10)]
        public string message;
        public bool pauseGame = true;
        public AudioClip voiceOver;  // Added voice over support
    }

    [Header("Tutorial Messages")]
    public TutorialMessage[] tutorialMessages;
    public bool showOnlyOnce = true;
    public bool destroyAfterTrigger = true;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    private bool hasTriggered = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Set up audio source for voice overs
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;  // 2D sound for voice over
            audioSource.volume = voiceVolume;
        }

        if (TutorialManager.Instance == null)
        {
            Debug.LogWarning("TutorialManager not found in scene!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!showOnlyOnce || !hasTriggered)
            {
                if (TutorialManager.Instance != null)
                {
                    foreach (var tutorial in tutorialMessages)
                    {
                        TutorialManager.Instance.QueueMessage(
                            tutorial.title,
                            tutorial.message,
                            tutorial.voiceOver,  // Pass the voice over clip
                            tutorial.pauseGame
                        );
                    }
                    hasTriggered = true;

                    if (destroyAfterTrigger)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    // Optional: Visualize trigger area in editor
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.bounds.center - transform.position, col.bounds.size);
        }
    }
}
