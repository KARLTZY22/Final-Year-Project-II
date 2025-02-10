using UnityEngine;

public class LoadingDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Scene started: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
