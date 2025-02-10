using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image healthBarFill;
    public TextMeshProUGUI bossNameText;

    [Header("Settings")]
    public float heightOffset = 2f;
    public string bossName = "Final Boss";

    [Header("Health Bar Colors")]
    public Color maxHealthColor = Color.red;
    public Color lowHealthColor = new Color(0.5f, 0f, 0f);
    public float colorChangeThreshold = 0.3f;

    private BossController boss;
    private Camera mainCamera;
    private Canvas healthCanvas;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get references
        mainCamera = Camera.main;
        boss = transform.parent.GetComponent<BossController>();
        healthCanvas = GetComponent<Canvas>();

        // Setup canvas
        if (healthCanvas != null)
        {
            healthCanvas.renderMode = RenderMode.WorldSpace;
        }

        // Initialize health display
        if (boss != null)
        {
            UpdateHealthBar(boss.currentHealth / boss.maxHealth);
            if (bossNameText != null)
            {
                bossNameText.text = bossName;
            }
        }
        else
        {
            Debug.LogWarning("BossController not found!");
        }
    }

    private void LateUpdate()
    {
        if (boss != null)
        {
            // Update position to follow boss
            transform.position = boss.transform.position + Vector3.up * heightOffset;

            // Make health bar face camera
            transform.rotation = mainCamera.transform.rotation;

            // Update health display
            float healthPercent = boss.currentHealth / boss.maxHealth;
            UpdateHealthBar(healthPercent);
        }
    }

    private void UpdateHealthBar(float healthPercent)
    {
        if (healthBarFill != null)
        {
            // Update fill amount
            healthBarFill.fillAmount = healthPercent;

            // Update color
            healthBarFill.color = Color.Lerp(lowHealthColor, maxHealthColor, healthPercent);
        }
    }
}