using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 300f;
    [HideInInspector]
    public float currentHealth;

    [Header("Combat Settings")]
    public float jumpAttackDamage = 15f;
    public float zombieAttackDamage = 10f;
    public float attackCooldown = 4f;
    public float attackRange = 4f;
    public float detectionRange = 20f;

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Attack Hitboxes")]
    public GameObject jumpAttackHitbox;
    public GameObject zombieAttackHitbox;

    [Header("Audio")]
    public AudioClip jumpAttackSound;
    public AudioClip zombieAttackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;

    // Components
    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform player;
    private PlayerHealth playerHealth;

    // Animation hashes
    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashJumpAttack = Animator.StringToHash("JumpAttack");
    private readonly int hashZombieAttack = Animator.StringToHash("ZombieAttack");
    private readonly int hashDie = Animator.StringToHash("Die");

    // State
    private bool isDead;
    private bool isAttacking;
    private float nextAttackTime;

    private void Start()
    {
        InitializeComponents();
        currentHealth = maxHealth;
        DisableHitboxes();
    }

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
        }

        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                HandleCombat(distanceToPlayer);
            }
            else
            {
                ReturnToIdle();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent negative health

        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, audioVolume);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void HandleCombat(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            // Face player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(directionToPlayer),
                Time.deltaTime * rotationSpeed);

            if (Time.time >= nextAttackTime)
            {
                // Random attack selection
                if (Random.value > 0.5f)
                    StartJumpAttack();
                else
                    StartZombieAttack();
            }
        }
        else if (!isAttacking)
        {
            ChasePlayer();
        }
    }

    private void StartJumpAttack()
    {
        isAttacking = true;
        animator.SetTrigger(hashJumpAttack);
        if (jumpAttackSound != null)
            audioSource.PlayOneShot(jumpAttackSound, audioVolume);
        nextAttackTime = Time.time + attackCooldown;

        if (agent != null)
            agent.isStopped = true;
    }

    private void StartZombieAttack()
    {
        isAttacking = true;
        animator.SetTrigger(hashZombieAttack);
        if (zombieAttackSound != null)
            audioSource.PlayOneShot(zombieAttackSound, audioVolume);
        nextAttackTime = Time.time + attackCooldown;

        if (agent != null)
            agent.isStopped = true;
    }

    // Called via animation events
    public void EnableJumpAttackHitbox()
    {
        if (jumpAttackHitbox != null)
            jumpAttackHitbox.SetActive(true);
    }

    public void EnableZombieAttackHitbox()
    {
        if (zombieAttackHitbox != null)
            zombieAttackHitbox.SetActive(true);
    }

    public void DisableHitboxes()
    {
        if (jumpAttackHitbox != null)
            jumpAttackHitbox.SetActive(false);
        if (zombieAttackHitbox != null)
            zombieAttackHitbox.SetActive(false);

        isAttacking = false;

        if (agent != null && !isDead)
            agent.isStopped = false;
    }

    private void ChasePlayer()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool(hashIsWalking, true);
        }
    }

    private void ReturnToIdle()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetBool(hashIsWalking, false);
        }
    }

    private void Die()
    {
        isDead = true;

        animator.SetTrigger(hashDie);

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, audioVolume);
        }

        if (agent != null)
            agent.enabled = false;

        DisableHitboxes();

        // Notify Scene2Manager
        var scene2Manager = FindFirstObjectByType<Scene2Manager>();
        if (scene2Manager != null)
        {
            scene2Manager.OnBossDefeated();
        }

        enabled = false;
    }

    private void OnDrawGizmos()
    {
        // Show detection and attack ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}