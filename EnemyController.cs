using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 15f;
    public float attackRange = 3f;
    public float attackAngle = 60f;

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;
    public float turnSpeed = 120f;
    public float patrolSpeed = 3f;
    public bool randomPatrol = true;
    public List<PatrolPoint> patrolPoints = new List<PatrolPoint>();

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageAmount = 20f;
    public float attackCooldown = 2f;
    public float hitRecoveryTime = 0.1f;

    [Header("Audio Settings")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float audioVolume = 0.7f;
    public float soundCooldown = 3f;

    // Core components
    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Collider enemyCollider;

    // Animation parameter hashes
    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashTakeHit = Animator.StringToHash("TakeHit");
    private readonly int hashDie = Animator.StringToHash("Die");

    // State tracking
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttackTime;
    private int currentPatrolIndex = -1;
    private float waitTimer = 0f;
    private bool isDead;
    private bool isAttacking;
    private bool isHit;
    private bool isWaiting;
    private bool canPlayAttackSound = true;

    private void Start()
    {
        InitializeComponents();
        currentHealth = maxHealth;
        SetupPatrolPoints();
    }

    private void InitializeComponents()
    {
        // Get required components
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        enemyCollider = GetComponent<Collider>();

        // Set up audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            ConfigureAudioSource();
        }

        // Configure NavMeshAgent
        if (agent != null)
        {
            ConfigureNavMeshAgent();
        }

        // Find and cache player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void ConfigureAudioSource()
    {
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.playOnAwake = false;
    }

    private void ConfigureNavMeshAgent()
    {
        agent.speed = patrolSpeed;
        agent.angularSpeed = turnSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.acceleration = 8f;
        agent.autoBraking = true;
    }

    private void SetupPatrolPoints()
    {
        if (patrolPoints.Count == 0)
        {
            PatrolPoint[] points = FindObjectsByType<PatrolPoint>(FindObjectsSortMode.None);
            patrolPoints.AddRange(points);
        }

        if (patrolPoints.Count > 0)
        {
            currentPatrolIndex = FindClosestPatrolPoint();
            if (agent != null)
            {
                MoveToNextPatrolPoint();
            }
        }
    }

    private int FindClosestPatrolPoint()
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] != null)
            {
                float distance = Vector3.Distance(transform.position, patrolPoints[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
        }

        return closestIndex;
    }

    private void Update()
    {
        if (isDead || isHit) return;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                agent.speed = chaseSpeed;
                HandleCombat(distanceToPlayer);
            }
            else
            {
                agent.speed = patrolSpeed;
                HandlePatrolling();
            }
        }
        else
        {
            HandlePatrolling();
        }

        UpdateAnimations();
    }

    private void HandleCombat(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle <= attackAngle * 0.5f)
            {
                if (Time.time >= nextAttackTime && !isAttacking)
                {
                    StartAttack();
                }
            }
            else
            {
                RotateTowardsPlayer(directionToPlayer);
            }
        }
        else
        {
            ChasePlayer();
        }
    }

    private void HandlePatrolling()
    {
        if (patrolPoints.Count == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolPoints[currentPatrolIndex].waitTime)
            {
                isWaiting = false;
                MoveToNextPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0) return;

        if (randomPatrol)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Count);
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }

        if (patrolPoints[currentPatrolIndex] != null)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].transform.position);
        }
    }

    private void StartAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(hashAttack);
            if (canPlayAttackSound)
            {
                PlayAttackSound();
                canPlayAttackSound = false;
                StartCoroutine(ResetSoundCooldown());
            }
        }

        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    private void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, audioVolume);
        }
    }

    private IEnumerator ResetSoundCooldown()
    {
        yield return new WaitForSeconds(soundCooldown);
        canPlayAttackSound = true;
    }

    private void RotateTowardsPlayer(Vector3 directionToPlayer)
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(directionToPlayer),
            turnSpeed * Time.deltaTime
        );
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    public void OnAttackHit()
    {
        if (!isAttacking || isDead) return;

        if (player != null && playerHealth != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }

        isAttacking = false;
    }

    public bool IsInAttackState()
    {
        return isAttacking && !isDead;
    }

    public bool CanBeHit()
    {
        return !isDead && !isHit;
    }

    public void HandleHitboxCollision(PlayerHealth target)
    {
        if (!isAttacking || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToPlayer <= attackRange)
        {
            target.TakeDamage(damageAmount);
            isAttacking = false;
        }
    }

    public void TakeHit(float damage, Vector3 hitPoint)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
        {
            animator.SetTrigger(hashTakeHit);
        }

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, audioVolume);
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartHitStagger();
    }

    private void StartHitStagger()
    {
        isHit = true;
        if (agent != null) agent.isStopped = true;
        Invoke(nameof(RecoverFromHit), hitRecoveryTime);
    }

    private void RecoverFromHit()
    {
        isHit = false;
        if (!isDead && agent != null)
        {
            agent.isStopped = false;
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger(hashDie);
        }

        if (agent != null) agent.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, audioVolume);
        }

        QuestManager questManager = FindFirstObjectByType<QuestManager>();
        if (questManager != null)
        {
            questManager.OnEnemyDefeated();
        }

        Destroy(gameObject, 3f);
    }

    private void UpdateAnimations()
    {
        if (animator != null && agent != null)
        {
            animator.SetBool(hashIsWalking, agent.velocity.magnitude > 0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection and attack ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw patrol connections
        if (patrolPoints.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Count - 1; i++)
            {
                if (patrolPoints[i] != null && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].transform.position,
                                  patrolPoints[i + 1].transform.position);
                }
            }

            // Connect last point to first
            if (patrolPoints[0] != null && patrolPoints[patrolPoints.Count - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Count - 1].transform.position,
                              patrolPoints[0].transform.position);
            }
        }

        // Draw attack angle when player is present
        if (Application.isPlaying && player != null)
        {
            Vector3 angleLeft = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward;
            Vector3 angleRight = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, angleLeft * attackRange);
            Gizmos.DrawRay(transform.position, angleRight * attackRange);
        }
    }
}
