using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyController enemyController;

    private void Start()
    {
        // Get reference to the parent EnemyController
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemyController != null && enemyController.IsInAttackState())
        {
            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    enemyController.HandleHitboxCollision(playerHealth);
                }
            }
        }
    }
}
