using UnityEngine;

public class PersistentGameData : MonoBehaviour
{
    public static PersistentGameData Instance { get; private set; }

    public int selectedChapter = 1;

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