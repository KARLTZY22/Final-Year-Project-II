using UnityEngine;
using TMPro;

public class ClueItem : MonoBehaviour
{
    [Header("Clue Settings")]
    public string clueTitle;
    [TextArea(3, 10)]
    public string clueContent;
    public Sprite clueIcon;

    [Header("UI Elements")]
    public TextMeshProUGUI pickupText;    // Reference to the "Press Q" text
    public float promptDistance = 3f;
    public AudioClip pickupSound;

    private bool isPlayerNear = false;
    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= promptDistance)
            {
                if (!isPlayerNear)
                {
                    isPlayerNear = true;
                    if (pickupText != null)
                        pickupText.gameObject.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    PickupClue();
                }
            }
            else if (isPlayerNear)
            {
                isPlayerNear = false;
                if (pickupText != null)
                    pickupText.gameObject.SetActive(false);
            }
        }
    }

    void PickupClue()
    {
        // Play pickup sound
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);

        // Add to clue inventory
        ClueManager.Instance.AddClue(clueTitle, clueContent, clueIcon);

        // Hide the pickup text
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        // Destroy the clue object
        Destroy(gameObject);
    }
}
