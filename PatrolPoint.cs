using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public float waitTime = 2f; // How long enemy should wait at this point

    private void OnDrawGizmos()
    {
        // Visual representation in editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
