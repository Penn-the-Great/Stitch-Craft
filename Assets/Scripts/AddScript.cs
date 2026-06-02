using UnityEngine;

public class AddScript : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private void OnGameStart()
    {
        // 1. Attach the script to the current GameObject
        Stay myNewScript = gameObject.AddComponent<Stay>();

    }
}