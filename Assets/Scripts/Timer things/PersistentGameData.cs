using UnityEngine;

public class PersistentGameData : MonoBehaviour
{
    public static PersistentGameData Instance { get; private set; }

    public int selectedChapter = 1;
    public float debugTimerSpeed = 1f;
    public int fabricAmount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
