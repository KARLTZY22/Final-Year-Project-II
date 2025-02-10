using UnityEngine;

public class TutorialSequenceTrigger : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSequence
    {
        public string title;
        [TextArea(3, 10)]
        public string message;
        public bool pauseGame = false;
        public AudioClip voiceOver;
    }

    [Header("Tutorial Settings")]
    public TutorialSequence[] tutorialSequences;
    public bool showOnlyOnce = true;
    public bool destroyAfterTrigger = true;

    private bool hasTriggered = false;

    private void Start()
    {
        if (TutorialManager.Instance == null)
        {
            Debug.LogWarning("TutorialManager not found in scene!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (TutorialManager.Instance == null)
        {
            Debug.LogError("TutorialManager not found when trying to trigger tutorial!", this);
            return;
        }

        if (!showOnlyOnce || !hasTriggered)
        {
            TriggerTutorialSequence();
            hasTriggered = true;

            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
    }

    private void TriggerTutorialSequence()
    {
        if (TutorialManager.Instance == null || tutorialSequences == null || tutorialSequences.Length == 0)
        {
            return;
        }

        foreach (var sequence in tutorialSequences)
        {
            TutorialManager.Instance.QueueMessage(
                sequence.title,
                sequence.message,
                sequence.voiceOver,
                sequence.pauseGame
            );
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the trigger area in the editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.bounds.center - transform.position, col.bounds.size);
        }
    }
}