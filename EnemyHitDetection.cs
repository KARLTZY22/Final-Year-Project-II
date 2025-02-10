using UnityEngine;

public class EnemyHitDetection : MonoBehaviour
{
    private EnemyController enemyController;

    private void Awake()
    {
        // Get reference to the EnemyController
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError($"Missing EnemyController on {gameObject.name}");
        }
    }

    // This method should be called by your weapon system
    public bool HandleHit(float damage, Vector3 hitPoint)
    {
        if (enemyController != null && enemyController.CanBeHit())
        {
            enemyController.TakeHit(damage, hitPoint);
            return true;
        }
        return false;
    }
}
